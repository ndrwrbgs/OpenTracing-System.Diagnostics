using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing
{
    using System.Diagnostics;
    using System.IO;
    using OpenTracing.Util;
    using Test;

    public static class LegacyCodeHelper
    {
        /// <summary>
        /// Exposed if using customer TraceSource's that are not tied to <see cref="Trace"/>.
        /// Customers pipe data INTO this
        /// </summary>
        internal static TraceListener OpenTracingTraceListener;

        /// <summary>
        /// Exposed if using TraceSource output sink.
        /// Customers read data OUT OF this
        /// </summary>
        public static TraceSource OpenTracingTraceSource;

        public static void EnableTracingForLegacyCode(
            LegacyFeaturesUsed flags,
            OutputSinks output)
        {
            if (flags.HasFlag(LegacyFeaturesUsed.CorrelationManager))
            {
                CorrelationManagerHook.PipeCorrelationManagerToOpenTracing();
            }

            var rawConsoleOut = Console.Out;
            if (flags.HasFlag(LegacyFeaturesUsed.ConsoleOut))
            {
                // Intercept Console.Out messages (this is a source)
                Console.SetOut(new OpenTracingTextWriter(
                    // We do NOT set to Console.Out, so that we ensure all log messages go through us
                    textWriterImplementation: TextWriter.Null));
            }

            // Start output
            if (output.HasFlag(OutputSinks.TraceSource))
            {
                // Create an ITracer/TraceSource pair for getting output FROM OpenTracing (this is a sink)
                var pair = ToOpenTracing.OpenTracingTraceSource.CreateTracerTraceSourcePair();
                var traceSourceSink = pair.TraceSourceSink;
                var tracerSource = pair.OpenTracingTracerSource;
                // Tell OT to write-to/actually-use that sink
                GlobalTracer.Register(tracerSource);

                OpenTracingTraceSource = traceSourceSink;
            }

            // Because it reads Trace.Listeners, must be before any writes to that.
            if (output.HasFlag(OutputSinks.CopyExistingTraceListeners))
            {
                if (!output.HasFlag(OutputSinks.TraceSource))
                {
                    throw new InvalidOperationException(
                        "Presently, you must pipe out to TraceSource to be able to get to Trace.Write, since that's all we've implemented.");
                }

                // No longer need to validate, since we write AFTER reading
                //if (flags.HasFlag(LegacyFeaturesUsed.TraceWrite))
                //{
                //    throw new InvalidOperationException(
                //        "Catching Trace.Write and piping output to Trace.Write would be cyclic");
                //}

                OpenTracingTraceSource.Listeners.AddRange(
                    Trace.Listeners);
            }

            if (flags.HasFlag(LegacyFeaturesUsed.TraceWrite))
            {
                OpenTracingTraceListener = new OpenTracingTraceListener();
                // Send Trace messages to OpenTracing (this is a source)
                Trace.Listeners.Add(OpenTracingTraceListener);
            }

            if (output.HasFlag(OutputSinks.ColoredConsole))
            {
                if (!output.HasFlag(OutputSinks.TraceSource))
                {
                    throw new InvalidOperationException(
                        "Presently, you must pipe out to TraceSource to be able to get to Console, since that's all we've implemented.");
                }

                var listener = new JsonDataConsoleTraceListener(rawConsoleOut);
                OpenTracingTraceSource.Listeners.Add(listener);
            }

            if (flags.HasFlag(LegacyFeaturesUsed.NoTracingSetup))
            {
                if (!flags.HasFlag(LegacyFeaturesUsed.ConsoleOut)
                    && !flags.HasFlag(LegacyFeaturesUsed.TraceWrite))
                {
                    throw new ArgumentException(
                        "No need to set the NoTracingSetup flag if not using any features that will output to the active span.");
                }

                // Explicitly NOT disposing of the IScope, we want it to remain forever active
                var scope = GlobalTracer.Instance
                    .BuildSpan("GlobalSpan")
                    .StartActive();
            }
        }

        [Flags]
        public enum LegacyFeaturesUsed
        {
            None = 0,

            /// <summary>
            /// Captures Console.Out messages and sends them to GlobalTracer
            /// </summary>
            ConsoleOut = 1 << 0,

            /// <summary>
            /// Captures Trace.Write messages and sends them to GlobalTracer
            /// </summary>
            TraceWrite = 1 << 1,

            /// <summary>
            /// Captures Trace.CorrelationManager.* messages and sends them to GlobalTracer
            /// </summary>
            CorrelationManager = 1 << 2,

            /// <summary>
            /// Signals that the implementing app will use the above features before/without
            /// opening any Spans. (e.g. ActiveSpan may be null).
            /// -
            /// As a work around, we open a singular global span for this case.
            /// WARNING for apps that are long lived doing this. Make sure you know what you're
            /// doing (though, to be fair, the risks that are involved wouldn't hurt your app
            /// until after quite a long time of running)
            /// -
            /// NOTE this should not be called from a nested CallContext. It expects to be top level. Thanks!
            /// TODO: Can we confirm this?
            /// </summary>
            NoTracingSetup = 1 << 3,
        }

        [Flags]
        public enum OutputSinks
        {
            None = 0,

            /// <summary>
            /// Outputs OpenTracing messages to a Colored Console.Out implementation.
            /// .
            /// The console output requires the TraceSource hook right now.
            /// </summary>
            ColoredConsole = 1 << 0 | TraceSource,

            /// <summary>
            /// Outputs OpenTracing messages to <see cref="LegacyCodeHelper.OpenTracingTraceSource"/>
            /// for whatever listening purposes the user has
            /// </summary>
            TraceSource = 1 << 1,

            /// <summary>
            /// Outputs OpenTracing messages to <see cref="Trace.Listeners"/>
            /// TODO: This should catch updates given to Trace.Listeners, but right now snapshots. When implementing the
            ///       proper solution, rename to TraceWrite[Line]
            ///     We cannot actually catch updates to Trace.Listeners in the current BCL - one work
            ///     around would be to have an OT -> Trace.Write plugin, but it would have to be sure
            ///     NOT to infinite loop in the Trace.Write -captureTraceWrite-> OT -sendToAllListeners-> Trace.Write cycle
            /// </summary>
            CopyExistingTraceListeners = 1 << 2 | TraceSource,
        }
    }
}
