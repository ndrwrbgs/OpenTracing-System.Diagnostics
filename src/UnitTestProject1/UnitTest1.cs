using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    using System.Diagnostics;
    using OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing;
    using OpenTracing.Util;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            LegacyCodeHelper.EnableTracingForLegacyCode(
                LegacyCodeHelper.LegacyFeaturesUsed.ConsoleOut
                | LegacyCodeHelper.LegacyFeaturesUsed.TraceWrite
                | LegacyCodeHelper.LegacyFeaturesUsed.NoTracingSetup,
                LegacyCodeHelper.OutputSinks.ColoredConsole);
            Console.WriteLine("CW");
            Trace.WriteLine("TW");
            GlobalTracer.Instance
                .BuildSpan("Stuff")
                .StartActive()
                .Dispose();
        }
    }
}
