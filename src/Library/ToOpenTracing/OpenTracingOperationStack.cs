namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing
{
    using System.Collections;
    using OpenTracing.Util;

    internal sealed class OpenTracingOperationStack : Stack
    {
        public OpenTracingOperationStack()
        {
        }

        public OpenTracingOperationStack(int initialCapacity)
            : base(initialCapacity)
        {
        }
        
        public override object Pop()
        {
            var val = base.Pop();
            GlobalTracer.Instance.ScopeManager.Active.Dispose();
            return val;
        }

        public override void Push(object operationId)
        {
            var valueAsString = operationId as string ?? operationId?.ToString() ?? "null operationId";

            // Note: we intentionally do not dispose this IDisposable. It will be handled by OnPop.
            var span = GlobalTracer.Instance.BuildSpan(valueAsString)
                .StartActive(finishSpanOnDispose: true);

            base.Push(operationId);
        }
    }
}