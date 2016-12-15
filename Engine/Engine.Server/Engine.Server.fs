module R4nd0mApps.TddStud10.Engine.Server

open Newtonsoft.Json
open Suave
open Suave.Filters
open Suave.Http
open Suave.Operators
open Suave.Web
open Suave.WebPart

[<AutoOpen>]
module Contract = 
    open System

    type RunRequest = 
        { SolutionPath : string
          Delay : TimeSpan }

[<AutoOpen>]
module Utils = 
    let private fromJson<'a> json = JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a
    
    let getResourceFromReq<'a> (req : HttpRequest) = 
        let getString rawForm = System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm
        |> getString
        |> fromJson<'a>

[<EntryPoint>]
let main argv = 
    System.Threading.ThreadPool.SetMinThreads(8, 8) |> ignore
    let handler f : WebPart = 
        fun (r : HttpContext) -> 
            async { 
                let data = r.request |> getResourceFromReq
                let! res = f data
                let res' = 
                    res
                    |> List.toArray
                    |> Json.toJson
                return! Response.response HttpCode.HTTP_200 res' r
            }
    
    let app = 
        Writers.setMimeType "application/json; charset=utf-8" >=> Filters.POST 
        >=> choose [ 
            // Test with: curl -Uri 'http://127.0.0.1:9999/run' -Method Post -Body '{ "solutionPath" : "c:\\a.sln", "delay" : "00:00:01" }'
            path "/run" >=> handler (fun (data : RunRequest) -> async { 
                do! Async.Sleep(data.Delay.TotalSeconds * 1000.0 |> int)
                return [ sprintf "Finished run on %s..." data.SolutionPath ]
            })
        ]
    
    let port = 
        try 
            int argv.[0]
        with _ -> 8088
    
    let defaultBinding = defaultConfig.bindings.[0]
    let withPort = { defaultBinding.socketBinding with port = uint16 port }
    let serverConfig = { defaultConfig with bindings = [ { defaultBinding with socketBinding = withPort } ] }
    startWebServer serverConfig app
    0
