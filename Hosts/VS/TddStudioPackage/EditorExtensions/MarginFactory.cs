using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor;

#if DONT_COMPILE

TODO:
- Service provider extensions should return None adn not null
- Marging needs to scale up with the editor
- Cannot check by str = "Discover Unit Tests" in datastore events
- Change in eventing infra 
  - RunStartEA, RunErrorEA, RunEndEA - make i tconsistent with runexecutor
  - RunErrorEA contains RSR
  - RunEndEA contains RSR and data
  - Engine steps don't update datastore
- datastore entities must be non-null always
- For new runs - we should merge right - when is the right time to pull that in?
- Engine events wire up 
  - Custom Trigger mechanism with 3 goals: exception in one handler should not affect the others
  - Combine attach/detach between EnginerLoader and TddStudi10Runner
  - Move to disposable model where we detach on dispose.
  - Get methods to attach from outside - dont expose events.
  - EngineHost, RunState, DataStore, ConsoleApp, [TBD:ToolWindow], etc.
- Move to async tagging
- Use SyncContext in Package class also
- SnapshotlineRange - tagger implementation assumes we we ask line by line and not for spans across multiplelines
- getMarkerTags - can detect sequence points but we arent instrumenting it.

- Spec questions
  - How do we deal with edited text [a] edit on a given line [b] shift lines up/down] 
  - if a sequence point is changed, its coverage data should be unknown: how does ncruch handle this?

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

datastore
state [potentially corresponding to >1 state per line]


http://stackoverflow.com/questions/17167423/creating-a-tagger-that-has-more-than-one-tag-type-for-vs-extension/24923127#24923127	
https://github.com/qwertie/Loyc/blob/master/Visual%20Studio%20Integration/LoycExtensionForVs/SampleLanguage.cs



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

- move tddpackageextension one level up - refactor the projects
- keyboard input
- implement sort/remove using in fsharppowertools
- compress datastore size - esp the last one where unit tests are repeated


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

        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider _serviceProvider = null;

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            // - Need to share these id's with package
            // - Ensure - this code is executed just once.
            var menuSvc = _serviceProvider.GetService<IMenuCommandService, IMenuCommandService>();
            var g = new Guid("{1E198C22-5980-4E7E-92F3-F73168D1FB63}");

            new[] { 
                new CommandID(g, 0x502),
                new CommandID(g, 0x503),
                new CommandID(g, 0x504)
            }.Aggregate(
                menuSvc,
                (mcs, e) =>
                {
                    if (mcs.FindCommand(e) == null)
                    {
                        mcs.AddCommand(new MenuCommand(ChangeColor, e));
                    }
                    return menuSvc;
                });

            return new Margin(
                textViewHost.TextView,
                _aggregatorFactory.CreateTagAggregator<TestMarkerTag>(textViewHost.TextView.TextBuffer),
                // - Pass only func not the entire interface
                // - Need another overload of GetService
                _serviceProvider.GetService<IMenuCommandService, IMenuCommandService>());
        }

        // - Move this to FS
        private void LaunchInBuiltinDebugger()
        {
            // - Right click on glyphs in debug mode
            // - TestRunner path needs to come from datastore
            // - what happens for multiple test cases
            // - Unit tests for Marker & CoverageDataCollector + all public statuc methis need to have DebuggerStepThrough
            // - Breakpoint - remove on debug stop, dont/add-remove if breakpoint already present

            var tc = ContextMenuData.Instance.TestCase;

            var _dte = _serviceProvider.GetService<EnvDTE.DTE, EnvDTE.DTE>();
            _dte.Debugger.Breakpoints.Add(null, tc.CodeFilePath, tc.LineNumber);

            var tpa = new PerAssemblyTestCases();
            var bag = new ConcurrentBag<TestCase>();
            bag.Add(tc);
            tpa.TryAdd(FilePath.NewFilePath(tc.Source), bag);
            var duts = Path.Combine(DataStore.Instance.SolutionBuildRoot.Item, "Z_debug.xml");
            tpa.Serialize(FilePath.NewFilePath(duts));

            VsDebugTargetInfo3[] targets = new VsDebugTargetInfo3[1];
            targets[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            targets[0].guidLaunchDebugEngine = new Guid("449EC4CC-30D2-4032-9256-EE18EB41B62B");
            targets[0].bstrExe = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(typeof(MarginFactory).Assembly.Location)), "TddStud10.TestHost.exe");
            targets[0].bstrArg = string.Format(@"debug {0} {0}\Z_coverageresults.xml {0}\Z_testresults.xml {1}", DataStore.Instance.SolutionBuildRoot.Item, duts);
            targets[0].bstrCurDir = DataStore.Instance.SolutionBuildRoot.Item;

            VsDebugTargetProcessInfo[] results = new VsDebugTargetProcessInfo[targets.Length];

            _serviceProvider.GetService<SVsShellDebugger, IVsDebugger3>().LaunchDebugTargets3((uint)targets.Length, targets, results);
        }

        private void ChangeColor(object sender, EventArgs e)
        {
            LaunchInBuiltinDebugger();
        }
    }
}
