using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test
{
    using System.Diagnostics;
    using System.Dynamic;
    using System.IO;
    using OpenTracing;
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
            // Representing:
            // Console.Out -> OpeTracing
            // Trace.WriteLine -> OpenTracing
            // OpenTracing -> traceSourceSink (A TraceSource)

            // traceSourceSink -> Console/others
            
            LegacyCodeHelper.EnableTracingForLegacyCode(
                LegacyCodeHelper.LegacyFeaturesUsed.ConsoleOut
                | LegacyCodeHelper.LegacyFeaturesUsed.TraceWrite,
                LegacyCodeHelper.OutputSinks.ColoredConsole);

            // Use the code
            using (var scope = GlobalTracer.Instance.BuildSpan("Overall")
                .WithTag("key", "value")
                .StartActive())
            {
                scope.Span.Log("Starting now");

                List<Task> tasks = new List<Task>();
                for (int i = 0; i < 50; i++)
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
                                Trace.TraceWarning("Running inside a span");
                                Trace.TraceError("Err");

                                Console.WriteLine("Console out!");
                                GlobalTracer.Instance.ActiveSpan.Log("Running inside a span");

                                await Task.Delay(1000);

                                GlobalTracer.Instance.ActiveSpan.SetTag("result", true);
                            }
                        }));
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
