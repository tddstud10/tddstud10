namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions

open System
open System.Runtime.CompilerServices
open Microsoft.Diagnostics.Tracing
open R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
open Microsoft.VisualStudio.Shell

[<Extension>]
type public Services = 
    
    [<Extension>]
    static member public GetService<'TS, 'TI when 'TI : null>(sp : IServiceProvider) : 'TI = 
        let s = sp.GetService(typeof<'TS>)
        match s with
        | null -> 
            Logger.logErrorf "Unable to query for service of type %s." typeof<'TS>.FullName
            null
        | :? 'TI as i -> i
        | _ -> 
            Logger.logErrorf "Cannot cast service '%s' to '%s'." typeof<'TS>.FullName typeof<'TI>.FullName
            null
    
    static member public GetService<'T when 'T : null>() : 'T = 
        Services.GetService<'T, 'T>(ServiceProvider.GlobalProvider)

    static member public GetService<'TS, 'TI when 'TI : null>() : 'TI = 
        Services.GetService<'TS, 'TI>(ServiceProvider.GlobalProvider)
