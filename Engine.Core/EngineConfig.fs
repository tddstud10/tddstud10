namespace R4nd0mApps.TddStud10.Engine.Core

open System.Runtime.Serialization

[<AllowNullLiteral>]
[<DataContract>]
type EngineConfig() = 
    let mutable snapShotRoot = @"%temp%\_tdd"
    let mutable ignoredTests = ""
    let mutable isDisabled = false
    
    [<DataMember(IsRequired = false)>]
    member __.SnapShotRoot 
        with get () = snapShotRoot
        and set value = snapShotRoot <- value

    [<DataMember(IsRequired = false)>]
    member __.IgnoredTests 
        with get () = ignoredTests
        and set value = ignoredTests <- value

    [<DataMember(IsRequired = false)>]
    member __.IsDisabled 
        with get () = isDisabled
        and set value = isDisabled <- value