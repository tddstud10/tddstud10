namespace R4nd0mApps.TddStud10.TestHost

module TestAdapterExtensions = 
    open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter
    open R4nd0mApps.TddStud10.Common
    open R4nd0mApps.TddStud10.Common.Domain
    open System
    open System.IO
    open System.Reflection
    
    let logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger

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
        let loadAssembly (FilePath p) = Assembly.LoadFrom(p)
        
        let loadType (a : Assembly) name =
            match a.GetType(name) with
            | null -> None
            | t -> Some t

        let asmResolver sp = 
            let innerFn _ (args : ResolveEventArgs) = 
                [ "*.dll"; "*.exe" ]
                |> Seq.map ((+) (AssemblyName(args.Name).Name))
                |> Seq.collect (fun name -> PathBuilder.enumerateFiles SearchOption.AllDirectories name sp)
                |> Seq.tryFind PathBuilder.fileExists
                |> Option.fold (fun _ -> loadAssembly) null
            innerFn
        
        let resolver = asmResolver (Path.GetDirectoryName(path.ToString()) |> FilePath)
        try 
            logger.logInfof "Attempting to load Test Adapter %s from %O" adapterType path
            AppDomain.CurrentDomain.add_AssemblyResolve (ResolveEventHandler resolver)
            let res =
                path
                |> loadAssembly
                |> fun a -> 
                    adapterMap
                    |> Map.tryFind (PathBuilder.getFileName path)
                    |> Option.bind (Map.tryFind adapterType)
                    |> Option.bind (loadType a)
                    |> Option.bind (Activator.CreateInstance >> Some)
            logger.logInfof "Loaded Test Adapter %s from %O" adapterType path
            res
        finally
            AppDomain.CurrentDomain.remove_AssemblyResolve (ResolveEventHandler resolver)
    
    let findAdapterAssemblies dir = 
        if PathBuilder.directoryExists dir then 
            PathBuilder.enumerateFiles SearchOption.AllDirectories "*.testadapter.dll" dir
        else Seq.empty<FilePath>
    
    let findTestDiscoverers adapterMap = 
        findAdapterAssemblies
        >> Seq.choose (createAdapter adapterMap "ITestDiscoverer")
        >> Seq.map (fun a -> a :?> ITestDiscoverer)
    
    let findTestExecutors adapterMap = 
        findAdapterAssemblies
        >> Seq.choose (createAdapter adapterMap "ITestExecutor")
        >> Seq.map (fun a -> a :?> ITestExecutor)
