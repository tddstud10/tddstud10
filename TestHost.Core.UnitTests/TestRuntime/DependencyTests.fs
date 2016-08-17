module R4nd0mApps.TddStud10.Common.Domain.DependencyTests

open Xunit
open Mono.Cecil
open System
open System.IO
open System.Collections.Generic

[<Fact>]
let ``Should target 2.0 Runtime``() = 
    let rt = typedefof<R4nd0mApps.TddStud10.TestRuntime.Marker>.Assembly.ImageRuntimeVersion
    Assert.Equal("v2.0.50727", rt)

[<Fact>]
let ``Should have restricted dependencies``() = 
    (* Update the TestRunTimeInstaller if you update this list *)
    let knownDeps = 
        [ ("mscorlib", "2.0.0.0")
          ("Microsoft.Diagnostics.Tracing.EventSource", "1.1.28.0")
          ("System.Core", "3.5.0.0")
          ("System.ServiceModel", "3.0.0.0")
          ("System", "2.0.0.0")
          ("nCrunch.TestRuntime", "2.15.0.9")
          ("R4nd0mApps.TddStud10.TestRuntime.df", "1.0.0.0") ]

    
    let deps = 
        (new Uri(typedefof<R4nd0mApps.TddStud10.TestRuntime.Marker>.Assembly.CodeBase)).LocalPath
        |> Path.GetFullPath
        |> AssemblyDefinition.ReadAssembly
        |> fun a -> a.MainModule.AssemblyReferences
        |> Seq.map (fun r -> r.Name, r.Version.ToString())
    
    Assert.Subset<_>(new HashSet<_>(knownDeps), new HashSet<_>(deps))
