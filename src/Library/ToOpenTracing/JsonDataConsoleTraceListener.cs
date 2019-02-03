namespace Test
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    // TODO: Don't want this dependency in this project
    using Newtonsoft.Json;
    using OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing;
    using OpenTracing.Util;

    /// <summary>
    /// Typically, user code will implement such a thing. This is included for debugging.
    /// </summary>
    public class JsonDataConsoleTraceListener : TextWriterTraceListener
    {
        private static class SpecialConstants
        {
            // TODO: key in TraceSourceEventHookTracer must be exposed
            public const string VectorClock = "vectorClock";
            public const string OperationName = "OperationName";

            public const string Category = "category";

            // TODO: Should be exposed also
            public const string RelatedActivityId = "relatedActivityId";
        }

        public JsonDataConsoleTraceListener(TextWriter consoleOut)
            : base(consoleOut)
        {
        }
            
        // (indirectly) OnEventHookTracerOnSpanActivated
        // (indirectly) OnEventHookTracerOnSpanFinished
        // (indirectly) OnEventHookTracerOnSpanLog
        // (indirectly) OnEventHookTracerOnSpanSetTag
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            var dictionary = data as IDictionary<string, object>;
            if (dictionary == null)
            {
                var enumerable = data as IEnumerable<KeyValuePair<string, object>>;
                if (enumerable == null)
                {
                    // TODO: Should we throw? We don't recognize this what do we do with it?
                    // TODO: Do we just ToString it and log it?
                    // TODO: Do we delegate to the underlying TraceData output
                    base.TraceEvent(eventCache, source, TraceEventType.Error, 998,
                        "Unrecognized captured TraceData call. Payload: " +
                        JsonConvert.SerializeObject(data,
                            new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));
                    base.TraceData(eventCache, source, eventType, id, data);
                    return;
                }

                dictionary = enumerable.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            object vectorClockObj;
            dictionary.TryGetValue(SpecialConstants.VectorClock, out vectorClockObj);
            string vectorClock = vectorClockObj?.ToString();
                
            object operationNameObj;
            dictionary.TryGetValue(SpecialConstants.OperationName, out operationNameObj);
            string operationName = operationNameObj?.ToString();

            switch (id)
            {
                // OnEventHookTracerOnSpanActivated
                case 1:
                // These two are mostly the same, presently.
                // OnEventHookTracerOnSpanFinished
                case 2:
                    this.TraceSpanLifecycleEvent(vectorClock, operationName, eventType);

                    break;
                // OnEventHookTracerOnSpanLog
                case 3:
                    object eventObj;
                    dictionary.TryGetValue(OpenTracingTraceListener.Constants.EventLogKey, out eventObj);
                    string eventText = eventObj?.ToString();

                    object traceLevelObj;
                    dictionary.TryGetValue(OpenTracingTraceListener.Constants.LevelLogKey, out traceLevelObj);
                    TraceEventType traceLevel = traceLevelObj == null
                        ? eventType
                        : (TraceEventType) Enum.Parse(typeof(TraceEventType), traceLevelObj?.ToString());

                    object isWriteObj;
                    bool isWrite = dictionary.TryGetValue(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, out isWriteObj);

                    object categoryObj;
                    dictionary.TryGetValue(SpecialConstants.Category, out categoryObj);
                    string category = categoryObj?.ToString();

                    this.TraceLog(vectorClock, traceLevel, eventText, isWrite, category, dictionary, operationName,
                        eventCache, source, id);

                    break;
                // OnEventHookTracerOnSpanSetTag
                case 4:
                    // Arbitrary keys
                    this.TraceSetTags(vectorClock, eventType, dictionary, operationName,
                        eventCache, source, id);
                    break;
            }

            //base.TraceData(
            //    eventCache,
            //    source,
            //    eventType,
            //    id,
            //    JsonConvert.SerializeObject(data));
        }

        // TODO: We want to expose this as an abstract base class, to get hands off the formatting -- Hmm if we do that it wouldn't need to be a text writer anymore, they'd extend it to write to Console/File (and could then use colors)
        private void TraceLog(
            string vectorClock,
            TraceEventType traceLevel,
            string eventText,
            bool isWrite,
            string category,
            IDictionary<string, object> dictionary,
            string operationName,
            // Below this line, for passing on
            TraceEventCache eventCache,
            string source,
            int id)
        {
            var ignore = new[]
            {
                SpecialConstants.VectorClock, SpecialConstants.Category,
                SpecialConstants.OperationName, OpenTracingTraceListener.Constants.EventLogKey,
                OpenTracingTraceListener.Constants.IsWriteWithoutNewline, OpenTracingTraceListener.Constants.LevelLogKey
            };
            var pruned = dictionary
                .Where(kvp => !ignore.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            StringBuilder builder = new StringBuilder();
            builder.Append($"{ToLogString(traceLevel)} | ");

            if (!String.IsNullOrEmpty(category))
            {
                builder.Append($"{category} | ");
            }

            if (!String.IsNullOrEmpty(eventText))
            {
                builder.Append($"{eventText} ");
            }

            if (pruned.Any())
            {
                builder.Append($"Payload: {JsonConvert.SerializeObject(pruned)} ");
            }

            // TODO: Keep some kind of queue based on the vectorClock to be able to retrieve partial line writes for easier reading
            // e.g. output I, then ' am' would do
            // Information | I (cont...)
            // Information | I am (cont..)
            if (isWrite)
            {
                builder.Append($"(cont...)");
            }
                
            this.WriteLine(vectorClock, builder.ToString(), traceLevel);
        }

        private static string ToLogString(TraceEventType type)
        {
            switch (type)
            {
                case TraceEventType.Critical:
                    return "Crit";
                    break;
                case TraceEventType.Error:
                    return "Err ";
                    break;
                case TraceEventType.Warning:
                    return "Warn";
                    break;
                case TraceEventType.Information:
                    return "Info";
                    break;
                case TraceEventType.Verbose:
                    return "Verb";
                    break;
                case TraceEventType.Start:
                    return "Strt";
                    break;
                case TraceEventType.Stop:
                    return "Stop";
                    break;
                case TraceEventType.Suspend:
                    return "Spnd";
                    break;
                case TraceEventType.Resume:
                    return "Rsme";
                    break;
                case TraceEventType.Transfer:
                    return "Tran";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void TraceSetTags(
            string vectorClock,
            TraceEventType eventType,
            IDictionary<string, object> data,
            string operationName,
            // Below this line, for passing on
            TraceEventCache eventCache,
            string source,
            int id)
        {
            var ignore = new[] {SpecialConstants.VectorClock};
            var pruned = data
                .Where(kvp => !ignore.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // TODO: This will always be 1, debounce and group tags for better output?
            if (pruned.Count == 1)
            {
                var kvp = pruned.Single();
                this.WriteLine(vectorClock, $"Set tag | \"{kvp.Key}\":\"{kvp.Value}\"", TraceEventType.Verbose);
            }else

                this.WriteLine(
                    vectorClock,
                    $"Set tags:\r\n{JsonConvert.SerializeObject(pruned, Formatting.Indented)}", TraceEventType.Verbose);
        }

        private void TraceSpanLifecycleEvent(
            string vectorClock,
            string operationName,
            TraceEventType eventType)
        {
            this.WriteLine(
                vectorClock,
                eventType == TraceEventType.Start
                    ? $"Span '{operationName}' starting"
                    : $"Span '{operationName}' finished", eventType);
        }

        private int longest = 0;

        private void WriteLine(string vectorClock, string message, TraceEventType level)
        {
            Interlocked.CompareExchange(ref this.longest, Math.Max(this.longest, vectorClock.Length), this.longest);

            // Indent newlines
            message = message.Replace("\n", "\n\t");

            // TODO: Asynchronous output
            // TODO: Can use a reader writer lock to avoid locking when values are the same
            lock (this)
            {
                var prevBack = Console.BackgroundColor;
                var prevFore = Console.ForegroundColor;
                bool changedBack = false;
                bool changedFore = false;

                changedFore = true;
                Console.ForegroundColor = this.colorProvider.GetColor(level, vectorClock);

                base.WriteLine($"{vectorClock.PadRight(this.longest)} | {message}");

                if (changedFore)
                {
                    Console.ForegroundColor = prevFore;
                }

                if (changedBack)
                {
                    Console.BackgroundColor = prevBack;
                }
            }
        }

        private readonly IColorProvider colorProvider = new ByLogType();

        public override void WriteLine(string message)
        {
            /* this method is only to suppress the header writes */

            if (message.StartsWith(this.Name))
            {
                // Ignore it, it's a Header
                return;
            }

            base.WriteLine(message);
        }
            
        internal interface IColorProvider
        {
            ConsoleColor GetColor(TraceEventType type, string vectorClock);
        }

        /// <summary>
        /// Reproducible
        /// </summary>
        private class ReproducibleByClock : IColorProvider
        {
            public ConsoleColor GetColor(TraceEventType type, string vectorClock)
            {
                var valid = new[]
                {
                    ///// <summary>The color gray.</summary>
                    //Gray,
                    ///// <summary>The color dark gray.</summary>
                    //DarkGray,
                    ///// <summary>The color blue.</summary>
                    //Blue,
                    ///// <summary>The color green.</summary>
                    //Green,
                    ///// <summary>The color cyan (blue-green).</summary>
                    //Cyan,
                    ///// <summary>The color red.</summary>
                    //Red,
                    ///// <summary>The color magenta (purplish-red).</summary>
                    //Magenta,
                    ///// <summary>The color yellow.</summary>
                    //Yellow,
                    ///// <summary>The color white.</summary>
                    //White,
                    ConsoleColor.Gray, ConsoleColor.Blue, ConsoleColor.Gray,
                    ConsoleColor.Cyan, ConsoleColor.Red, ConsoleColor.Magenta, ConsoleColor.Yellow,
                    ConsoleColor.White
                };
                var random = Math.Abs(vectorClock.GetHashCode()) % valid.Length;
                return valid[random];
            }
        }

        /// <summary>
        /// Best for when logs are streaming by in front of the user
        /// </summary>
        private class ByLogType : IColorProvider
        {
            public ConsoleColor GetColor(TraceEventType level, string vectorClock)
            {
                switch (level)
                {
                    case TraceEventType.Critical:
                        return ConsoleColor.Red;
                        break;
                    case TraceEventType.Error:
                        return  ConsoleColor.Magenta;
                        break;
                    case TraceEventType.Warning:
                        return  ConsoleColor.Yellow;
                        break;
                    case TraceEventType.Information:
                        return  ConsoleColor.Gray;
                        break;
                    case TraceEventType.Verbose:
                        return  ConsoleColor.DarkGray;
                        break;
                    case TraceEventType.Start:
                        return  ConsoleColor.Green;
                        break;
                    case TraceEventType.Stop:
                        return  ConsoleColor.Green;
                        break;
                    case TraceEventType.Suspend:
                    //break;
                    case TraceEventType.Resume:
                    //break;
                    case TraceEventType.Transfer:
                    //break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            }
        }

        /// <summary>
        /// Best for visual distance amongst active spans.
        /// </summary>
        sealed class LeastRecentlyUsed : IColorProvider
        {
            private int index = 0;
            private ConcurrentDictionary<ConsoleColor, int> colors;

            public LeastRecentlyUsed()
            {
                this.colors = new ConcurrentDictionary<ConsoleColor, int>(new[]
                    {
                        ConsoleColor.Gray, ConsoleColor.Blue, ConsoleColor.Green,
                        ConsoleColor.Cyan, ConsoleColor.Red, ConsoleColor.Magenta, ConsoleColor.Yellow,
                        ConsoleColor.White
                    }
                    .ToDictionary(k => k, k => 1));
            }

            public ConsoleColor GetColor(TraceEventType type, string vectorClock)
            {
                lock (this)
                {
                    unchecked
                    {
                        this.index++;
                    }

                    ConsoleColor toUse;

                    var c = GlobalTracer.Instance.ActiveSpan.GetBaggageItem("colorFor" + vectorClock);
                    if (c != null)
                    {
                        toUse = (ConsoleColor) Enum.Parse(typeof(ConsoleColor), c);
                    }
                    else
                    {
                        toUse = this.colors.OrderBy(k => k.Value).First().Key;
                        // Save the color
                        GlobalTracer.Instance.ActiveSpan
                            .SetBaggageItem("colorFor" + vectorClock, toUse.ToString());
                    }

                    // Update last used time
                    this.colors.AddOrUpdate(toUse, k => 1, (_, __) => this.index);

                    return toUse;
                }
            }
        }
    }
}