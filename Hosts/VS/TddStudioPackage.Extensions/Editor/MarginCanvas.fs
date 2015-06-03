namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open System.Windows.Controls
open System.Windows

type ChildEntry = Rect * FrameworkElement

type MarginCanvas() as self = 
    inherit Canvas()
    
    do 
        self.Width <- MarginConstants.Width
        self.ClipToBounds <- true
    
    let positionChild (topLeft : Point) ((r, e) : ChildEntry) = 
        e.Height <- MarginConstants.Width * 0.8
        e.Width <- e.Height
        e.SetValue(Canvas.TopProperty, (r.Top + r.Bottom) / 2.0 - 4.0 - topLeft.Y)
        e.SetValue(Canvas.LeftProperty, 1.0)
        e
    
    let addChild _ e = self.Children.Add(e) |> ignore

    member public self.Refresh (topLeft : Point) (newChildren : ChildEntry seq) = 
        self.Children.Clear()
        newChildren
        |> Seq.map (positionChild topLeft)
        |> Seq.fold addChild ()

#if DONT_COMPILE
- Ctor - width, cliptobounds, no children

- Refresh
  - old children cleared, only new children present
  - Height/Width, TopProp, LeftProp set

#endif
