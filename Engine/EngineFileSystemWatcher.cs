using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R4nd0mApps.TddStud10.Engine.Diagnostics;

namespace R4nd0mApps.TddStud10.Engine
{
    internal sealed class EngineFileSystemWatcher : IDisposable
    {
        private Action<DateTime, string> action;

        private string solutionPath;

        public static EngineFileSystemWatcher Create(string solutionPath, Action<DateTime, string> runEngine)
        {
            var efsWatcher = new EngineFileSystemWatcher();

            efsWatcher.solutionPath = solutionPath;

            efsWatcher.action = runEngine;
            efsWatcher.fsWatcher = new FileSystemWatcher();
            efsWatcher.fsWatcher.Filter = "*";
            efsWatcher.fsWatcher.Path = Path.GetDirectoryName(solutionPath);

            efsWatcher.SubscribeToEvents();

            return efsWatcher;
        }

        private EngineFileSystemWatcher()
        {
        }

        internal bool IsEnabled()
        {
            return fsWatcher.EnableRaisingEvents;
        }

        public void Enable()
        {
            fsWatcher.EnableRaisingEvents = true;
        }

        internal void Disable()
        {
            fsWatcher.EnableRaisingEvents = false;
        }

        #region IDisposable

        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    UnsubscribeToEvents();
                    fsWatcher.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EngineFileSystemWatcher()
        {
            Dispose(false);
        }

        #endregion IDisposable

        #region FileSystemWatcher

        private FileSystemWatcher fsWatcher;

        private void SubscribeToEvents()
        {
            fsWatcher.Created += FsWatcher_Created;
            fsWatcher.Changed += FsWatcher_Changed;
            fsWatcher.Renamed += FsWatcher_Renamed;
            fsWatcher.Deleted += FsWatcher_Deleted;
            fsWatcher.Error += FsWatcher_Error;
        }

        private void UnsubscribeToEvents()
        {
            fsWatcher.Error -= FsWatcher_Error;
            fsWatcher.Deleted -= FsWatcher_Deleted;
            fsWatcher.Renamed -= FsWatcher_Renamed;
            fsWatcher.Changed -= FsWatcher_Changed;
            fsWatcher.Created -= FsWatcher_Created;
        }

        void FsWatcher_Error(object sender, ErrorEventArgs e)
        {
            Logger.I.LogError(e.ToString());
            action(DateTime.UtcNow, solutionPath);
        }

        void FsWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Logger.I.LogError("FSWatcher: Got created event");
            action(DateTime.UtcNow, solutionPath);
        }

        void FsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Logger.I.LogError("FSWatcher: Got changed event");
            action(DateTime.UtcNow, solutionPath);
        }

        void FsWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Logger.I.LogError("FSWatcher: Got renamed event");
            action(DateTime.UtcNow, solutionPath);
        }

        void FsWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Logger.I.LogError("FSWatcher: Got deleted event");
            action(DateTime.UtcNow, solutionPath);
        }

        #endregion
    }
}
