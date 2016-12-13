namespace R4nd0mApps.TddStud10.Logger

[<Sealed>]
type internal NullTelemetryClient() = 
    interface ITelemetryClient with
        member __.Initialize(_, _) = ()
        member __.TrackEvent(_, _, _) = ()
        member __.Flush() = ()
