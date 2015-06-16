namespace R4nd0mApps.TddStud10.Common.TestFramework

open System

type CallSpyBehavior = 
    | DoesNotThrow
    | Throws of Exception

// TODO: Move this along with CallSpy in a common location
// TODO: Consider basing this on Foq
type CallSpy1<'T>(behavior) =
    new() = CallSpy1<'T>(DoesNotThrow) 
    member val CallCount = 0 with get, set
    member val Called = false with get, set
    member val CalledWith = None with get, set
    member public t.Func(arg : 'T) = 
        t.CallCount <- t.CallCount + 1
        t.Called <- true
        t.CalledWith <- Some arg
        match behavior with
        | DoesNotThrow -> ()
        | Throws(ex) -> raise ex
