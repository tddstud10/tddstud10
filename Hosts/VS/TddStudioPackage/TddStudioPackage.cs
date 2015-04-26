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

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [Guid(GuidList.TddStud10PackageId)]
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

            Instance = this;
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
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
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
