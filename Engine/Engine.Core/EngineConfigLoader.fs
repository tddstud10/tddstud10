namespace R4nd0mApps.TddStud10.Engine.Core

module EngineConfigLoader = 
    open R4nd0mApps.TddStud10.Common.Domain
    open R4nd0mApps.TddStud10.Engine.Core
    open System.IO
    open System.Runtime.Serialization.Json
    open System.Text
    
    let configPath slnPath = slnPath.ToString() + ".tddstud10.user"
    
    let private toJson<'T> (cfg : 'T) = 
        use ms = new MemoryStream()
        (new DataContractJsonSerializer(typeof<'T>)).WriteObject(ms, cfg)
        Encoding.UTF8.GetString(ms.ToArray())
    
    let private fromJson<'T> (jsonString : string) : 'T option = 
        use ms = new MemoryStream(UTF8Encoding.Default.GetBytes(jsonString))
        fun () -> (new DataContractJsonSerializer(typeof<'T>)).ReadObject(ms) :?> 'T 
        |> Common.safeExec2
    
    let load defaultCfg (slnPath : FilePath) = 
        let nullCfg = 
            "{}"
            |> fromJson
            |> Option.fold (fun _ -> id) defaultCfg
        
        let cfgPath = configPath slnPath
        
        let cfg, json = 
            if File.Exists(cfgPath) then 
                let cfg = 
                    File.ReadAllText(cfgPath)
                    |> fromJson
                    |> Option.fold (fun _ -> id) defaultCfg
                cfg, toJson cfg
            else defaultCfg, toJson defaultCfg
        
        let copyNonDefaultProperties nullCfg defaultCfg cfg = 
            nullCfg.GetType().GetProperties()
            |> Array.filter (fun pi -> pi.GetValue(cfg) = pi.GetValue(nullCfg))
            |> Array.iter (fun pi -> pi.SetValue(cfg, pi.GetValue(defaultCfg)))
        
        Common.safeExec (fun () -> File.WriteAllText(cfgPath, json))
        copyNonDefaultProperties nullCfg defaultCfg cfg
        cfg

    let setConfig (slnPath: FilePath) cfg =
        let cfgPath = configPath slnPath
        File.WriteAllText(cfgPath, cfg |> toJson)