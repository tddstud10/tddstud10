namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Editor
open System
open Microsoft.VisualStudio.Utilities
open System.ComponentModel.Composition

[<Export(typeof<IWpfTextViewMarginProvider>)>]
[<Name(MarginConstants.Name)>]
[<Order(After = PredefinedMarginNames.Outlining)>]
[<MarginContainer(PredefinedMarginNames.Left)>]
[<ContentType("code")>]
[<TextViewRole(PredefinedTextViewRoles.Interactive)>]
type MarginFactory = 
    interface IWpfTextViewMarginProvider with
        member x.CreateMargin(wpfTextViewHost : IWpfTextViewHost, marginContainer : IWpfTextViewMargin) : IWpfTextViewMargin = 
            new Margin(wpfTextViewHost.TextView) :> IWpfTextViewMargin
