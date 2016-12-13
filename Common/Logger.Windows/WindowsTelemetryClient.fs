namespace R4nd0mApps.TddStud10.Logger

open Microsoft.ApplicationInsights
open System
open System.IO
open System.Reflection

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
        
        member __.Initialize(version, edition) = 
            tc.InstrumentationKey <- loadKey()
            tc.Context.Session.Id <- Guid.NewGuid().ToString()
            tc.Context.Component.Version <- version
            tc.Context.Device.Model <- edition
            tc.Context.User.Id <- sprintf "%s(%s)" Environment.UserName Environment.MachineName
        
        member __.TrackEvent(eventName, properties, metrics) = tc.TrackEvent(eventName, properties, metrics)
        member __.Flush() = tc.Flush()
