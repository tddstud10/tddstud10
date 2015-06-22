using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor;
using CommandEntry = System.Tuple<string, uint, System.EventHandler, System.EventHandler>;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    // NOTE: Move to FS when we make a change in this next.
    public class PackageCommands
    {
        private IServiceProvider _serviceProvider;
        private EnvDTE.DTE _dte;
        private IMenuCommandService _mcs;

        public PackageCommands(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dte = serviceProvider.GetService<EnvDTE.DTE>();
            _mcs = serviceProvider.GetService<IMenuCommandService>();
        }

        public void AddCommands()
        {
            new List<CommandEntry>()
            {
                new CommandEntry(PkgGuids.GuidTddStud10CmdSet, PkgCmdID.ChangeTddStud10State, ExecuteChangeTddStud10State, OnBeforeQueryStatusChangeTddStud10State),
                new CommandEntry(PkgGuids.GuidGlyphContextCmdSet, PkgCmdID.DebugTest, ExecuteDebugTest, OnBeforeQueryStatusDebugTest),
            }.Aggregate(
                _mcs,
                (mcs, e) =>
                {
                    CommandID menuCommandID = new CommandID(new Guid(e.Item1), (int)e.Item2);
                    var menuItem = new OleMenuCommand(e.Item3, menuCommandID);
                    menuItem.BeforeQueryStatus += e.Item4;
                    mcs.AddCommand(menuItem);
                    return mcs;
                });
        }

        #region PkgCmdIDList.ChangeTddStud10State

        private void ExecuteChangeTddStud10State(object sender, EventArgs e)
        {
            Logger.I.LogInfo("Changing TddStud10 state...");

            if (EngineLoader.IsEngineEnabled())
            {
                EngineLoader.DisableEngine();
            }
            else
            {
                EngineLoader.EnableEngine();
            }
        }

        private void OnBeforeQueryStatusChangeTddStud10State(object sender, EventArgs e)
        {
            Logger.I.LogInfo("Querying for TddStud10 state...");

            var cmd = sender as OleMenuCommand;
            if (cmd == null)
            {
                Logger.I.LogError("sender should have been an OleMenuCommand. This is unexpected.");
                return;
            }

            if (!_dte.Solution.IsOpen)
            {
                Logger.I.LogInfo("Solution is not open.");
                cmd.Visible = false;
                return;
            }

            cmd.Visible = true;
            if (EngineLoader.IsEngineEnabled())
            {
                cmd.Text = Properties.Resources.DisableTddStud10State;
            }
            else
            {
                cmd.Text = Properties.Resources.EnableTddStud10State;
            }
        }

        #endregion

        #region PkgCmdIDList.DebugTest

        private void ExecuteDebugTest(object sender, EventArgs e)
        {
            Logger.I.LogInfo("Debug Test...");

            var mgts = ContextMenuData.Instance.GlyphTags;
            if (mgts.Any())
            {
                return;
            }

            var tsts = from mt in mgts
                       let tt = mt as TestStartTag
                       where tt != null
                       from tc in tt.testCases
                       select tc;

            var test = tsts.FirstOrDefault();
            if (test == null)
            {
                return;
            }

            _dte.SetBreakPoint(test.CodeFilePath, test.LineNumber);

            var tpa = new PerDocumentLocationTestCases();
            var bag = new ConcurrentBag<TestCase>();
            bag.Add(test);
            tpa.TryAdd(new DocumentLocation { document = FilePath.NewFilePath(test.CodeFilePath), line = DocumentCoordinate.NewDocumentCoordinate(test.LineNumber) }, bag);
            var duts = Path.Combine(DataStore.Instance.RunStartParams.Value.solutionBuildRoot.Item, "Z_debug.xml");
            tpa.Serialize(FilePath.NewFilePath(duts));

            _serviceProvider.GetService<SVsShellDebugger, IVsDebugger3>().Launch(
                DataStore.Instance.RunStartParams.Value.testHostPath.Item,
                string.Format(@"_na_ {0} _na_ _na_ {1} _na_", DataStore.Instance.RunStartParams.Value.solutionBuildRoot.Item, duts));
        }

        private void OnBeforeQueryStatusDebugTest(object sender, EventArgs e)
        {
            Logger.I.LogInfo("Querying for Debug Test state...");
            var cmd = sender as OleMenuCommand;
            if (cmd == null)
            {
                Logger.I.LogError("sender should have been an OleMenuCommand. This is unexpected.");
                return;
            }

            if (_dte.Mode == EnvDTE.vsIDEMode.vsIDEModeDebug)
            {
                Logger.I.LogInfo("Debug mode is on. Disabling command.");
                cmd.Visible = false;
                return;
            }

            cmd.Visible = true;
        }

        #endregion
    }
}
