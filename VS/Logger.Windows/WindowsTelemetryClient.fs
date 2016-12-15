namespace R4nd0mApps.TddStud10.Logger

open Microsoft.ApplicationInsights
open Microsoft.ApplicationInsights.DataContracts
open Microsoft.ApplicationInsights.Extensibility
open System
open System.IO
open System.Reflection

type internal WindowsTelemetryOperation(oh : IOperationHolder<RequestTelemetry>) = 
    member val OperationHolder = oh
    interface ITelemetryOperation

[<Sealed>]
type internal WindowsTelemetryClient() = 
    let tc = TelemetryClient()
    
    let loadKey() = 
        let d = 
            Assembly.GetExecutingAssembly().CodeBase
            |> fun cb -> Uri(cb).LocalPath
            |> Path.GetFullPath
            |> Path.GetDirectoryName
        
        let keyFile = Path.Combine(d, "Telemetry.Instrumentation.Key")
        if File.Exists keyFile then File.ReadAllText(keyFile).Trim()
        else ""
    
    static let i = new WindowsTelemetryClient()
    static member public I = i
    interface ITelemetryClient with
        
        member __.Initialize(version, hostVersion, hostEdition) = 
            tc.InstrumentationKey <- loadKey()
            tc.Context.Session.Id <- Guid.NewGuid().ToString()
            tc.Context.Component.Version <- version
            tc.Context.Device.Type <- hostVersion
            tc.Context.Device.Model <- hostEdition
            tc.Context.User.Id <- sprintf "%s(%s)" Environment.UserName Environment.MachineName
        
        member __.TrackEvent(eventName, properties, metrics) = tc.TrackEvent(eventName, properties, metrics)
        member __.StartOperation(operationName) = 
            new WindowsTelemetryOperation(tc.StartOperation<RequestTelemetry>(operationName)) :> _
        member __.StopOperation(operation) = 
            (operation :?> WindowsTelemetryOperation).OperationHolder |> tc.StopOperation
        member __.Flush() = tc.Flush()
