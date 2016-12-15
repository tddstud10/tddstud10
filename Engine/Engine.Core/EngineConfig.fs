namespace R4nd0mApps.TddStud10.Engine.Core

open System.Runtime.Serialization

[<AllowNullLiteral>]
[<DataContract>]
type EngineConfig() = 
    [<DataMember(IsRequired = false)>]
    member val SnapShotRoot = @"%temp%\_tdd" with get, set

    [<DataMember(IsRequired = false)>]
    member val IgnoredTests = "" with get, set

    [<DataMember(IsRequired = false)>]
    member val IsDisabled = false with get, set

    [<DataMember(IsRequired = false)>]
    member val AdditionalMSBuildProperties : string[] = [||] with get, set
