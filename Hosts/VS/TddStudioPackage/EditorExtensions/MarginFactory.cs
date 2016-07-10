using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;

#if DONT_COMPILE

TODO:
- Incremental Pipeline
- <deploy>
- Disable/Enable for project
  - remember across solution loads
  - disable tdd should show '?' in the status bar
  - clear & disable editor extensions
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
- Instrumentation - inserts after jumps - so marker gets ignored
  - createCoverageData doesnt show coverage information - entersp markers are not injected looks like
- <deploy>
- Kata Videos
- <release>
- Toolwindow + Click on Notification Icon
- Engine events wire up 
  - Custom Trigger mechanism with 3 goals: exception in one handler should not affect the others
  - Combine attach/detach between EnginerLoader and TddStudi10Runner
  - Move to disposable model where we detach on dispose.
  - Get methods to attach from outside - dont expose events.
  - EngineHost, RunState, DataStore, ConsoleApp, [TBD:ToolWindow], etc.
- Perf Ideas
  - Out of process engine, build and test hosts
  - fail gracefully on large projects
  - vsvim, xunit
  - Move to async tagging
  - Taggers
    - Filter on empty, non-code
    - Do we need async tagger?
  - Dont react to changes lines without code
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

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            var menuCmdService = TddStud10Package.Instance.GetService<IMenuCommandService>();
            if (menuCmdService == null)
            {
                Logger.I.LogError("Unable to get IMenuCommandService. Context menus will be disabled!");
            }

            return new Margin(
                textViewHost.TextView,
                _aggregatorFactory.CreateTagAggregator<IMarginGlyphTag>(textViewHost.TextView.TextBuffer),
                menuCmdService != null
                    ? menuCmdService.ShowContextMenu
                    : new Action<CommandID, int, int>((_, __, ___) => { }));
        }
    }
}
