namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core

open Microsoft.VisualStudio
open Microsoft.VisualStudio.Shell.Interop
open System
open System.Runtime.CompilerServices
open System.Windows

[<Extension>]
type public VSSolutionExtensions = 
    [<Extension>]
    static member public GetProperty<'T>(vsSln : IVsSolution, propId : int) : 'T = 
        let mutable value : obj = null
        let mutable result : 'T = Unchecked.defaultof<'T>
        
        if vsSln.GetProperty(propId, &value) = VSConstants.S_OK then
            result <- value :?> 'T;

        result;
