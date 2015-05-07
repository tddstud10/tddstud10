using System;
using System.Threading.Tasks;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Engine.Diagnostics;

namespace R4nd0mApps.TddStud10.Engine
{
    // TODO: Move to fs
    public interface IEngineHost : IRunExecutorHost
    {
        bool CanStart();

        void RunStarting(RunData rd);

        void RunStepStarting(string runStepName, RunData rd);

        void RunStepEnded(string runStepName, RunData rd);

        void OnRunError(Exception ex);

        void RunEnded(RunData rd);
    }

    // TODO: Cleanup: Move to fs
    // TODO: Cleanup: Remove the ugly delegates
    public static class EngineLoader
    {
        private static EventHandler<RunData> runStartingHandler;
        private static EventHandler<Tuple<string, RunData>> runStepStartingHandler;
        private static EventHandler<Tuple<string, RunData>> runStepEndedHandler;
        private static EventHandler<Exception> onRunErrorHandler;
        private static EventHandler<RunData> runEndedHandler;
        private static EngineFileSystemWatcher efsWatcher;
        private static IEngineHost _host;
        private static TddStud10Runner _runner;
        private static Task _currentRun;

        public static void Load(IEngineHost host, string solutionPath)
        {
            Logger.I.LogInfo("Loading Engine with solution {0}", solutionPath);

            _host = host;
            runStartingHandler = (o, ea) => host.RunStarting(ea);
            runStepStartingHandler = (o, ea) => host.RunStepStarting(ea.Item1, ea.Item2);
            runStepEndedHandler = (o, ea) => host.RunStepEnded(ea.Item1, ea.Item2);
            onRunErrorHandler = (o, ea) => host.OnRunError(ea);
            runEndedHandler = (o, ea) => host.RunEnded(ea);

            _runner = _runner ?? TddStud10Runner.Create(host, Engine.CreateRunSteps());
            _runner.AttachHandlers(
                _host.RunStarting,
                ea => _host.RunStepStarting(ea.Item1.Item, ea.Item2),
                ea => _host.RunStepEnded(ea.Item1.Item, ea.Item2),
                _host.OnRunError,
                _host.RunEnded);

            efsWatcher = EngineFileSystemWatcher.Create(solutionPath, RunEngine);
        }

        private static void OnRunEnded(object sender, RunData e)
        {
            _runner = null;
            CoverageData.Instance.UpdateCoverageResults(e.sequencePoints.Value, e.codeCoverageResults.Value, e.executedTests.Value);
        }

        public static bool IsEngineLoaded()
        {
            return efsWatcher != null;
        }

        public static bool IsEngineEnabled()
        {
            var enabled = efsWatcher.IsEnabled();
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
                ea => _host.RunStepEnded(ea.Item1.Item, ea.Item2),
                ea => _host.RunStepStarting(ea.Item1.Item, ea.Item2),
                _host.RunStarting);

            runStartingHandler = null;
            runStepStartingHandler = null;
            runStepEndedHandler = null;
            onRunErrorHandler = null;
            runEndedHandler = null;

        }

        public static bool IsRunInProgress()
        {
            if (_currentRun != null
                && (_currentRun.Status == TaskStatus.Canceled
                    || _currentRun.Status == TaskStatus.Faulted
                    || _currentRun.Status == TaskStatus.RanToCompletion))
            {
                return true;
            }

            return false;
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
                if (!_host.CanStart())
                {
                    Logger.I.LogInfo("Cannot start engine. Host has denied request.");
                    return;
                }

                _currentRun = _runner.StartAsync(runStartTime, solutionPath);
            }
            catch (Exception e)
            {
                Logger.I.LogError("Exception thrown in InvokeEngine: {0}.", e);
            }
        }
    }
}
