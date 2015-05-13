using System;
using System.Threading;
using System.Threading.Tasks;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Engine.Diagnostics;

namespace R4nd0mApps.TddStud10.Engine
{
    public interface IEngineHost : IRunExecutorHost
    {
        void RunStarting(RunData rd);

        void RunStepStarting(RunStepEventArg rsea);

        void OnRunStepError(RunStepResult rss);

        void RunStepEnded(RunStepResult rss);

        void OnRunError(Exception ex);

        void RunEnded(RunData rd);
    }

    public static class EngineLoader
    {
        private static EngineFileSystemWatcher efsWatcher;
        private static IEngineHost _host;
        private static TddStud10Runner _runner;
        private static Task _currentRun;
        private static CancellationTokenSource _currentRunCts;

        public static void Load(IEngineHost host, string solutionPath)
        {
            Logger.I.LogInfo("Loading Engine with solution {0}", solutionPath);

            _host = host;

            _runner = _runner ?? TddStud10Runner.Create(host, Engine.CreateRunSteps());
            _runner.AttachHandlers(
                _host.RunStateChanged,
                _host.RunStarting,
                _host.RunStepStarting,
                _host.OnRunStepError,
                _host.RunStepEnded,
                _host.OnRunError,
                _host.RunEnded);

            efsWatcher = EngineFileSystemWatcher.Create(solutionPath, RunEngine);
        }

        public static bool IsEngineLoaded()
        {
            return efsWatcher != null;
        }

        public static bool IsEngineEnabled()
        {
            var enabled = IsEngineLoaded() && efsWatcher.IsEnabled();
            Logger.I.LogInfo("Engine is {0}", enabled);

            return enabled;
        }

        public static void EnableEngine()
        {
            Logger.I.LogInfo("Enabling Engine...");
            efsWatcher.Enable();
        }

        public static void DisableEngine()
        {
            Logger.I.LogInfo("Disabling Engine...");
            efsWatcher.Disable();
        }

        public static void Unload()
        {
            Logger.I.LogInfo("Unloading Engine...");

            efsWatcher.Dispose();
            efsWatcher = null;

            _runner.DetachHandlers(
                _host.RunEnded,
                _host.OnRunError,
                _host.RunStepEnded,
                _host.OnRunStepError,
                _host.RunStepStarting,
                _host.RunStarting,
                _host.RunStateChanged);
        }

        public static bool IsRunInProgress()
        {
            if (_currentRun == null
                || (_currentRun.Status == TaskStatus.Canceled
                    || _currentRun.Status == TaskStatus.Faulted
                    || _currentRun.Status == TaskStatus.RanToCompletion))
            {
                return false;
            }

            return true;
        }

        private static void RunEngine(DateTime runStartTime, string solutionPath)
        {
            if (_runner != null)
            {
                InvokeEngine(runStartTime, solutionPath);
            }
            else
            {
                Logger.I.LogInfo("Engine is not loaded. Ignoring command.");
            }
        }

        private static void InvokeEngine(DateTime runStartTime, string solutionPath)
        {
            try
            {
                if (!_host.CanContinue())
                {
                    Logger.I.LogInfo("Cannot start engine. Host has denied request.");
                    return;
                }

                if (IsRunInProgress())
                {
                    Logger.I.LogInfo("Cannot start engine. A run is already in progress.");
                    return;
                }

                Logger.I.LogInfo("--------------------------------------------------------------------------------");
                Logger.I.LogInfo("EngineLoader: Going to trigger a run.");
                // NOTE: Note fix the CT design once we wire up.
                if (_currentRunCts != null)
                {
                    _currentRunCts.Dispose();
                }
                _currentRunCts = new CancellationTokenSource();
                _currentRun = _runner.StartAsync(runStartTime, solutionPath, _currentRunCts.Token);
            }
            catch (Exception e)
            {
                Logger.I.LogError("Exception thrown in InvokeEngine: {0}.", e);
            }
        }
    }
}
