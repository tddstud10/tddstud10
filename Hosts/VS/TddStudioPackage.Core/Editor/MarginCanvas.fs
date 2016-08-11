namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor

open System.Windows.Controls
open System.Windows
open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
open R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel


// NOTE: This class should not contain any business other than the l/t/w/h set + seq -> UIElementCollction copy.
// Hence this class is not covered by unit tests  
type MarginCanvas =
   abstract member Refresh: seq<Rect * FrameworkElement> -> unit
   abstract member FE: FrameworkElement

type MarginCanvasV1(getZL) as self = 
    inherit Canvas()
    
    do 
        self.ClipToBounds <- true
    
    interface MarginCanvas with
        member x.FE: FrameworkElement = 
            x :> FrameworkElement
         
        member self.Refresh(newChildren : (Rect * FrameworkElement) seq) = 
            let addChild (acc : UIElementCollection) ((r, e) : Rect * FrameworkElement) = 
                e.Height <- r.Height
                e.Width <- r.Width
                e.SetValue(Canvas.TopProperty, r.Top)
                e.SetValue(Canvas.LeftProperty, r.Left)
                acc.Add(e) |> ignore
                acc
            self.Children.Clear()
            self.Width <- MarginConstants.Width * getZL()
            newChildren
            |> Seq.fold addChild self.Children
            |> ignore

type MarginCanvasV2(getZL) = 
    let c = MainUserControl1(DataContext = MainViewModel1())

    do 
        c.ClipToBounds <- true
    
    interface MarginCanvas with 
        member __.FE: FrameworkElement = 
            c :> FrameworkElement

        member self.Refresh(newChildren : (Rect * FrameworkElement) seq) = 
            let addChild (acc : UIElementCollection) ((r, e) : Rect * FrameworkElement) = 
                e.Height <- r.Height
                e.Width <- r.Width
                e.SetValue(Canvas.TopProperty, r.Top)
                e.SetValue(Canvas.LeftProperty, r.Left)
                e.MouseDown.Add(fun _ -> (c.DataContext :?> MainViewModel1).ShowPopup(e.Tag :?> HostIdeActions))
                acc.Add(e) |> ignore
                acc
            c.Canvas.Children.Clear()
            // Unhook event handlers
            c.Width <- MarginConstants.Width * getZL()
            newChildren
            |> Seq.fold addChild c.Canvas.Children
            |> ignore
