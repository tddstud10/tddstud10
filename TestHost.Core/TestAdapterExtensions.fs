namespace R4nd0mApps.TddStud10.TestHost

module TestAdapterExtensions = 
    open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
    open R4nd0mApps.TddStud10.Common
    open R4nd0mApps.TddStud10.Common.Domain
    open System
    open System.IO
    open System.Reflection

    let knownAdaptersMap = 
        [ (FilePath "xunit.runner.visualstudio.testadapter.dll", 
           [ ("ITestDiscoverer", "Xunit.Runner.VisualStudio.TestAdapter.VsTestRunner")
             ("ITestExecutor", "Xunit.Runner.VisualStudio.TestAdapter.VsTestRunner") ]
           |> Map.ofList)
          (FilePath "NUnit.VisualStudio.TestAdapter.dll", 
           [ ("ITestDiscoverer", "NUnit.VisualStudio.TestAdapter.NUnitTestDiscoverer")
             ("ITestExecutor", "NUnit.VisualStudio.TestAdapter.NUnitTestExecutor") ]
           |> Map.ofList)
          (FilePath "NUnit3.TestAdapter.dll", 
           [ ("ITestDiscoverer", "NUnit.VisualStudio.TestAdapter.NUnit3TestDiscoverer")
             ("ITestExecutor", "NUnit.VisualStudio.TestAdapter.NUnit3TestExecutor") ]
           |> Map.ofList) ]
        |> Map.ofList
    
    let createAdapter adapterMap adapterType path =
        let asmResolver sp = 
            let innerFn sp _ (args : ResolveEventArgs) =
                let loadedAsm =
                    ["*.dll"; "*.exe"]
                    |> Seq.collect (fun extn -> PathBuilder.enumerateFiles SearchOption.AllDirectories (AssemblyName(args.Name).Name + extn) sp)
                    |> Seq.tryFind (fun (FilePath p) -> File.Exists(p))
                    |> Option.fold (fun _ (FilePath e) -> Assembly.LoadFrom(e)) null
                loadedAsm
            innerFn sp

        let loadAssembly (FilePath p) =
            Assembly.LoadFrom(p)
                
        let resolver = asmResolver (Path.GetDirectoryName(path.ToString()) |> FilePath)
        try
            AppDomain.CurrentDomain.add_AssemblyResolve (ResolveEventHandler resolver)
            path
            |> loadAssembly
            |> fun a -> 
                adapterMap
                |> Map.tryFind (PathBuilder.getFileName path)
                |> Option.bind (Map.tryFind adapterType)
                |> Option.bind (a.GetType >> Some)
                |> Option.bind (Activator.CreateInstance >> Some)
        finally
            AppDomain.CurrentDomain.remove_AssemblyResolve (ResolveEventHandler resolver)

    let findAdapterAssemblies =
        PathBuilder.enumerateFiles SearchOption.AllDirectories "*.testadapter.dll"

    let findTestDiscoverers adapterMap = 
        findAdapterAssemblies
        >> Seq.choose (createAdapter adapterMap "ITestDiscoverer")
        >> Seq.map (fun a -> a :?> ITestDiscoverer)

    let findTestExecutors adapterMap= 
        findAdapterAssemblies
        >> Seq.choose (createAdapter adapterMap "ITestExecutor")
        >> Seq.map (fun a -> a :?> ITestExecutor)
