namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

type ContextMenuData() = 
    static let instance = lazy ContextMenuData()
    member val GlyphTags : IMarginGlyphTag seq = Seq.empty with get, set
    static member Instance 
        with public get () = instance.Value
