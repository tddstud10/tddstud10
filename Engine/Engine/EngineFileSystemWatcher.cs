using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Logger;
using System;
using System.IO;

namespace R4nd0mApps.TddStud10.Engine
{
    internal sealed class EngineFileSystemWatcher : IDisposable
    {
        private static ILogger Logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger;

        private Action<EngineLoaderParams> _action;

        private EngineLoaderParams _loaderParams;

        public static EngineFileSystemWatcher Create(EngineLoaderParams loaderParams, Action<EngineLoaderParams> runEngine)
        {
            var efsWatcher = new EngineFileSystemWatcher();

            efsWatcher._loaderParams = loaderParams;

            efsWatcher._action = runEngine;
            efsWatcher.fsWatcher = new FileSystemWatcher();
            efsWatcher.fsWatcher.Filter = "*";
            efsWatcher.fsWatcher.Path = Path.GetDirectoryName(loaderParams.SolutionPath.ToString());
            // NOTE: Too many events otherwise. Let it be this way till we have figured out the right trigger mechanism.
            efsWatcher.fsWatcher.IncludeSubdirectories = true;

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
            Logger.LogError(e.ToString());
            _action(_loaderParams);
        }

        void FsWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Logger.LogInfo("########: FSWatcher: Got created event: {0}", e.FullPath);
            _action(_loaderParams);
        }

        void FsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Logger.LogInfo("########: FSWatcher: Got changed event: {0}", e.FullPath);
            _action(_loaderParams);
        }

        void FsWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Logger.LogInfo("########: FSWatcher: Got renamed event: {0}", e.FullPath);
            _action(_loaderParams);
        }

        void FsWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Logger.LogInfo("########: FSWatcher: Got deleted event: {0}", e.FullPath);
            _action(_loaderParams);
        }

        #endregion
    }
}
