module R4nd0mApps.TddStud10.Engine.Core.EngineConfigLoaderTests

open R4nd0mApps.TddStud10.Common.Domain
open System.IO
open System.Runtime.Serialization
open Xunit

[<AllowNullLiteral>]
[<DataContract>]
type AConfig() = 
    let mutable aSetting = "aSetting default value"
    let mutable bSetting = 0xdeadbeef
    
    [<DataMember(IsRequired = false)>]
    member __.ASetting 
        with get () = aSetting
        and set value = aSetting <- value
    
    [<DataMember(IsRequired = false)>]
    member __.BSetting 
        with get () = bSetting
        and set value = bSetting <- value

let getSlnPath sln = sprintf @"%s.%s" (Path.GetTempFileName()) sln |> FilePath

[<Fact>]
let ``First time load creates file and returns default value``() = 
    let sln = getSlnPath "first.sln"
    let defCfg = AConfig()
    let cfg = EngineConfigLoader.load defCfg sln
    Assert.Same(defCfg, cfg)
    Assert.True(File.Exists(EngineConfigLoader.configPath sln))

[<Fact>]
let ``First time load creates file and returns default value, even if save fails``() = 
    let sln = getSlnPath "|first.sln"
    let defCfg = AConfig()
    let cfg = EngineConfigLoader.load defCfg sln
    Assert.Same(defCfg, cfg)
    Assert.False(File.Exists(EngineConfigLoader.configPath sln))

[<Fact>]
let ``Second time load reads from file and returns value``() = 
    let sln = getSlnPath "second.sln"
    let defCfg = AConfig()
    EngineConfigLoader.load defCfg sln |> ignore
    let cfg = EngineConfigLoader.load (null :> AConfig) sln
    Assert.NotSame(defCfg, cfg)
    Assert.Equal(defCfg.ASetting, cfg.ASetting)
    File.Delete(EngineConfigLoader.configPath sln)

[<Fact>]
let ``Second time load with corrupted file, returns default values and recreates file``() = 
    let sln = getSlnPath "first.sln"
    File.WriteAllText(EngineConfigLoader.configPath sln, "{\"aSetting :\"aSetting default value\"}")
    let defCfg = AConfig()
    let cfg = EngineConfigLoader.load defCfg sln
    let cfg2 = EngineConfigLoader.load (null :> AConfig) sln
    Assert.Same(defCfg, cfg)
    Assert.NotSame(defCfg, cfg2)
    Assert.Equal(defCfg.ASetting, cfg2.ASetting)
    File.Delete(EngineConfigLoader.configPath sln)

[<Fact>]
let ``Second time load with some values, override those and return default value for others``() = 
    let sln = getSlnPath "second.sln"
    File.WriteAllText(EngineConfigLoader.configPath sln, "{\"ASetting\":\"changed aSetting value\"}")
    let defCfg = AConfig()
    let cfg = EngineConfigLoader.load defCfg sln
    Assert.Equal(cfg.ASetting, "changed aSetting value")
    Assert.NotEqual<string>(cfg.ASetting, defCfg.ASetting)
    Assert.Equal(cfg.BSetting, defCfg.BSetting)
    File.Delete(EngineConfigLoader.configPath sln)
