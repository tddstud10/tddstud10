module R4nd0mApps.TddStud10.Engine.TestDoubles

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.TestFramework

type public TestHost(cancelStep : int) = 
    let mutable callCount = 0
    interface IRunExecutorHost with
        member __.CanContinue() = 
            callCount <- callCount + 1
            callCount <= cancelStep
        member __.RunStateChanged _ =
            ()

let getRss rss = 
    fun n k ->
        { rss with name = n
                   kind = k
                   status = Failed
                   addendum = FreeFormatData "There has been a failure" }

type StepFunc(behavior) = 
    new() = StepFunc(DoesNotThrow)
    member val Called = false with get, set
    member val CalledWith = None with get, set
    member val ReturningWith = None with get, set
    member public t.Func _ sp name kind subKind _ : RunStepResult = 
        t.Called <- true
        t.CalledWith <- Some sp
        match behavior with
        | DoesNotThrow -> ()
        | Throws(ex) -> raise ex
        let rsr = { startParams = sp
                    name = name
                    kind = kind
                    subKind = subKind
                    status = Failed
                    addendum = FreeFormatData "There has been a failure"
                    runData = NoData }
        t.ReturningWith <- Some rsr
        rsr


let inline RS(sf : StepFunc) = 
    { kind = Build
      subKind = InstrumentBinaries
      name = RunStepName(sf.GetHashCode().ToString())
      func = sf.Func }
