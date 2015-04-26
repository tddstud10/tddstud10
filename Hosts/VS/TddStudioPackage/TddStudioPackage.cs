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
using R4nd0mApps.TddStud10.Hosts.VS.Diagnostics;
using System.IO;
using R4nd0mApps.TddStud10.Engine;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [Guid(GuidList.guidTddStud10Pkg)]
    public sealed class TddStud10Package : Package, IVsSolutionEvents
    {
        private bool _disposed;

        private uint solutionEventsCookie;

        private IVsSolution2 solution = null;

        public TddStud10Package()
        {
        }

        public static TddStud10Package Instance
        {
            get;
            private set;
        }

        #region Package Members

        protected override void Initialize()
        {
            base.Initialize();

            solution = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution)) as IVsSolution2;
            if (solution != null)
            {
                solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            }

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(new Guid(GuidList.guidProgressBarCmdSetString), (int)PkgCmdIDList.cmdidProgressBar);
                MenuCommand menuItem = new MenuCommand(TriggerTddStudioRun, menuCommandID);
                mcs.AddCommand(menuItem);
            }

            Instance = this;
        }

        private void TriggerTddStudioRun(object sender, EventArgs e)
        {
            Logger.I.Log("Called Trigger TddStud10");
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                if (solution != null && solutionEventsCookie != 0)
                {
                    solution.UnadviseSolutionEvents(solutionEventsCookie);
                }
                solution = null;

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
            EngineLoader.Load(solutionPath);

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
    }
}
