module R4nd0mApps.TddStud10.Engine.TestFramework

open System
open R4nd0mApps.TddStud10.Common.Domain

let inline (~~) s = FilePath s

type CallSpyBehavior =
    | DoesNotThrow
    | Throws of Exception

type CallSpy<'T>(behavior) =
    new() = CallSpy<'T>(DoesNotThrow) 
    member val Called = false with get, set
    member val CalledWith = None with get, set
    member public t.Func(arg : 'T) : 'T = 
        t.Called <- true
        t.CalledWith <- Some arg
        match behavior with
        | DoesNotThrow -> ()
        | Throws(ex) -> raise ex
        arg
