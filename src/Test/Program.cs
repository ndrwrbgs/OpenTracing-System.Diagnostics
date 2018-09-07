using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    using System.Diagnostics;
    using System.Dynamic;
    using Newtonsoft.Json;
    using OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing;
    using OpenTracing.Util;

    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            Trace.Listeners.Add(new OpenTracingTraceListener());

            var pair = OpenTracingTraceSource.CreateTracerTraceSourcePair();
            var traceSourceSink = pair.TraceSourceSink;
            var tracerSource = pair.TracerSource;
            
            // Tell the sink to write to console
            traceSourceSink.Listeners.Add(new JsonDataConsoleTraceListener());

            // Tell OT to write to the sink
            GlobalTracer.Register(tracerSource);


            using (var scope = GlobalTracer.Instance.BuildSpan("Overall")
                .WithTag("key", "value")
                .StartActive())
            {
                scope.Span.Log("Starting now");

                List<Task> tasks = new List<Task>();
                for (int i = 0; i < 5; i++)
                {
                    var i1 = i;
                    tasks.Add(Task.Run(
                        async () =>
                        {
                            await Task.Yield();
                            using (GlobalTracer.Instance.BuildSpan("Span " + i1)
                                .WithTag("k" + i1, "v" + i1)
                                .StartActive())
                            {
                                Trace.WriteLine("Running inside a span");
                                GlobalTracer.Instance.ActiveSpan.Log("Running inside a span");

                                await Task.Delay(1000);

                                GlobalTracer.Instance.ActiveSpan.SetTag("result", true);
                            }
                        }));
                }

                await Task.WhenAll(tasks);
            }
        }

        private class JsonDataConsoleTraceListener : ConsoleTraceListener
        {
            public JsonDataConsoleTraceListener()
            {
            }

            public JsonDataConsoleTraceListener(bool useErrorStream) : base(useErrorStream)
            {
            }

            public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
            {
                if (data is IDictionary<string, object> dictionary)
                {
                    var obj = new ExpandoObject();
                    foreach (var kvp in dictionary)
                    {
                        (obj as IDictionary<string, object>).Add(kvp);
                    }

                    data = obj;
                }

                base.TraceData(eventCache, source, eventType, id, 
                    // TODO: Will break some Filters, since now it's not the object anymore. Better to JUST override the ToString()
                    JsonConvert.SerializeObject(data));
            }
        }
    }
}
