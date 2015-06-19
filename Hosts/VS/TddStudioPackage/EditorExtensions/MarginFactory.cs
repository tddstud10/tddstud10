using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor;

#if DONT_COMPILE
v TestStartTag
- SequencePointTag
- FailurePointTag
- CodeCoverageTag 

TODO:
- Move all icons to margin 
  - SequencePointTag
    - TDD Tagger
    - TDD Margin w/ trivial glyph factory [i.e. overwrites glyphs]
    - Move stuff to datastore
    - Optimize Glyph factory
  - CoverageTag
    - TDD Tagger
    - TDD Margin w/ trivial glyph factory [i.e. overwrites glyphs]
    - Move stuff to datastore
    - Optimize Glyph factory
  - PointOfFailureTag
    - Datastore entity
    - Collect data
    - TDD Tagger
    - TDD Margin w/ trivial glyph factory [i.e. overwrites glyphs]
    - Move stuff to datastore
    - Optimize Glyph factory
  - Light up as soon as data is available
  - TODO: Why can we not compare the sp.id itself?
  - Margin needs to scale up with the editor
  - Delete old datastore methods
  - Delete old tagger code
- switches
  - run pipeline but no data collection [testhost does not look for testcases]
  - Editor tagging
  - Toolwindow
  - merging data vs no-merging data
  - disable == all
  - Do we need async tagger?
- createCoverageData doesnt show coverage information - entersp markers are not injected looks like
- <deploy>
- Blockers
  - Merge data
- Kata Videos
- Disable/Enable for project
- <release>
- Toolwindow + Click on Notification Icon
- For new runs - we should merge right - when is the right time to pull that in?
- Engine events wire up 
  - Custom Trigger mechanism with 3 goals: exception in one handler should not affect the others
  - Combine attach/detach between EnginerLoader and TddStudi10Runner
  - Move to disposable model where we detach on dispose.
  - Get methods to attach from outside - dont expose events.
  - EngineHost, RunState, DataStore, ConsoleApp, [TBD:ToolWindow], etc.
- Move to async tagging
- SnapshotlineRange - tagger implementation assumes we we ask line by line and not for spans across multiplelines
- Breakpoint - remove on debug stop, dont/add-remove if breakpoint already present

- Spec questions
  - How do we deal with edited text [a] edit on a given line [b] shift lines up/down] 
  - if a sequence point is changed, its coverage data should be unknown: how does ncruch handle this?

- Infra questions
  - Should the tagger be disposable, if so who will call the IDispose?
  - Should I not be unsubscribing from the eventhanders in Margin
  - NormalizedSnapshotSpanCollection
    - Normalized means [a] sorted [b] overlaps combined [c] but not necessarily consecutive
    - Arg to GetTags - can contain SnapshotSpan spanning multiple TextSnapshotLine-s
  - Span == Eucleadean Line Segment
  - SnapshotSpan == Span but within a text snapshot [not neessarily in the same text line]
  - SnapshotPoint == Point in a SnapshotSpan, also in a TextSnapshotLine
  - In Margin.TagsChangedEventArgs we are refetching all tags is that OK?
  - Ok to hold reference to ITextSnapshotLine in Tag?
  - Jared thinks LayoutEvent is too costly - what is the option?

==================


glyphs 
- test start, red/green, exception point 
- debug first failing tests, debug all tests
toolwindow - build breaks, list of test, error details
click on icon takes to toolwindow
misc
- c#, vb, fsharp
- signing
- solution folder
- IN_TDDSTUDIO + move tddstudio
- disable tdd should show '?' in the status bar
2 x katas
perf
- disable components
- fail gracefully on large projects
- vsvim, xunit
diagnostics
-------------------------------------
- incremental build [per assembly pipeline]
- nunit support
- cpp
- sublime text

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
        private IBufferTagAggregatorFactoryService _aggregatorFactory = null;

        [Import(typeof(SVsServiceProvider), AllowDefault = true)]
        private IServiceProvider _serviceProvider = null;

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            return new Margin(
                textViewHost.TextView,
                _aggregatorFactory.CreateTagAggregator<IMarginGlyphTag>(textViewHost.TextView.TextBuffer),
                _serviceProvider != null
                    ? _serviceProvider.GetService<IMenuCommandService>().ShowContextMenu
                    : new Action<System.ComponentModel.Design.CommandID, int, int>((_, __, ___) => { }));
        }
    }
}
