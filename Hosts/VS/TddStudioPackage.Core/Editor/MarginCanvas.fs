namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open System.Windows.Controls
open System.Windows
open R4nd0mApps.TddStud10.Hosts.Common.Margin
open R4nd0mApps.TddStud10.Hosts.Common.Margin.ViewModel


// NOTE: This class should not contain any business other than the l/t/w/h set + seq -> UIElementCollction copy.
// Hence this class is not covered by unit tests  
type MarginCanvas(getZL) = 
    let c = MainUserControl(DataContext = MainViewModel())

    do 
        c.ClipToBounds <- true
    
    member __.UserControl: FrameworkElement = 
        c :> FrameworkElement

    member __.Refresh(newChildren : (Rect * FrameworkElement) seq) = 
        let addChild (acc : UIElementCollection) ((r, e) : Rect * FrameworkElement) = 
            e.Height <- r.Height
            e.Width <- r.Width
            e.SetValue(Canvas.TopProperty, r.Top)
            e.SetValue(Canvas.LeftProperty, r.Left)
            e.MouseDown.Add(fun _ -> (c.DataContext :?> MainViewModel).ShowPopup(e.Tag :?> HostIdeActions))
            acc.Add(e) |> ignore
            acc
        c.Canvas.Children.Clear()
        c.Width <- MarginConstants.Width * getZL()
        newChildren
        |> Seq.fold addChild c.Canvas.Children
        |> ignore
