module R4nd0mApps.TddStud10.Engine.Core.RunStepFuncBehaviorsTests

open Xunit
open System
open R4nd0mApps.TddStud10.Engine.TestFramework

let inline (~~) s = FilePath s

let makeSpies (sb, eb, fb) = 
    new CallSpy<RunStepEventArg>(sb), new CallSpy<RunStepErrorEventArg>(eb), new CallSpy<RunStepEventArg>(fb)

let makeAndWireUpRSESpies2 (ss : CallSpy<RunStepEventArg>, se : CallSpy<RunStepErrorEventArg>, sf : CallSpy<RunStepEventArg>) = 
    let rses = 
        { onStart = new Event<RunStepEventArg>()
          onError = new Event<RunStepErrorEventArg>()
          onFinish = new Event<RunStepEventArg>() }
    (rses.onStart.Publish).Add(ss.Func >> ignore)
    (rses.onError.Publish).Add(se.Func >> ignore)
    (rses.onFinish.Publish).Add(sf.Func >> ignore)
    rses, (ss, se, sf)

let makeAndWireUpRSESpies () = 
    let (ss, se, sf) = makeSpies (DoesNotThrow, DoesNotThrow, DoesNotThrow)
    makeAndWireUpRSESpies2 (ss, se, sf)

let isHandlerCalled (s : CallSpy<RunStepEventArg>) slnName stepName =
    s.CalledWith |> Option.map (fun (_, rd) -> rd.solutionPath) = Some(~~slnName) 
    && s.CalledWith |> Option.map (fun (rsn, _) -> rsn) = Some(RunStepName stepName) 

let areStartAndFinishHandlersCalled (ss : CallSpy<RunStepEventArg>, sf : CallSpy<RunStepEventArg>) slnName stepName = 
    true
    && isHandlerCalled ss slnName stepName 
    && isHandlerCalled sf slnName stepName 

let isErrorHandlerCalled (se : CallSpy<RunStepErrorEventArg>) slnName stepName = 
    se.CalledWith |> Option.map (fun rss -> rss.runData.solutionPath) = Some(~~slnName) 

let isErrorHandlerCalled2 (se : CallSpy<RunStepErrorEventArg>) slnName stepName status = 
    isErrorHandlerCalled (se : CallSpy<RunStepErrorEventArg>) slnName stepName  
    && se.CalledWith |> Option.map (fun rss -> rss.status) = Some(status) 

let isErrorHandlerCalled3 (se : CallSpy<RunStepErrorEventArg>) slnName stepName status addendum = 
    isErrorHandlerCalled2 (se : CallSpy<RunStepErrorEventArg>) slnName stepName status
    && se.CalledWith |> Option.map (fun rss -> rss.addendum) = Some(Some(addendum)) 

[<Fact>]
let ``Events Publisher Behavior - Raises start, finish events if no failure``() = 
    let rd = RunExecutor.makeRunData DateTime.Now ~~"c:\\a\\b.sln"
    let rses, (ss, se, sf) = makeAndWireUpRSESpies()
    let f h n k es rd = 
        { name = n; kind = k; status = Succeeded; addendum = None; runData = rd }
    (f |> RunStepFuncBehaviors.eventsPublisher) 1 (RunStepName "step name") Build rses rd |> ignore
    Assert.True(areStartAndFinishHandlersCalled (ss, sf) "c:\\a\\b.sln" "step name")
    Assert.False(isErrorHandlerCalled se "c:\\a\\b.sln" "step name")

[<Fact>]
let ``Events Publisher Behavior - Handled errors - Raises start, error, finish events``() = 
    let rd = RunExecutor.makeRunData DateTime.Now ~~"c:\\a\\b.sln"
    let rses, (ss, se, sf) = makeAndWireUpRSESpies()
    let ex = new TimeZoneNotFoundException()
    let f h n k es rd = 
        raise ex
        { name = n; kind = k; status = Succeeded; addendum = None; runData = rd }
    let f1 = fun () -> ((f |> RunStepFuncBehaviors.eventsPublisher) 1 (RunStepName "step name") Test rses rd |> ignore)
    Assert.Equal(ex, Assert.Throws<TimeZoneNotFoundException>(f1))
    Assert.True(areStartAndFinishHandlersCalled (ss, sf) "c:\\a\\b.sln" "step name")
    Assert.True(isErrorHandlerCalled3 se "c:\\a\\b.sln" "step name" Aborted (ExceptionData(ex)))

[<Fact>]
let ``Events Publisher - Unhandled errors - Raises start, error, finish events``() = 
    let rd = RunExecutor.makeRunData DateTime.Now ~~"c:\\a\\b.sln"
    let rses, (ss, se, sf) = makeAndWireUpRSESpies()
    let f h n k es rd = 
        { name = n; kind = k; status = Failed; addendum = Some(FreeFormatData "Error Details"); runData = rd }
    (f |> RunStepFuncBehaviors.eventsPublisher) 1 (RunStepName "step name") Build rses rd |> ignore
    Assert.True(areStartAndFinishHandlersCalled (ss, sf) "c:\\a\\b.sln" "step name")
    Assert.True(isErrorHandlerCalled3 se "c:\\a\\b.sln" "step name" Failed (FreeFormatData("Error Details")))

[<Fact>]
let ``Events Publisher - Handled errors - Raises all events even when all of them crash``() = 
    let (ss, se, sf) = makeSpies(Throws, DoesNotThrow, DoesNotThrow)
    let rd = RunExecutor.makeRunData DateTime.Now ~~"c:\\a\\b.sln"
    let rses, _ = makeAndWireUpRSESpies2 (ss, se, sf)
    let f h n k es rd = 
        { name = n; kind = k; status = Failed; addendum = Some(FreeFormatData "Error Details"); runData = rd }
    (f |> RunStepFuncBehaviors.eventsPublisher) 1 (RunStepName "step name") Build rses rd |> ignore
    Assert.True(ss.Called)
    Assert.True(isErrorHandlerCalled3 se "c:\\a\\b.sln" "step name" Failed (FreeFormatData("Error Details")))
    Assert.True(isHandlerCalled sf "c:\\a\\b.sln" "step name")

[<Fact>]
let ``Events Publisher - Unhandled errors - Raises all events even when all of them crash``() = 
    let (ss, se, sf) = makeSpies(Throws, Throws, Throws)
    let rd = RunExecutor.makeRunData DateTime.Now ~~"c:\\a\\b.sln"
    let rses, (ss, se, sf) = makeAndWireUpRSESpies2 (ss, se, sf)
    let ex = new TimeZoneNotFoundException()
    let f h n k es rd = 
        raise ex
        { name = n; kind = k; status = Failed; addendum = Some(FreeFormatData "Error Details"); runData = rd }
    let f1 = fun () -> ((f |> RunStepFuncBehaviors.eventsPublisher) 1 (RunStepName "step name") Build rses rd |> ignore)
    Assert.Equal(ex, Assert.Throws<TimeZoneNotFoundException>(f1))
    Assert.True(ss.Called)
    Assert.True(se.Called)
    Assert.True(sf.Called)

[<Fact>]
let ``Timer - Does not fail for funcs that dont fail``() = 
    let f h n k es rd = rd
    (f |> RunStepFuncBehaviors.stepTimer) 1 2 3 4 |> ignore
    Assert.True(true)

[<Fact>]
let ``Timer - Lets exceptions pass through``() = 
    let ex = new TimeZoneNotFoundException()
    
    let f h n k es rd = 
        raise ex
        rd
    
    let f1 = fun () -> ((f |> RunStepFuncBehaviors.stepTimer) 1 2 3 4 5 |> ignore)
    Assert.Equal(ex, Assert.Throws<TimeZoneNotFoundException>(f1))

[<Fact>]
let ``Logger - Does not fail for funcs that dont fail``() = 
    let f h n k es rd = rd
    (f |> RunStepFuncBehaviors.stepLogger) 1 2 3 4 |> ignore
    Assert.True(true)

[<Fact>]
let ``Logger - Lets exceptions pass through``() = 
    let ex = new TimeZoneNotFoundException()
    
    let f h n k es rd = 
        raise ex
        rd
    
    let f1 = fun () -> ((f |> RunStepFuncBehaviors.stepLogger) 1 2 3 4 5 |> ignore)
    Assert.Equal(ex, Assert.Throws<TimeZoneNotFoundException>(f1))
