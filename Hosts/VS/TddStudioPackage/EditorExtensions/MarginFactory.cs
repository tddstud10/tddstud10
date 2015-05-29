using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor;

#if DONT_COMPILE

datastore
state [potentially corresponding to >1 state per line]

MarginFactory -> Margin -> Canvas
- canvas

[CreateMargin] -> WpfTextViewHost -> WpfTextViewHost 
- lines

spans

tags

events:
- datastoreupdated
- layoutchanged
- tagschanged




states
- unittest start
- unknown coverage
- uncovered
- partially covered failing tests
- partially covered all passing tests
- fully covered failing tests
- fully covered all passing tests
- test failure origin

Articles:
all of noahric blogs looks like
http://chrisparnin.github.io/articles/2013/09/using-tagging-and-adornments-for-better-todos-in-visual-studio/

Potentials:
https://github.com/EWSoftware/VSSpellChecker


what is the nssc passed to gettags? who calls it? when?
- what is the nssc a collection of?
when do i fire the tags changed?
- is it that once extension detects that tags may be out of dat, it fires it -> gettags is called?
how do other use the TagSpan returned from gettags

spec
- if a sequence point is changed, its coverage data should be unknown: how does ncruch handle this?

===================



  mouse
- https://github.com/tunnelvisionlabs/InheritanceMargin/blob/f9f47148c7eb3de15fc92ca2ff372d266af63d4f/Tvl.VisualStudio.InheritanceMargin/InheritanceGlyphFactory.cs
  - command bindings on glyph
- [Export(typeof(IGlyphMouseProcessorProvider))]


selective of margin

blogs.msdn.com vs editor 

myltiple tag attr
- https://github.com/adamdriscoll/poshtools/blob/18eee4842c5643385bdd8db148b42d48d867c74e/ReplWindow/Repl/Margin/GlyphPrompts.cs

- getting access to service provider
    [Import(typeof(Microsoft.VisualStudio.Shell.SVsServiceProvider))]
    internal IServiceProvider _serviceProvider = null;

- check if textview is not closed
- viewchange module

#endif

namespace R4nd0mApps.TddStud10.Hosts.VS.EditorExtensions
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(MarginConstants.Name)]
    [Order(After = PredefinedMarginNames.Outlining)]
    [MarginContainer(PredefinedMarginNames.Left)]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public sealed class MarginFactory : IWpfTextViewMarginProvider
    {
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            return new Margin(textViewHost.TextView);
        }
    }
}
