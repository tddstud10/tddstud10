module R4nd0mApps.TddStud10.Logger.XFactory

open System
open System.IO
open System.Reflection

let private getLocalPath() = 
    Assembly.GetExecutingAssembly().CodeBase
    |> fun cb -> Uri(cb).LocalPath
    |> Path.GetFullPath
    |> Path.GetDirectoryName

let X<'T> typeName nullX : 'T = 
    let dir = () |> getLocalPath
    let file = Assembly.GetExecutingAssembly().GetName().Name + ".Windows.dll"
    let path = Path.Combine(dir, file)
    if File.Exists path then 
        Assembly.LoadFrom(path)
        |> fun a -> a.GetType(typeName)
        |> fun t -> t.GetProperty("I", System.Reflection.BindingFlags.NonPublic ||| BindingFlags.Static)
        |> fun f -> f.GetValue(null) :?> 'T
    else nullX
