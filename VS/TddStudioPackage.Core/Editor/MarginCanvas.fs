namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open System.Windows.Controls
open System.Windows
open R4nd0mApps.TddStud10.Hosts.Common.Margin
open R4nd0mApps.TddStud10.Hosts.Common.Margin.ViewModel


// NOTE: This class should not contain any business other than the l/t/w/h set + seq -> UIElementCollction copy.
// Hence this class is not covered by unit tests  
type MarginCanvas(getZL) = 
    let vm = MainViewModel()
    let c = MainUserControl(DataContext = vm)

    do 
        c.ClipToBounds <- true
    
    member __.UserControl: FrameworkElement = 
        c :> FrameworkElement

    member __.Refresh(newChildren : (Rect * FrameworkElement * HostIdeActions) seq) = 
        let addChild (acc : UIElementCollection) ((r, e, ha) : Rect * FrameworkElement * HostIdeActions) = 
            e.Height <- r.Height
            e.Width <- r.Width
            e.SetValue(Canvas.TopProperty, r.Top)
            e.SetValue(Canvas.LeftProperty, r.Left)
            e.MouseDown.Add(fun _ -> vm.ShowPopup(ha))
            acc.Add(e) |> ignore
            acc
        c.Canvas.Children.Clear()
        c.Width <- MarginConstants.Width * getZL()
        newChildren
        |> Seq.fold addChild c.Canvas.Children
        |> ignore
