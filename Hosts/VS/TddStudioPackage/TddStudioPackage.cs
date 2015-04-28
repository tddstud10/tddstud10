using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using R4nd0mApps.TddStud10.Hosts.VS.Helpers;
using System.IO;
using R4nd0mApps.TddStud10.Engine;
using System.Windows.Controls;
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "0.1.4.4", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [Guid(GuidList.guidTddStud10Pkg)]
    public sealed class TddStud10Package : Package, IVsSolutionEvents
    {
        private Control _uiThreadInvoker;

        private bool _disposed;

        private uint solutionEventsCookie;

        private IVsSolution2 _solution = null;
        private IVsStatusbar _statusBar;

        public static TddStud10Package Instance { get; private set; }

        public DateTime LoadTimestamp { get; private set; }

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

            LoadTimestamp = DateTime.UtcNow;

            _uiThreadInvoker = new Control();

            _solution = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution)) as IVsSolution2;
            if (_solution != null)
            {
                _solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            }

            _statusBar = ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                new Dictionary<uint, EventHandler>
                {
                    { PkgCmdIDList.cmdidEnableTddStud10, EnableTddStud10 },
                    { PkgCmdIDList.cmdidDisableTddStud10, DisableTddStud10 },
                }.Aggregate(
                    new KeyValuePair<uint, EventHandler>(),
                    (_, kvp) => 
                    {
                        // Create the command for the menu item.
                        CommandID menuCommandID = new CommandID(new Guid(GuidList.guidProgressBarCmdSetString), (int)kvp.Key);
                        MenuCommand menuItem = new MenuCommand(kvp.Value, menuCommandID);
                        mcs.AddCommand(menuItem); 
                        return kvp;
                    });
            }

            Instance = this;

            Logger.I.Log("Initialized Package. Load timestamp {0}.", LoadTimestamp);
        }

        private void EnableTddStud10(object sender, EventArgs e)
        {
            Logger.I.Log("Enabling TddStud10...");

            EngineLoader.EnableEngine();
        }

        private void DisableTddStud10(object sender, EventArgs e)
        {
            Logger.I.Log("Disabling TddStud10...");

            EngineLoader.DisableEngine();
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
            var solutionPath = ((Package.GetGlobalService(typeof(EnvDTE.DTE))) as EnvDTE.DTE).Solution.FullName;
            EngineLoader.Load(LoadTimestamp, solutionPath, RunStarting, RunStepStarting, RunEnded);

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            EngineLoader.Unload();

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
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion

        private void RunStepStarting(string stepDetails)
        {
            InvokeOnUIThread(() => {
                _statusBar.SetText(stepDetails);
            });
        }

        private void RunStarting()
        {
            InvokeOnUIThread(() => {
                _statusBar.SetText(string.Empty);
                _statusBar.Animation(1, (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Synch);
            });
        }

        private void RunEnded()
        {
            InvokeOnUIThread(() =>
            {
                _statusBar.Animation(0, (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Synch);
                _statusBar.SetText(string.Empty);
            });
        }
    }
}
