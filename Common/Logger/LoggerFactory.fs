module R4nd0mApps.TddStud10.Logger.LoggerFactory

open System
open System.IO
open System.Reflection

let private getLocalPath() = 
    Assembly.GetExecutingAssembly().CodeBase
    |> fun cb -> Uri(cb).LocalPath
    |> Path.GetFullPath
    |> Path.GetDirectoryName

let logger : ILogger = 
    let dir = () |> getLocalPath
    let file = Assembly.GetExecutingAssembly().GetName().Name + ".Windows.dll"
    let path = Path.Combine(dir, file)
    if File.Exists path then 
        Assembly.LoadFrom(path)
        |> fun a -> a.GetType("R4nd0mApps.TddStud10.Logger.WindowsLogger")
        |> fun t -> Activator.CreateInstance(t) :?> ILogger
    else NullLogger() :> _
