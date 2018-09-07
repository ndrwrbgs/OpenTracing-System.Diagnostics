#define TRACE

namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing
{
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using OpenTracing.Contrib.EventHookTracer;
    using OpenTracing.Mock;
    using OpenTracing.Noop;
    using OpenTracing.Util;

    public static class OpenTracingTraceSource
    {
        /// <summary>
        /// Represents a <see cref="System.Diagnostics.TraceSource"/> and a <see cref="OpenTracing.ITracer"/>
        /// where the <see cref="TracerSource"/> is the source and the <see cref="TraceSourceSink"/> is the sink.
        /// </summary>
        public struct TracerTraceSourcePair
        {
            public TraceSource TraceSourceSink { get; }

            /// <summary>
            /// The <see cref="OpenTracing.ITracer"/> that is hooked up to the <see cref="TraceSourceSink"/>.
            /// </summary>
            public ITracer TracerSource { get; }

            public TracerTraceSourcePair(TraceSource traceSourceSink, ITracer tracerSource)
            {
                this.TraceSourceSink = traceSourceSink;
                this.TracerSource = tracerSource;
            }
        }

        /// <summary>
        /// Creates a <see cref="TraceSource"/> that will receive trace events from a paired <see cref="ITracer"/>.
        /// </summary>
        public static TracerTraceSourcePair CreateTracerTraceSourcePair()
        {
            var traceSourceSink = new TraceSource("OpenTracing", SourceLevels.All);

            AsyncLocal<Stack<string>> curOpNameStack = new AsyncLocal<Stack<string>> {Value = new Stack<string>()};
            AsyncLocal<string> nextVectorClock = new AsyncLocal<string>();
            nextVectorClock.Value = "1";

            void ExtendClock() => nextVectorClock.Value += ".1";
            string CurrentClock() => nextVectorClock.Value.Substring(0, nextVectorClock.Value.LastIndexOf(".") >= 0 ? nextVectorClock.Value.LastIndexOf(".") : 0);
            void PopAndIncrementClock()
            {
                var cur = CurrentClock();
                var lastPartOfCurrentClock = cur.Substring(
                    cur.LastIndexOf(".") >= 0
                        ? (cur.LastIndexOf(".") + 1)
                        : 0);
                var toTrim = lastPartOfCurrentClock.Length == cur.Length
                    ? lastPartOfCurrentClock.Length
                    : lastPartOfCurrentClock.Length + 1;
                var lastIntOfCurrentClock = int.Parse(lastPartOfCurrentClock);
                nextVectorClock.Value = cur.Substring(0, cur.Length - toTrim) + "." + (lastIntOfCurrentClock + 1);
            }

            var eventHookTracer = new EventHookTracer(new MockTracer());
            eventHookTracer.SpanActivated += (sender, span) =>
            {
                // Need to copy to truly be AsyncLocal
                curOpNameStack.Value = new Stack<string>(curOpNameStack.Value.Reverse());
                curOpNameStack.Value.Push(span.OperationName);
                ExtendClock();

                traceSourceSink.TraceEvent(TraceEventType.Start, 1, "{1} Starting {0}", span.OperationName, CurrentClock());
            };
            eventHookTracer.SpanFinished += (sender, span) =>
            {
                // Need to copy to truly be AsyncLocal
                curOpNameStack.Value = new Stack<string>(curOpNameStack.Value.Reverse());
                var previousSpan = curOpNameStack.Value.Pop();
                
                if (!string.Equals(previousSpan, span.OperationName))
                {
                    throw new InvalidOperationException("Code error - you finished a span that was not the currently active one");
                }

                traceSourceSink.TraceEvent(TraceEventType.Stop, 2, "{1} Finished {0}", span.OperationName, CurrentClock());
                PopAndIncrementClock();
            };
            eventHookTracer.SpanLog += (sender, args) =>
            {
                var dict = new Dictionary<string, object>();
                dict["vectorClock"] = CurrentClock();

                foreach (var keyValuePair in args.Fields)
                {
                    dict.Add(keyValuePair.Key, keyValuePair.Value);
                }

                traceSourceSink.TraceData(TraceEventType.Information, 3, dict);
            };
            eventHookTracer.SpanSetTag += (sender, args) =>
            {
                var curOpName = curOpNameStack.Value.Peek();
                
                traceSourceSink.TraceData(TraceEventType.Information, 4, new Dictionary<string, object>
                {
                    ["vectorClock"] = CurrentClock(),
                    [args.Key] = args.Value
                });
            };

            return new TracerTraceSourcePair(
                traceSourceSink,
                eventHookTracer);
        }
    }

    public sealed class OpenTracingTraceListener : TraceListener
    {
        private readonly StringBuilder currentLine = new StringBuilder();

        public OpenTracingTraceListener()
            : base("OpenTracingTraceListener")
        {
        }

        public override void Write(string message)
        {
            this.currentLine.Append(message);
        }

        public override void WriteLine(string message)
        {
            if (this.currentLine.Length > 0)
            {
                GlobalTracer.Instance.ActiveSpan
                    .Log(this.currentLine.ToString() + message);
                this.currentLine.Clear();
            }
            else
            {
                GlobalTracer.Instance.ActiveSpan
                    .Log(message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.Flush();
            base.Dispose(disposing);
        }

        public override void Close()
        {
            this.Flush();
            base.Close();
        }

        public override void Flush()
        {
            base.Flush();

            if (this.currentLine.Length > 0)
            {
                GlobalTracer.Instance.ActiveSpan
                    .Log(this.currentLine.ToString());
                this.currentLine.Clear();
            }
        }
    }
}