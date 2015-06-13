module R4nd0mApps.TddStud10.Engine.TestDoubles

open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.Engine.TestFramework

type public TestHost(cancelStep : int) = 
    let mutable callCount = 0
    interface IRunExecutorHost with
        member this.CanContinue() = 
            callCount <- callCount + 1
            callCount <= cancelStep
        member this.RunStateChanged rs =
            ()

let getRss rss = 
    fun n k rd ->
        let retRd = { rd with sequencePoints = Some(new PerDocumentSequencePoints()) }
        { rss with name = n
                   kind = k
                   status = Failed
                   addendum = FreeFormatData "There has been a failure"
                   runData = retRd }

type StepFunc(behavior) = 
    new() = StepFunc(DoesNotThrow)
    member val Called = false with get, set
    member val CalledWith = None with get, set
    member val ReturningWith = None with get, set
    member public t.Func (h : IRunExecutorHost) name kind subKind events (rd : RunData) : RunStepResult = 
        t.Called <- true
        t.CalledWith <- Some(rd.GetHashCode())
        match behavior with
        | DoesNotThrow -> ()
        | Throws(ex) -> raise ex
        let retRd = { rd with sequencePoints = Some(new PerDocumentSequencePoints()) }
        t.ReturningWith <- Some(retRd.GetHashCode())
        { name = name
          kind = kind
          subKind = subKind
          status = Failed
          addendum = FreeFormatData "There has been a failure"
          runData = retRd }

let inline RS(sf : StepFunc) = 
    { kind = Build
      subKind = InstrumentBinaries
      name = RunStepName(sf.GetHashCode().ToString())
      func = sf.Func }
