module R4nd0mApps.TddStud10.Engine.TestDoubles

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.TestFramework

type public TestHost(cancelStep : int) = 
    let mutable callCount = 0
    interface IRunExecutorHost with
        member __.HostVersion = VS2015

        member __.CanContinue() = 
            callCount <- callCount + 1
            callCount <= cancelStep
        
        member __.RunStateChanged _ = ()

type StepFunc(behavior) = 
    new() = StepFunc(DoesNotThrow)
    member val Called = false with get, set
    member val CalledWith = None with get, set
    member val ReturningWith = None with get, set
    member public t.Func host sp i rses : RunStepResult = 
        t.Called <- true
        t.CalledWith <- Some(host, sp, i, rses)
        match behavior with
        | DoesNotThrow -> ()
        | Throws(ex) -> raise ex
        let rsr = 
            { status = Succeeded
              addendum = FreeFormatData "And addendum data"
              runData = NoData }
        t.ReturningWith <- Some rsr
        rsr

let inline RS(sf : StepFunc) = 
    { info = 
          { kind = Build
            subKind = InstrumentBinaries
            name = RunStepName(sf.GetHashCode().ToString()) }
      func = sf.Func }
