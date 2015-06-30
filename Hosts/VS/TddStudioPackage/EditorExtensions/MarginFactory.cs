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

TODO:
- <deploy>
- switches
  - Core Idea: Remove a piece selectively w.r.t. CPU/Memory impact
  - Parts
    - Pipeline
    - Datastore [TestHost does discovery as well]
    - Visualizations
    - Disable All + Clear All
    - Dumps Intermediate Data
  - Later 
    - Merging data vs No-merging data
    - Toolwindow
- <deploy>
- Margin needs to scale up with the editor
- ContextMenu
  - All: Mouse over to hand
  - All: Run Covering tests under debugger
  - TS: x
  - FP: (f) Break into first failing test
  - CC: (f) Break into first failing test
- Tooltips - simple text
  - TS: List of test starting at that line 
  - FP: Failing Tests,Exception message + stack trace
  - CC: Failing Tests, Passing Tests
  - All: Right click for more
- <deploy>
- c#, vb, fsharp - test projects
- signing
- solution folder
- IN_TDDSTUDIO + move tddstudio
- Merge data [when to clear data]
- Disable/Enable for project
  - disable tdd should show '?' in the status bar
  - clear & disable editor extensions
- Instrumentation - inserts after jumps - so marker gets ignored
  - createCoverageData doesnt show coverage information - entersp markers are not injected looks like
- <deploy>
- Kata Videos
- <release>
- Per test workflow
- Toolwindow + Click on Notification Icon
- Engine events wire up 
  - Custom Trigger mechanism with 3 goals: exception in one handler should not affect the others
  - Combine attach/detach between EnginerLoader and TddStudi10Runner
  - Move to disposable model where we detach on dispose.
  - Get methods to attach from outside - dont expose events.
  - EngineHost, RunState, DataStore, ConsoleApp, [TBD:ToolWindow], etc.
- Perf Ideas
  - fail gracefully on large projects
  - vsvim, xunit
  - Move to async tagging
  - Do we need async tagger?
- Breakpoint - remove on debug stop, dont/add-remove if breakpoint already present

==================

perf
diagnostics
-------------------------------------
toolwindow - build breaks, list of test, error details
click on icon takes to toolwindow
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
