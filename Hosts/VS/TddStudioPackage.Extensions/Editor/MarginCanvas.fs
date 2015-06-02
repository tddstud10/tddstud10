namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open System.Windows.Controls
open System.Windows.Media

type MarginCanvas(width : float) as t = 
    inherit Canvas()
    do 
        t.Width <- width
        t.ClipToBounds <- true
