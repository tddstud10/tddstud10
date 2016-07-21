namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.TestDoubles

open System.Runtime.CompilerServices
open System

[<AllowNullLiteral>]
type public ITestServiceInterface = 
    abstract Method1 : unit -> string

type public TestService() = 
    interface ITestServiceInterface with
        member this.Method1() = "TestService.ITestServiceInterface.Method1"

type public TestServiceProvider() = 
    interface IServiceProvider with
        member this.GetService(serviceType : Type) = 
            match serviceType with
            | t when t = typeof<TestService> -> new TestService() :> Object
            | _ -> null
