namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.TestPlatform.ObjectModel

type ContextMenuData() =
    static let instance = lazy ContextMenuData()
    member val TestCase : TestCase option = None with get, set

    static member Instance 
        with public get () = instance.Value
