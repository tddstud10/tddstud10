using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
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

    // NOTE: This entity will continue to be alive till we figure out the final trigger mechanism(s)
    // Till then we will just have to carefully do/undo the pairs of functionality at appropriate places
    public static class EngineLoader
    {
        private static EngineFileSystemWatcher _efsWatcher;
        private static IEngineHost _host;
        private static TddStud10Runner _runner;
        private static Task _currentRun;
        private static CancellationTokenSource _currentRunCts;

        public static FSharpHandler<RunState> _runStateChangedHandler;
        public static FSharpHandler<RunData> _runStartingHandler;
        public static FSharpHandler<RunStepEventArg> _runStepStartingHandler;
        public static FSharpHandler<RunStepResult> _onRunStepErrorHandler;
        public static FSharpHandler<RunStepResult> _runStepEndedHandler;
        public static FSharpHandler<Exception> _onRunErrorHandler;
        public static FSharpHandler<RunData> _runEndedHandler;

        public static void Load(IEngineHost host, string solutionPath, DateTime sessionStartTimestamp)
        {
            Logger.I.LogInfo("Loading Engine with solution {0}", solutionPath);

            _host = host;

            _runStateChangedHandler = new FSharpHandler<RunState>((s, ea) => _host.RunStateChanged(ea));
            _runStartingHandler = new FSharpHandler<RunData>((s, ea) => _host.RunStarting(ea));
            _runStepStartingHandler = new FSharpHandler<RunStepEventArg>((s, ea) => _host.RunStepStarting(ea));
            _onRunStepErrorHandler = new FSharpHandler<RunStepResult>((s, ea) => _host.OnRunStepError(ea));
            _runStepEndedHandler = new FSharpHandler<RunStepResult>((s, ea) => _host.RunStepEnded(ea));
            _onRunErrorHandler = new FSharpHandler<Exception>((s, ea) => _host.OnRunError(ea));
            _runEndedHandler = new FSharpHandler<RunData>((s, ea) => _host.RunEnded(ea));

            _runner = _runner ?? TddStud10Runner.Create(host, Engine.CreateRunSteps());
            _runner.AttachHandlers(
                _runStateChangedHandler,
                _runStartingHandler,
                _runStepStartingHandler,
                _onRunStepErrorHandler,
                _runStepEndedHandler,
                _onRunErrorHandler,
                _runEndedHandler);

            _efsWatcher = EngineFileSystemWatcher.Create(solutionPath, sessionStartTimestamp, RunEngine);
        }

        public static bool IsEngineLoaded()
        {
            return _efsWatcher != null;
        }

        public static bool IsEngineEnabled()
        {
            var enabled = IsEngineLoaded() && _efsWatcher.IsEnabled();
            Logger.I.LogInfo("Engine is {0}", enabled);

            return enabled;
        }

        public static void EnableEngine()
        {
            Logger.I.LogInfo("Enabling Engine...");
            _efsWatcher.Enable();
        }

        public static void DisableEngine()
        {
            Logger.I.LogInfo("Disabling Engine...");
            _efsWatcher.Disable();
        }


        public static void Unload()
        {
            Logger.I.LogInfo("Unloading Engine...");

            _efsWatcher.Dispose();
            _efsWatcher = null;

            _runner.DetachHandlers(
                _runEndedHandler,
                _onRunErrorHandler,
                _runStepEndedHandler,
                _onRunStepErrorHandler,
                _runStepStartingHandler,
                _runStartingHandler,
                _runStateChangedHandler);

            _runEndedHandler = null;
            _onRunErrorHandler = null;
            _runStepEndedHandler = null;
            _onRunStepErrorHandler = null;
            _runStepStartingHandler = null;
            _runStartingHandler = null;
            _runStateChangedHandler = null;
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
