namespace R4nd0mApps.TddStud10.Common.TestFramework

open System

type CallSpyBehavior = 
    | DoesNotThrow
    | Throws of Exception

// TODO: Move this along with CallSpy in a common location
type CallSpy1<'T>(behavior) =
    new() = CallSpy1<'T>(DoesNotThrow) 
    member val Called = false with get, set
    member val CalledWith = None with get, set
    member public t.Func(arg : 'T) = 
        t.Called <- true
        t.CalledWith <- Some arg
        match behavior with
        | DoesNotThrow -> ()
        | Throws(ex) -> raise ex
