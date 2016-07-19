module R4nd0mApps.TddStud10.TestHost.TestAdapterExtensionsTests

open R4nd0mApps.TddStud10.Common
open R4nd0mApps.TddStud10.Common.Domain
open R4nd0mApps.TddStud10.TestExecution
open System.IO
open Xunit

let testDataRoot = 
    PathBuilder.combine [ TestPlatformExtensions.getLocalPath()
                          FilePath "TestData" ]

let adapterMap = 
    [ (FilePath "aunit.testadapter.dll", 
       [ ("ITestDiscoverer", "AUnit.TestAdapter.VsDiscoverer")
         ("ITestExecutor", "AUnit.TestAdapter.VsTestExecutor") ]
       |> Map.ofList)
      (FilePath "bUnit.Testadapter.dll", 
       [ ("ITestDiscoverer", "BUnit.TestAdapter.VsTestDiscoverer")
         ("ITestExecutor", "BUnit.TestAdapter.VsTestExecutor") ]
       |> Map.ofList) ]
    |> Map.ofList

let ``Test Data for Nested search path with no adapters, return empty`` : obj array seq = 
    [| TestAdapterExtensions.findTestDiscoverers adapterMap >> Seq.map box
       TestAdapterExtensions.findTestExecutors adapterMap >> Seq.map box |]
    |> Seq.map (fun a -> [| box a |])

[<Theory>]
[<MemberData("Test Data for Nested search path with no adapters, return empty")>]
let ``Non existant path, return empty`` (f : FilePath -> obj seq) = 
    let sp = 
        PathBuilder.combine [ testDataRoot
                              FilePath "NonExistantPath" ]
    Assert.Empty(sp |> f)

[<Theory>]
[<MemberData("Test Data for Nested search path with no adapters, return empty")>]
let ``Nested search path with no adapters, return empty`` (f : FilePath -> obj seq) = 
    let sp = 
        PathBuilder.combine [ testDataRoot
                              FilePath "NoTestAdapters" ]
    Assert.NotEmpty(sp |> PathBuilder.enumerateFiles SearchOption.AllDirectories "*.*")
    Assert.Empty(sp |> f)

let ``Test Data for Nested Search path with 2 adapters, return both`` : obj array seq = 
    [| (TestAdapterExtensions.findTestDiscoverers adapterMap >> Seq.map box, 
        [ "AUnit.TestAdapter.VsDiscoverer"; "BUnit.TestAdapter.VsTestDiscoverer" ])
       
       (TestAdapterExtensions.findTestExecutors adapterMap >> Seq.map box, 
        [ "AUnit.TestAdapter.VsTestExecutor"; "BUnit.TestAdapter.VsTestExecutor" ]) |]
    |> Seq.map (fun (a, b) -> 
           [| box a
              box b |])

[<Theory>]
[<MemberData("Test Data for Nested Search path with 2 adapters, return both")>]
let ``Nested Search path with 2 adapters, return both`` (f : FilePath -> obj seq, adapters : string list) = 
    let sp = 
        PathBuilder.combine [ testDataRoot
                              FilePath "TestAdapters" ]
    let found = 
        sp
        |> f
        |> Seq.map (fun d -> d.GetType().FullName)
        |> Seq.sort
        |> List.ofSeq
    
    Assert.Equal<string seq>(found, adapters)
