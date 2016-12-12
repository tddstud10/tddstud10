using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core;
using R4nd0mApps.TddStud10.Logger;
using System;
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
        private static ILogger Logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger;

        private EnvDTE.DTE _dte;
        private IMenuCommandService _mcs;

        public PackageCommands(IServiceProvider serviceProvider)
        {
            _dte = serviceProvider.GetService<EnvDTE.DTE>();
            _mcs = serviceProvider.GetService<IMenuCommandService>();
        }

        public void AddCommands()
        {
            new List<CommandEntry>()
            {
                new CommandEntry(PkgGuids.GuidTddStud10CmdSet, PkgCmdID.ChangeTddStud10State, ExecuteChangeTddStud10State, OnBeforeQueryStatusChangeTddStud10State),
                new CommandEntry(PkgGuids.GuidTddStud10CmdSet, PkgCmdID.ViewTddStud10Logs, ExecuteViewTddStud10Logs, OnBeforeQueryStatusViewTddStud10Logs),
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
            Logger.LogInfo("Changing TddStud10 state...");

            if (EngineLoader.IsEngineEnabled())
            {
                EngineLoader.DisableEngine();
                SetTddStudioDisable(true);
            }
            else
            {
                EngineLoader.EnableEngine();
                SetTddStudioDisable(false);
            }
        }

        private void SetTddStudioDisable(bool isDisabled)
        {
            var solutionPath = FilePath.NewFilePath(_dte.Solution.FileName);
            var config = EngineConfigLoader.load(new EngineConfig(), solutionPath);
            config.IsDisabled = isDisabled;
            TddStud10Package.Instance.IconHost.RunState = RunState.Initial;

            EngineConfigLoader.setConfig(solutionPath, config);
        }

        private void OnBeforeQueryStatusChangeTddStud10State(object sender, EventArgs e)
        {
            Logger.LogInfo("Querying for TddStud10 state...");

            var cmd = sender as OleMenuCommand;
            if (cmd == null)
            {
                Logger.LogError("sender should have been an OleMenuCommand. This is unexpected.");
                return;
            }

            if (!_dte.Solution.IsOpen)
            {
                Logger.LogInfo("Solution is not open.");
                cmd.Visible = false;
                return;
            }

            cmd.Visible = true;
            cmd.Text = EngineLoader.IsEngineEnabled()
                        ? Properties.Resources.DisableTddStud10State
                        : Properties.Resources.EnableTddStud10State;
        }

        #endregion

        #region PkgCmdIDList.ViewTddStud10Logs

        private void OnBeforeQueryStatusViewTddStud10Logs(object sender, EventArgs e)
        {
            Logger.LogInfo("Querying for ViewTddStud10Logs...");

            var cmd = sender as OleMenuCommand;
            if (cmd == null)
            {
                Logger.LogError("sender should have been an OleMenuCommand. This is unexpected.");
                return;
            }

            if (!_dte.Solution.IsOpen)
            {
                Logger.LogInfo("Solution is not open.");
                cmd.Visible = false;
                return;
            }

            cmd.Visible = true;
            cmd.Text = Properties.Resources.ViewTddStud10Logs;
        }

        private void ExecuteViewTddStud10Logs(object sender, EventArgs e)
        {
            var pkgPath = Path.GetDirectoryName(Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath));
            var loggerPath = Path.Combine(pkgPath, string.Format(@"rtlogs\RealTimeEtwListener{0}.exe", Constants.ProductVariant));
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
    }
}
