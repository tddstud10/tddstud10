using System;
using System.IO;
using R4nd0mApps.TddStud10.Engine.Diagnostics;

namespace R4nd0mApps.TddStud10.Engine
{
    internal sealed class EngineFileSystemWatcher : IDisposable
    {
        private Action<DateTime, string> _action;

        private string _solutionPath;

        private DateTime _sessionStartTimestamp;

        public static EngineFileSystemWatcher Create(string solutionPath, DateTime sessionStartTimestamp, Action<DateTime, string> runEngine)
        {
            var efsWatcher = new EngineFileSystemWatcher();

            efsWatcher._solutionPath = solutionPath;
            efsWatcher._sessionStartTimestamp = sessionStartTimestamp;

            efsWatcher._action = runEngine;
            efsWatcher.fsWatcher = new FileSystemWatcher();
            efsWatcher.fsWatcher.Filter = "*";
            efsWatcher.fsWatcher.Path = Path.GetDirectoryName(solutionPath);
            // NOTE: Too many events otherwise. Let it be this way till we have figured out the right trigger mechanism.
            efsWatcher.fsWatcher.IncludeSubdirectories = false; 

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
            _action(_sessionStartTimestamp, _solutionPath);
        }

        void FsWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Logger.I.LogInfo("FSWatcher: Got created event");
            _action(_sessionStartTimestamp, _solutionPath);
        }

        void FsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Logger.I.LogInfo("FSWatcher: Got changed event");
            _action(_sessionStartTimestamp, _solutionPath);
        }

        void FsWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Logger.I.LogInfo("FSWatcher: Got renamed event");
            _action(_sessionStartTimestamp, _solutionPath);
        }

        void FsWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Logger.I.LogInfo("FSWatcher: Got deleted event");
            _action(_sessionStartTimestamp, _solutionPath);
        }

        #endregion
    }
}
