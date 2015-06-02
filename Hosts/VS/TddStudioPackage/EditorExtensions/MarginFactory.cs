﻿using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor;

#if DONT_COMPILE

TODO:
- DataStore code
  - Move into
  - Unit tests
- Margin code
  - Move into
  - Unit tests
  - DRY removal
- Canvas code
  - Move into
-------------------
- Cannot check by str = "Discover Unit Tests" in datastore events
- Change in eventing infra 
  - RunStartEA, RunErrorEA, RunEndEA - make i tconsistent with runexecutor
  - RunErrorEA contains RSR
  - RunEndEA contains RSR and data
  - Engine steps don't update datastore
- datastore entities must be non-null always
- rename datastorexxx
- For new runs - we should merge right - when is the right time to pull that in?
- Engine events wire up - exception in one handler should not affect the others
  - Combine attach/detach between EnginerLoader and TddStudi10Runner
  - Move to disposable model where we detach on dispose.
  - Get methods to attach from outside - dont expose events.
  - EngineHost, RunState, DataStore, ConsoleApp, [TBD:ToolWindow], etc.
- Move to async tagging

- Infra questions
  - Should the tagger be disposable, if so who will call the IDispose?
  - Should I not be unsubscribing from the eventhanders in Margin
  - NormalizedSnapshotSpanCollection
    - Normalized means [a] sorted [b] overlaps combined [c] but not necessarily consecutive
    - Arg to GetTags - will never contain SnapshotSpan spanning multiple TextSnapshotLine-s
  - Span == Eucleadean Line Segment
  - SnapshotSpan == Span but within a text snapshot [not neessarily in the same text line]
  - SnapshotPoint == Point in a SnapshotSpan, also in a TextSnapshotLine
  - In Margin.TagsChangedEventArgs we are refetching all tags is that OK?
  - Ok to hold reference to ITextSnapshotLine in Tag?
  - Jared thinks LayoutEvent is too costly - what is the option?
  - s.Start.GetContainingLine().LineNumber - in GetTags is obviously not valid - as the snap could be the entire document.

==================

- reload solution logic
- save file command
- reduce view model class size
  - remove openfiledialog from vm
  - remove unwanted methods - esp. from the editor creation


datastore
state [potentially corresponding to >1 state per line]

MarginFactory -> Margin -> Canvas
- canvas

[CreateMargin] -> WpfTextViewHost -> WpfTextViewHost 
- lines

tagger trio

events:
- datastoreupdated
- layoutchanged
- tagschanged


----
[Discovery Test Finishes]
- E:RunStepEnded [RunStepKind = DiscoveryUnitTests, RunStepStatus, RunData]
  - If not Succeeded then ()
  - Else DataStore.Update PerAssemblyTestCases
  [Each open TextBuffers have associated Taggers which have subscribed to E:DataStoreUpdated] 
    - M:xTagger_OnDataStoreUpdated
      - raise E:TagsChanged
      [??? Each open document has Margin which subscribes to E:xTagsChanged]
        - M:Margin_xTagsChanged
          - Locate the glyph corresponding that line, clear it
          - Add new glyph if needed
      [??? Will M:Margin_xTagsChanged and M:Margin_LayoutChanged collide]

[File Open]
[??? How does M:Margin_xTagsChanged get called?]

[File Scroll]
[??? How does M:Margin_xTagsChanged get called?]


[??? How do we deal with edited text [a] edit on a given line [b] shift lines up/down]


http://stackoverflow.com/questions/17167423/creating-a-tagger-that-has-more-than-one-tag-type-for-vs-extension/24923127#24923127	
https://github.com/qwertie/Loyc/blob/master/Visual%20Studio%20Integration/LoycExtensionForVs/SampleLanguage.cs



Datastore
- E:Updated

Tagger trio
- M:GetTags
- E:TagsChanged

Margin
- M: Ctor
- M: TextView_LayoutChanged
- M: Dispose

TextView
- E: LayoutChanged




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


Enhance Test App
- move tddpackageextension one level up - refactor the projects
- keyboard input
- implement sort/remove using in fsharppowertools
- compress datastore size - esp the last one where unit tests are repeated


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
        [Import]
        private IBufferTagAggregatorFactoryService AggregatorFactory = null;

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            return new Margin(textViewHost.TextView, AggregatorFactory.CreateTagAggregator<TestMarkerTag>(textViewHost.TextView.TextBuffer));
        }
    }
}