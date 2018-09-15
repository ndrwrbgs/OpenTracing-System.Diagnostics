namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing
{
    using System.Diagnostics;

    internal static class TraceFilterExtensionsForCopyingFromBcl
    {
        internal static bool ShouldTrace(
            this TraceFilter @this,
            TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage)
        {
            return @this.ShouldTrace(cache, source, eventType, id, formatOrMessage, (object[]) null, (object) null, (object[]) null);
        }

        internal static bool ShouldTrace(
            this TraceFilter @this,TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args)
        {
            return @this.ShouldTrace(cache, source, eventType, id, formatOrMessage, args, (object) null, (object[]) null);
        }

        internal static bool ShouldTrace(
            this TraceFilter @this,TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1)
        {
            return @this.ShouldTrace(cache, source, eventType, id, formatOrMessage, args, data1, (object[]) null);
        }
    }
}