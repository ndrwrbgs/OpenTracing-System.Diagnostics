#define TRACE

namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing
{
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using OpenTracing.Contrib.EventHookTracer;
    using OpenTracing.Mock;
    using OpenTracing.Noop;

    public static class OpenTracingTraceSource
    {
        /// <summary>
        /// Represents a <see cref="System.Diagnostics.TraceSource"/> and a <see cref="OpenTracing.ITracer"/>
        /// where the <see cref="OpenTracingTracerSource"/> is the source and the <see cref="TraceSourceSink"/> is the sink.
        /// </summary>
        public struct TracerTraceSourcePair
        {
            public TraceSource TraceSourceSink { get; }

            /// <summary>
            /// The <see cref="OpenTracing.ITracer"/> that is hooked up to the <see cref="TraceSourceSink"/>.
            /// </summary>
            public ITracer OpenTracingTracerSource { get; }

            public TracerTraceSourcePair(TraceSource traceSourceSink, ITracer openTracingTracerSource)
            {
                this.TraceSourceSink = traceSourceSink;
                this.OpenTracingTracerSource = openTracingTracerSource;
            }
        }

        /// <summary>
        /// Creates a <see cref="TraceSource"/> that will receive trace events from a paired <see cref="ITracer"/>.
        /// </summary>
        public static TracerTraceSourcePair CreateTracerTraceSourcePair()
        {
            var traceSourceSink = new TraceSource("OpenTracing", SourceLevels.All);

            var eventHookTracer = new TraceSourceEventHookTracer(traceSourceSink);
            
            return new TracerTraceSourcePair(
                traceSourceSink,
                eventHookTracer);
        }
    }
}