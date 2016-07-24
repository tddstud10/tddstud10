using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
                new CommandEntry(PkgGuids.GuidTddStud10CmdSet, PkgCmdID.ViewTddStud10Logs, ExecuteViewTddStud10Logs, (s, e) => { }),
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

        #region PkgCmdIDList.ViewTddStud10Logs

        private void ExecuteViewTddStud10Logs(object sender, EventArgs e)
        {
            var pkgPath = Path.GetDirectoryName(Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath));
            var loggerPath = Path.Combine(pkgPath, @"rtlogs\RealTimeEtwListener.exe");
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(loggerPath);
                info.UseShellExecute = true;
                info.Verb = "runas";
                Process.Start(info);
            }
            catch
            {
                Services
                    .GetService<SVsUIShell, IVsUIShell>()
                    .DisplayMessageBox(
                        Properties.Resources.ProductTitle,
                        string.Format(Properties.Resources.UnableToStartLogger, loggerPath));
            }
        }

        #endregion

        #region PkgCmdIDList.DebugTest

        private void ExecuteDebugTest(object sender, EventArgs e)
        {
            Logger.I.LogInfo("Debug Test...");

            var mgts = ContextMenuData.Instance.GlyphTags;
            if (!mgts.Any())
            {
                return;
            }

            var tsts = from mt in mgts
                       let tt = mt as CodeCoverageTag
                       where tt != null
                       let x = from tc in tt.CCTTestResults
                               select tc.TestCase
                       select new { sp = tt.CCTSeqPoint, tests = x.FirstOrDefault() };

            if (!tsts.Any())
            {
                return;
            }

            var tst = tsts.First();
            if (tst.tests == null)
            {
                return;
            }

            _dte.SetBreakPoint(tst.sp.document.Item, tst.sp.startLine.Item);

            var tpa = new PerDocumentLocationDTestCases();
            var bag = new ConcurrentBag<DTestCase>();
            bag.Add(tst.tests);
            tpa.TryAdd(new DocumentLocation { document = tst.tests.CodeFilePath, line = tst.tests.LineNumber }, bag);
            tpa.Serialize(DataStore.Instance.RunStartParams.Value.DataFiles.DiscoveredUnitDTestsStore);

            _serviceProvider.GetService<SVsShellDebugger, IVsDebugger3>().Launch(
                DataStore.Instance.RunStartParams.Value.TestHostPath.Item,
                Engine.Engine.BuildTestHostCommandLine("execute", DataStore.Instance.RunStartParams.Value));
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
