namespace R4nd0mApps.TddStud10.Logger

open System.Collections.Generic

type ITelemetryClient = 
    abstract Initialize : version:string * edition:string -> unit
    abstract TrackEvent : eventName:string * properties:IDictionary<string, string> * metrics:IDictionary<string, float>
     -> unit
    abstract Flush : unit -> unit
