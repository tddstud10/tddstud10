namespace R4nd0mApps.TddStud10.Logger

open System.Collections.Generic

type ITelemetryOperation = 
    interface
    end

type ITelemetryClient = 
    abstract Initialize : version:string * hostVersion:string * hostEdition:string -> unit
    abstract TrackEvent : eventName:string * properties:IDictionary<string, string> * metrics:IDictionary<string, float>
     -> unit
    abstract StartOperation : operationName:string -> ITelemetryOperation
    abstract StopOperation : ITelemetryOperation -> unit
    abstract Flush : unit -> unit
