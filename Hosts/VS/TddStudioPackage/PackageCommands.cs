using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine.Core;
using CommandEntry = System.Tuple<string, uint, System.EventHandler, System.EventHandler>;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    // NOTE: Move to FS when we make a change in this next.
    public class PackageCommands
    {
        private IServiceProvider _serviceProvider;
        private readonly Settings _settings;
        private EnvDTE.DTE _dte;
        private IMenuCommandService _mcs;

        public PackageCommands(IServiceProvider serviceProvider, Settings settings)
        {
            _serviceProvider = serviceProvider;
            _settings = settings;
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
            Logger.I.LogInfo("Changing TddStud10 state...");

            if (EngineLoader.IsEngineEnabled())
            {
                EngineLoader.DisableEngine();
                _settings.SetSetting(Settings.IsTddStudioEnabled, false);
            }
            else
            {
                EngineLoader.EnableEngine();
                _settings.SetSetting(Settings.IsTddStudioEnabled, true);
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
            cmd.Text = _settings.GetSetting(Settings.IsTddStudioEnabled) 
                        ? Properties.Resources.DisableTddStud10State 
                        : Properties.Resources.EnableTddStud10State;
        }

        #endregion

        #region PkgCmdIDList.ViewTddStud10Logs

        private void OnBeforeQueryStatusViewTddStud10Logs(object sender, EventArgs e)
        {
            Logger.I.LogInfo("Querying for ViewTddStud10Logs...");

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
