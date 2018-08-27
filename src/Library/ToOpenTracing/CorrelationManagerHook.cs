namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;

    /// <summary>
    /// Hooks into <see cref="CorrelationManager"/> to observe the <see cref="System.Diagnostics"/> calls that
    /// represent context in `.NET Framework`
    /// </summary>
    public static class CorrelationManagerHook
    {
        /// <exception cref="NotImplementedException">As this relies on internals of the BCL, this exception is thrown if the current runtime is detected to not be supported.</exception>
        /// <exception cref="NotSupportedException">Thrown if the current <see cref="CorrelationManager.LogicalOperationStack"/> has values pushed onto it before this method is called.</exception>
        public static void PipeCorrelationManagerToOpenTracing()
        {
            const string transactionSlotFieldName = "transactionSlotName";
            var transactionSlotField = typeof(CorrelationManager)
                .GetField(transactionSlotFieldName, BindingFlags.Static | BindingFlags.NonPublic);

            if (transactionSlotField == null)
            {
                throw new NotImplementedException(
                    $"Library does not support the current runtime - could not resolve the private field {nameof(CorrelationManager)}.{transactionSlotFieldName}");
            }

            var transactionSlotName = transactionSlotField.GetValue(null /* because it's static */) as string;
            if (transactionSlotName == null)
            {
                // This should not be possible, but being clear with exceptional cases nonetheless
                throw new NotImplementedException(
                    $"Library does not support the current runtime - could not find a (string) value for {nameof(CorrelationManager)}.{transactionSlotFieldName}");
            }

            var newStack = CreateOpenTracingOperationStack(transactionSlotName);

            CallContext.LogicalSetData(transactionSlotName, newStack);
        }

        private static OpenTracingOperationStack CreateOpenTracingOperationStack(string transactionSlotName)
        {
            // Check for anything existing on the stack
            var currentStackObject = CallContext.LogicalGetData(transactionSlotName);
            if (currentStackObject == null)
            {
                // No current stack
                return new OpenTracingOperationStack();
            }

            // Handle the existing Stack
            var currentStack = currentStackObject as Stack;
            if (currentStack == null)
            {
                throw new NotImplementedException(
                    $"Library does not support the current runtime - {nameof(CorrelationManager)}'s internals do not match expectations, CallContext's {transactionSlotName} is currently a {currentStackObject.GetType()} but we expected a {typeof(Stack)}");
            }

            // We do not copy over the Stack presently because it complicates what happens on Pop for items that were already there.
            throw new NotSupportedException(
                $"You must ensure that {nameof(PipeCorrelationManagerToOpenTracing)} is called before any items are pushed onto the {nameof(CorrelationManager)}.{nameof(CorrelationManager.LogicalOperationStack)} (e.g. via {nameof(CorrelationManager)}.{nameof(CorrelationManager.StartLogicalOperation)}");
        }
    }
}