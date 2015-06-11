using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    [ProvideBindingPath]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "0.3.4.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [Guid(PkgGuids.GuidTddStud10Pkg)]
    public sealed class TddStud10Package : Package, IVsSolutionEvents, IEngineHost
    {
        private SynchronizationContext syncContext = SynchronizationContext.Current;

        private bool _disposed;

        private uint solutionEventsCookie;

        private IVsSolution2 _solution;
        private IVsStatusbar _statusBar;
        private EnvDTE.DTE _dte;

        private VsStatusBarIconHost _iconHost;

        public static TddStud10Package Instance { get; private set; }

        public void InvokeOnUIThread(Action action)
        {
            syncContext.Send(new SendOrPostCallback(_ => action()), null);
        }

        public string GetSolutionPath()
        {
            return _dte.Solution.FullName;
        }

        #region Package Members

        protected override void Initialize()
        {
            base.Initialize();

            _solution = Services.GetService<SVsSolution, IVsSolution2>();
            if (_solution != null)
            {
                _solution.AdviseSolutionEvents(this, out solutionEventsCookie).ThrowOnFailure();
            }

            _statusBar = Services.GetService<SVsStatusbar, IVsStatusbar>();

            _dte = Services.GetService<EnvDTE.DTE>();

            new PackageCommands(this).AddCommands();

            _iconHost = VsStatusBarIconHost.CreateAndInjectIntoVsStatusBar();

            Instance = this;

            Logger.I.LogInfo("Initialized Package successfully.");
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
            EngineLoader.Load(this, DataStore.Instance, GetSolutionPath(), DateTime.UtcNow);
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
                pfCancel = 1; // 1 => Veto closing of solution.
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
                () => CoverageData.Instance.UpdateCoverageResults(rd)
            );
        }

        #endregion
    }
}
