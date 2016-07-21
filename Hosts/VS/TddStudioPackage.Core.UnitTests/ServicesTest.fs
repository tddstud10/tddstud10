module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.ServicesTests

open System
open System.Runtime.CompilerServices
open Xunit
open R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.TestDoubles

[<Fact>]
let ``Service Provider returns service interface if service is found``() = 
    let sp = new TestServiceProvider()
    let i = sp.GetService<TestService, ITestServiceInterface>()
    Assert.Equal(i.Method1(), "TestService.ITestServiceInterface.Method1")

[<Fact>]
let ``Service Provider returns null if service is not found``() = 
    let sp = new TestServiceProvider()
    let i = sp.GetService<Object, ITestServiceInterface>()
    Assert.Equal(i, null)

[<Fact>]
let ``Service Provider returns null if service is found but interface isnt``() = 
    let sp = new TestServiceProvider()
    let i = sp.GetService<TestService, IServiceProvider>()
    Assert.Equal(i, null)
