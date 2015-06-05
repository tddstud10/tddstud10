namespace R4nd0mApps.TddStud10.Common.TestFramework

open System

type CallSpyBehavior = 
    | DoesNotThrow
    | Throws of Exception

// TODO: Move this along with CallSpy in a common location
type CallSpy2<'T1, 'T2>(behavior) = 
    new() = CallSpy2<'T1, 'T2>(DoesNotThrow)
    member val Called = false with get, set
    member val CalledWith = None with get, set
    member public t.Func (arg1 : 'T1) (arg2 : 'T2) = 
        t.Called <- true
        t.CalledWith <- Some(arg1, arg2)
        match behavior with
        | DoesNotThrow -> ()
        | Throws(ex) -> raise ex
