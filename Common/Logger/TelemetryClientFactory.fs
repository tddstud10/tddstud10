module R4nd0mApps.TddStud10.Logger.TelemetryClientFactory

let telemetryClient : ITelemetryClient = 
    XFactory.X "R4nd0mApps.TddStud10.Logger.WindowsTelemetryClient" (NullTelemetryClient() :> _)
