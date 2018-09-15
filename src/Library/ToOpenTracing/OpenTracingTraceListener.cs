namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Remoting;
    using System.Text;
    using JetBrains.Annotations;
    using OpenTracing.Util;

    /// <summary>
    /// Send Trace.X events to OT
    /// </summary>
    [PublicAPI]
    public sealed class OpenTracingTraceListener : TraceListener
    {
        public OpenTracingTraceListener()
            : base("OpenTracingTraceListener")
        {
        }

        /// <summary>
        /// Not needed, could just set null, but users may get confused about isWriteWithoutNewLine:null
        /// </summary>
        private static readonly object trueAsObject = true;
        
        public override void Write(string message)
        {
            // Because it's temporal tracing, when you wrote to the line matters. It'll make it harder to read, but that's the scenario.
            // This strongly suggests we shouldn't be listening to OpenTracing
            GlobalTracer.Instance.ActiveSpan
                // TODO: Pool arrays
                .Log(new[]
                {
                    new KeyValuePair<string, object>(Constants.IsWriteWithoutNewline, trueAsObject),
                    new KeyValuePair<string, object>(Constants.EventLogKey, message),
                });
        }

        public override void WriteLine(string message)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(message);
        }

        public static class Constants
        {
            public const string EventLogKey = "event";
            public const string LevelLogKey = "traceLevel";
            public const string IsWriteWithoutNewline = "isWriteWithoutNewline";

            public static string GetTraceDataLogKeyForIndex(int i)
            {
                return "data." + i;
            }
        }

        private void WriteLine(TraceEventType eventType, string message)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(Constants.EventLogKey, message),
                    new KeyValuePair<string, object>(Constants.LevelLogKey, eventType),
                });
        }

        private void WriteLine(TraceEventType eventType, object o)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    // TODO: Should sending an object also go to 'event'? 'Event' is usually string. Check semantics for options.
                    new KeyValuePair<string, object>(Constants.EventLogKey, o),
                    new KeyValuePair<string, object>(Constants.LevelLogKey, eventType),
                });
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, message))
                return;
            this.WriteLine(eventType, message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            this.TraceEvent(eventCache, source, eventType, id, string.Empty);
        }

        public override void TraceEvent(TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string format,
            params object[] args)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, format, args))
                return;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse Can be null since it's explicitly passed
            if (args != null)
                this.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, format, args));
            else
                this.WriteLine(format);
        }

        public override void Write(object o)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace((TraceEventCache) null, "", TraceEventType.Verbose, 0, (string) null, (object[]) null, o) || o == null)
                return;
            GlobalTracer.Instance.ActiveSpan
                // TODO: Pool arrays
                .Log(new[]
                {
                    new KeyValuePair<string, object>(Constants.IsWriteWithoutNewline, trueAsObject),
                    // TODO: Should sending an object also go to 'event'? 'Event' is usually string. Check semantics for options.
                    new KeyValuePair<string, object>(Constants.EventLogKey, o),
                });
        }

        public override void Write(string message, string category)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace((TraceEventCache) null, "", TraceEventType.Verbose, 0, message))
                return;

            GlobalTracer.Instance.ActiveSpan
                // TODO: Pool arrays
                .Log(new[]
                {
                    new KeyValuePair<string, object>(Constants.IsWriteWithoutNewline, trueAsObject),
                    new KeyValuePair<string, object>(Constants.EventLogKey, message),
                    new KeyValuePair<string, object>(nameof(category), category),
                });
        }

        public override void Write(object o, string category)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace((TraceEventCache) null, "", TraceEventType.Verbose, 0, category, (object[]) null, o))
                return;
            GlobalTracer.Instance.ActiveSpan
                // TODO: Pool arrays
                .Log(new[]
                {
                    new KeyValuePair<string, object>(Constants.IsWriteWithoutNewline, trueAsObject),
                    // TODO: Should sending an object also go to 'event'? 'Event' is usually string. Check semantics for options.
                    new KeyValuePair<string, object>(Constants.EventLogKey, o),
                    new KeyValuePair<string, object>(nameof(category), category),
                });
        }

        protected override void WriteIndent()
        {
            // Do nothing for indents, should not be needed in tracing
        }

        public override void WriteLine(object o)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace((TraceEventCache) null, "", TraceEventType.Verbose, 0, (string) null, (object[]) null, o))
                return;
            GlobalTracer.Instance.ActiveSpan
                // TODO: Pool arrays
                .Log(new[]
                {
                    // TODO: Should sending an object also go to 'event'? 'Event' is usually string. Check semantics for options.
                    new KeyValuePair<string, object>(Constants.EventLogKey, o),
                });
        }

        public override void WriteLine(string message, string category)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace((TraceEventCache) null, "", TraceEventType.Verbose, 0, message))
                return;
            GlobalTracer.Instance.ActiveSpan
                // TODO: Pool arrays
                .Log(new[]
                {
                    new KeyValuePair<string, object>(Constants.EventLogKey, message),
                    new KeyValuePair<string, object>(nameof(category), category),
                });
        }

        public override void WriteLine(object o, string category)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace((TraceEventCache) null, "", TraceEventType.Verbose, 0, category, (object[]) null, o))
                return;
            GlobalTracer.Instance.ActiveSpan
                // TODO: Pool arrays
                .Log(new[]
                {
                    // TODO: Should sending an object also go to 'event'? 'Event' is usually string. Check semantics for options.
                    new KeyValuePair<string, object>(Constants.EventLogKey, o),
                    new KeyValuePair<string, object>(nameof(category), category),
                });
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, (string) null, (object[]) null, data))
                return;
            this.WriteLine(eventType, data);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, (string) null, (object[]) null, (object) null, data))
                return;

            
            KeyValuePair<string, object>[] fields = new KeyValuePair<string, object>[data.Length + 1];
            fields[0] = new KeyValuePair<string, object>(Constants.LevelLogKey, eventType);
            
            for (var i = 0; i < data.Length; i++)
            {
                var key = Constants.GetTraceDataLogKeyForIndex(i);
                fields[i + 1] = new KeyValuePair<string, object>(key, data[i]);
            }
            GlobalTracer.Instance.ActiveSpan
                .Log(fields);
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            // TODO: Unsure what to do here, if they didn't use the CorrelationManager construct to do this transfer.
            if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, TraceEventType.Transfer, id, message))
                return;
            GlobalTracer.Instance.ActiveSpan
                // TODO: Pool arrays
                .Log(new[]
                {
                    new KeyValuePair<string, object>(Constants.EventLogKey, message),
                    new KeyValuePair<string, object>(nameof(relatedActivityId), relatedActivityId),
                    new KeyValuePair<string, object>(Constants.LevelLogKey, TraceEventType.Transfer), 
                });
        }
    }
}