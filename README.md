# OpenTracing-System.Diagnostics
Bridge between C# `System.Diagnostics` tracing and `OpenTracing`

## CorrelationManagerHook
Target audience: Folks who have code instrumented with [Trace.Correlationmanager.StartLogicalOperation](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.correlationmanager.startlogicaloperation?redirectedfrom=MSDN&view=netframework-4.7.2#System_Diagnostics_CorrelationManager_StartLogicalOperation) and [Trace.CorrelationManager.StopLogicalOperation](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.correlationmanager.stoplogicaloperation?view=netframework-4.7.2) but want those calls to go to [OpenTracing](http://opentracing.io).

Use `CorrelationManagerHook.PipeCorrelationManagerToOpenTracing()` to establish the connection so that all calls to `Trace.CorrelationManager` are passed to `GlobalTracer.Instance`.

## OpenTracingTraceListener
Creates a `TraceListener` that will forward the writes to `GlobalTracer.Instance.ActiveSpan.Log`. This is useful if your exiting code contains `Trace.Write` events that you wish to have proper context for (having them be logged to the `ISpan` rather than ambiently).

## OpenTracingTraceSource
Creates a `ITracer`/`TraceSource` pair that are tied together. The `ITracer` returned is the source of the events, and they're sent to the `TraceSource`'s `.Listeners` as `TraceEvent`, `TraceInformation`, and `TraceData` calls.

* ScopeManager.Activate: `TraceEvent(TraceEventType.Start, id: 1, ...)`
* Span.Finish: `TraceEvent(TraceEventType.Stop, id: 2, ...)`
* Span.Log: `TraceData(TraceEventType.Information, id: 3, data: IEnumerable<KeyValuePair<string, object>>)`
* Span.SetTag: `TraceData(TraceEventType.Information, id: 4, data: KeyValuePair<string, object>)`

## Usage
1. Hook Trace.CorrelationManager to GlobalTracer to capture legacy calls
	`CorrelationManagerHook.PipeCorrelationManagerToOpenTracing()`
1. Hook Trace to GlobalTracer to capture Trace messages
	`Trace.Listeners.Add(new OpenTracingTraceListener());`
1. [Optional] Hook your-logging to GlobalTracer to capture those calls (as desired)
1. Hook GlobalTracer to an output system of your choosing. E.g. to ConsoleListener
	```C#
	var pair = OpenTracingTraceSource.CreateTracerTraceSourcePair();
	var traceSourceSink = pair.TraceSourceSink;
	var tracerSource = pair.TracerSource;
	
	// Tell the sink to write to console
	traceSourceSink.Listeners.Add(new ConsoleTraceListener());

	// Tell OT to write to the sink
	GlobalTracer.Register(tracerSource);
	```