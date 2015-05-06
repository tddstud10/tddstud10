module R4nd0mApps.TddStud10.Engine.Core.StaleMessageIgnoringAgentTests

open Xunit
open System
open System.Collections.Concurrent
open R4nd0mApps.TddStud10.Engine.TestFramework
open R4nd0mApps.TddStud10.Engine.Diagnostics
open System.Threading

let createAgent<'T> (b) =
    let cs = new CallSpy<'T>(b)
    let agent = new StaleMessageIgnoringAgent<'T>(cs.Func >> ignore)
    let eh = new CallSpy<Exception>()
    agent.OnError.Add(eh.Func >> ignore)
    agent, cs, eh

[<Fact>]
let ``SMIAgent discards old messages and processes only the latest``() = 
    let agent, cs, eh = createAgent (DoesNotThrow)
    agent.PauseAsync 200<ms>
    agent.SendMessageAsync "Hello 1" |> ignore
    agent.SendMessageAsync "Hello 2" |> ignore
    let s, is = agent.SendMessage "Hello 3"
    agent.Stop()
    Assert.Equal<string list>(["Hello 2"; "Hello 1"], is)
    Assert.Equal(Some "Hello 3", s)
    Assert.True(cs.Called, "Message processing func was not called")

[<Fact>]
let ``SMIAgent continues processing even when exception is thrown by message processor``() = 
    let agent, cs, eh = createAgent (Throws)
    agent.SendMessageAsync DateTime.Now |> ignore
    Thread.Sleep(500) // NOTE: Duh! Have any better ideas?
    Assert.True(cs.Called, "Message handler should have been invoked")
    Assert.True(eh.Called, "Exception handler should have been invoked")
    agent.Stop()
