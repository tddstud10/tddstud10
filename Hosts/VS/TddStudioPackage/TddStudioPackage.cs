using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;
using EventHandlerPair = System.Tuple<System.EventHandler, System.EventHandler>;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    [ProvideBindingPath]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "0.3.3.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [Guid(GuidList.GuidTddStud10Pkg)]
    public sealed class TddStud10Package : Package, IVsSolutionEvents, IEngineHost
    {
        private Control _uiThreadInvoker;

        private bool _disposed;

        private uint solutionEventsCookie;

        private IVsSolution2 _solution;
        private IVsStatusbar _statusBar;
        private EnvDTE.DTE _dte;

        private VsStatusBarIconHost _iconHost;

        public static TddStud10Package Instance { get; private set; }

        public TddStud10Package()
        {
        }

        public void InvokeOnUIThread(Action action)
        {
            _uiThreadInvoker.Dispatcher.Invoke(action);
        }

        #region Package Members

        protected override void Initialize()
        {
            base.Initialize();

            _uiThreadInvoker = new Control();

            _solution = Services.GetService<SVsSolution, IVsSolution2>();
            if (_solution != null)
            {
                _solution.AdviseSolutionEvents(this, out solutionEventsCookie).ThrowOnFailure();
            }

            _statusBar = Services.GetService<SVsStatusbar, IVsStatusbar>();

            _dte = Services.GetService<EnvDTE.DTE>();

            // TODO: Move to fs: This needs to be moved out to another method and another class responsible for
            // providing commands.
            OleMenuCommandService mcs = this.GetService<IMenuCommandService, OleMenuCommandService>();
            if (null != mcs)
            {
                new Dictionary<uint, EventHandlerPair>
                {
                    { PkgCmdIDList.ChangeTddStud10State, new EventHandlerPair(ExecuteChangeTddStud10State, OnBeforeQueryStatusChangeTddStud10State) },
                }.Aggregate(
                    new KeyValuePair<uint, EventHandlerPair>(),
                    (_, kvp) =>
                    {
                        CommandID menuCommandID = new CommandID(new Guid(GuidList.GuidProgressBarCmdSetString), (int)kvp.Key);
                        var menuItem = new OleMenuCommand(kvp.Value.Item1, menuCommandID);
                        menuItem.BeforeQueryStatus += kvp.Value.Item2;
                        mcs.AddCommand(menuItem);
                        return kvp;
                    });
            }

            _iconHost = VsStatusBarIconHost.CreateAndInjectIntoVsStatusBar();

            Instance = this;

            Logger.I.LogInfo("Initialized Package successfully.");
        }

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

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                if (_solution != null && solutionEventsCookie != 0)
                {
                    _solution.UnadviseSolutionEvents(solutionEventsCookie);
                }
                _solution = null;

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region IVsSolutionEvents Members

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            EngineLoader.Load(this, _dte.Solution.FullName, DateTime.UtcNow);
            EngineLoader.EnableEngine();

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            EngineLoader.DisableEngine();
            EngineLoader.Unload();
            _iconHost.InvokeAsyncOnStatusBarThread(
                () =>
                {
                    _iconHost.RunState = RunState.Initial;
                });

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            pfCancel = 0;
            if (EngineLoader.IsRunInProgress())
            {
                Logger.I.LogInfo("Run in progress. Denying request to close solution.");
                Services.GetService<SVsUIShell, IVsUIShell>().DisplayMessageBox(Properties.Resources.ProductTitle, Properties.Resources.CannotCloseSolution);
                pfCancel = 1; // Veto closing of solution.
            }

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IEngineHost Members

        public bool CanContinue()
        {
            if (_dte.Solution.SolutionBuild.BuildState == EnvDTE.vsBuildState.vsBuildStateInProgress)
            {
                Logger.I.LogInfo("Build in progress. Asking to stop.");
                return false;
            }

            return true;
        }

        public void RunStateChanged(RunState rs)
        {
            _iconHost.InvokeAsyncOnStatusBarThread(
                () =>
                {
                    _iconHost.RunState = rs;
                });
        }

        public void RunStarting(RunData rd)
        {
        }

        public void RunStepStarting(RunStepEventArg rsea)
        {
        }

        public void OnRunStepError(RunStepResult rss)
        {
        }

        public void RunStepEnded(RunStepResult rss)
        {
        }

        public void OnRunError(Exception e)
        {
        }

        public void RunEnded(RunData rd)
        {
            InvokeOnUIThread(
                () => CoverageData.Instance.UpdateCoverageResults(
                    rd.sequencePoints == Microsoft.FSharp.Core.FSharpOption<PerDocumentSequencePoints>.None ? new PerDocumentSequencePoints() : rd.sequencePoints.Value,
                    rd.codeCoverageResults == Microsoft.FSharp.Core.FSharpOption<PerAssemblySequencePointsCoverage>.None ? new PerAssemblySequencePointsCoverage() : rd.codeCoverageResults.Value,
                    rd.executedTests == Microsoft.FSharp.Core.FSharpOption<PerTestIdResults>.None ? new PerTestIdResults() : rd.executedTests.Value)
            );
        }

        #endregion
    }
}
