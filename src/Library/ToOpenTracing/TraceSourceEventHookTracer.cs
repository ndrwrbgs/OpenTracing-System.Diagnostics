using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing
{
    using System.Diagnostics;
    using System.Threading;
    using OpenTracing.Contrib.EventHookTracer;
    using OpenTracing.Mock;
    using OpenTracing.Propagation;

    internal sealed class TraceSourceEventHookTracer : ITracer
    {
        public const string VectorClockLogKeyName = "vectorClock";

        private readonly TraceSource traceSourceSink;

        private readonly AsyncLocal<Stack<string>> curOpNameStack = new AsyncLocal<Stack<string>>
            {Value = new Stack<string>()};

        private readonly VectorClock vectorClock = new VectorClock();
        private readonly ITracer tracerImplementation;

        #region Clock manipulation

        private void ExtendClock()
        {
            this.vectorClock.Extend();
            //this.nextVectorClock.Value += ".1";
        }

        private string CurrentClock()
        {
            return this.vectorClock.Value;
            //return this.nextVectorClock.Value.Substring(0,
            //    this.nextVectorClock.Value.LastIndexOf(".") >= 0 ? this.nextVectorClock.Value.LastIndexOf(".") : 0);
        }

        private void PopAndIncrementClock()
        {
            this.vectorClock.Pop();
            //var cur = CurrentClock();
            //var lastPartOfCurrentClock = cur.Substring(
            //    cur.LastIndexOf(".") >= 0
            //        ? (cur.LastIndexOf(".") + 1)
            //        : 0);
            //var toTrim = lastPartOfCurrentClock.Length == cur.Length
            //    ? lastPartOfCurrentClock.Length
            //    : lastPartOfCurrentClock.Length + 1;
            //var lastIntOfCurrentClock = int.Parse(lastPartOfCurrentClock);
            //nextVectorClock.Value = cur.Substring(0, cur.Length - toTrim) + "." + (lastIntOfCurrentClock + 1);
        }

        #endregion

        #region EventHandlers

        private void OnEventHookTracerOnSpanFinished(object sender, EventHookTracer.SpanLifecycleEventArgs span)
        {
            // Need to copy to truly be AsyncLocal
            this.curOpNameStack.Value = new Stack<string>(this.curOpNameStack.Value.Reverse());
            var previousSpan = this.curOpNameStack.Value.Pop();

            if (!string.Equals(previousSpan, span.OperationName))
            {
                throw new InvalidOperationException(
                    "Code error - you finished a span that was not the currently active one");
            }

            this.traceSourceSink.TraceData(
                TraceEventType.Stop,
                2,
                new Dictionary<string, object>
                {
                    [VectorClockLogKeyName] = this.CurrentClock(),
                    [nameof(span.OperationName)] = span.OperationName
                });
            this.PopAndIncrementClock();
        }

        private void OnEventHookTracerOnSpanLog(object sender, EventHookTracer.LogEventArgs args)
        {
            var curOpName = this.curOpNameStack.Value.Peek();

            var dict = new Dictionary<string, object>();
            dict[VectorClockLogKeyName] = this.CurrentClock();
            //dict[nameof(EventHookTracer.SpanLifecycleEventArgs.OperationName)] = curOpName;

            foreach (var keyValuePair in args.Fields)
            {
                dict.Add(keyValuePair.Key, keyValuePair.Value);
            }

            traceSourceSink.TraceData(TraceEventType.Information, 3, dict);
        }

        private void OnEventHookTracerOnSpanSetTag(object sender, EventHookTracer.SetTagEventArgs args)
        {
            var curOpName = this.curOpNameStack.Value.Peek();

            traceSourceSink.TraceData(TraceEventType.Information, 4,
                new Dictionary<string, object>
                {
                    [VectorClockLogKeyName] = this.CurrentClock(), [args.Key] = args.Value,
                    //[nameof(EventHookTracer.SpanLifecycleEventArgs.OperationName)] = curOpName
                });
        }

        private void OnEventHookTracerOnSpanActivated(object sender, EventHookTracer.SpanLifecycleEventArgs span)
        {
            // Need to copy to truly be AsyncLocal
            this.curOpNameStack.Value = new Stack<string>(this.curOpNameStack.Value.Reverse());
            this.curOpNameStack.Value.Push(span.OperationName);
            this.ExtendClock();
            
            this.traceSourceSink.TraceData(
                TraceEventType.Start,
                1,
                new Dictionary<string, object>
                {
                    [VectorClockLogKeyName] = this.CurrentClock(),
                    [nameof(span.OperationName)] = span.OperationName
                });
        }

        #endregion

        public TraceSourceEventHookTracer(TraceSource traceSourceSink)
        {
            this.traceSourceSink = traceSourceSink;
            var eventHookTracer = new EventHookTracer(
                // We want something that actually keeps track of the current span.
                // TODO: Pending work in the EventHookTracer project, we could remove this.
                new MockTracer());

            eventHookTracer.SpanActivated += this.OnEventHookTracerOnSpanActivated;
            eventHookTracer.SpanFinished += this.OnEventHookTracerOnSpanFinished;
            eventHookTracer.SpanLog += this.OnEventHookTracerOnSpanLog;
            eventHookTracer.SpanSetTag += this.OnEventHookTracerOnSpanSetTag;

            this.tracerImplementation = eventHookTracer;
        }

        #region ITracer

        ISpanBuilder ITracer.BuildSpan(string operationName)
        {
            return this.tracerImplementation.BuildSpan(operationName);
        }

        void ITracer.Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            this.tracerImplementation.Inject(spanContext, format, carrier);
        }

        ISpanContext ITracer.Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            return this.tracerImplementation.Extract(format, carrier);
        }

        IScopeManager ITracer.ScopeManager => this.tracerImplementation.ScopeManager;

        ISpan ITracer.ActiveSpan => this.tracerImplementation.ActiveSpan;

        #endregion
    }
}