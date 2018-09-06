namespace System.Threading
{
    using System;
#if NET45 // AsyncLocal is .NET 4.6+, so fall back to CallContext for .NET 4.5
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
#endif

#if NET45 // AsyncLocal is .NET 4.6+, so fall back to CallContext for .NET 4.5
    internal sealed class AsyncLocal<T>
    {
        private static readonly string logicalDataKey = "__AsyncLocal_" + Guid.NewGuid();

        public T Value
        {
            get
            {
                var handle = CallContext.LogicalGetData(logicalDataKey) as ObjectHandle;
                return (T) handle?.Unwrap();
            }
            set
            {
                CallContext.LogicalSetData(logicalDataKey, new ObjectHandle(value));
            }
        }
    }
#endif
}
