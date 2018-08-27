# OpenTracing-System.Diagnostics
Bridge between C# `System.Diagnostics` tracing and `OpenTracing`

Target audience: Folks who have code instrumented with [Trace.Correlationmanager.StartLogicalOperation](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.correlationmanager.startlogicaloperation?redirectedfrom=MSDN&view=netframework-4.7.2#System_Diagnostics_CorrelationManager_StartLogicalOperation) and [Trace.CorrelationManager.StopLogicalOperation](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.correlationmanager.stoplogicaloperation?view=netframework-4.7.2) but want those calls to go to [OpenTracing](http://opentracing.io).
