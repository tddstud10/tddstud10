using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IEngineHost
    {
        private RunState _runState;
        public RunState RunState
        {
            get { return _runState; }
            set
            {
                if (_runState == value)
                {
                    return;
                }

                _runState = value;
                RaisePropertyChanged(() => RunState);
            }
        }

        private bool _currentRunCancelled;
        private Dictionary<string, StringBuilder> sbMap;

        public string SolutionPath { get; set; }

        private StringBuilder _consoleContents = new StringBuilder();
        public string ConsoleContents
        {
            get
            {
                return _consoleContents.ToString();
            }
        }


        private StringBuilder _stepResultAddendum = new StringBuilder();
        public string StepResultAddendum
        {
            get
            {
                return _stepResultAddendum.ToString();
            }
        }

        public RelayCommand EnableDisableTddStud10Command { get; set; }

        public RelayCommand CancelRunCommand { get; set; }

        public string EngineState
        {
            get
            {
                if (EngineLoader.IsEngineEnabled())
                {
                    return "Disable";
                }
                else
                {
                    return "Enable";
                }
            }
        }

        public bool IsRunInProgress
        {
            get
            {
                return !_currentRunCancelled && EngineLoader.IsRunInProgress();
            }
        }

        public MainWindowViewModel()
        {
            RunState = RunState.Initial;

            SolutionPath = @"d:\src\r4nd0mkatas\fizzbuzz\FizzBuzz.sln";
            EnableDisableTddStud10Command = new RelayCommand(
                EnableOrDisable,
                () =>
                {
                    return !string.IsNullOrWhiteSpace(SolutionPath);
                });
            CancelRunCommand = new RelayCommand(
                () =>
                {
                    _currentRunCancelled = true;
                    RaisePropertyChanged("IsRunInProgress");
                });
            sbMap = new Dictionary<string, StringBuilder>
            {
                {"ConsoleContents", _consoleContents},
                {"StepResultAddendum", _stepResultAddendum},
            };
        }

        private void EnableOrDisable()
        {
            if (EngineLoader.IsEngineEnabled())
            {
                EngineLoader.DisableEngine();
                RaisePropertyChanged("EngineState");
                return;
            }

            var slnPath = SolutionPath;
            if (EngineLoader.IsEngineLoaded())
            {
                EngineLoader.DisableEngine();
                EngineLoader.Unload();
            }
            EngineLoader.Load(this, slnPath, DateTime.UtcNow);

            EngineLoader.EnableEngine();
            RaisePropertyChanged("EngineState");
        }

        private void PrependTextToConsole(Action<StringBuilder> textAdder, string field)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    string oldStr = sbMap[field].ToString();
                    sbMap[field].Clear();
                    textAdder(sbMap[field]);
                    string newStr = sbMap[field].ToString();
                    sbMap[field].Clear();
                    sbMap[field].AppendFormat("{0}{1}{2}", newStr, Environment.NewLine, oldStr);
                    RaisePropertyChanged(field);
                });
        }

        private void AddTextToConsole(Action<StringBuilder> textAdder, string field)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    textAdder(sbMap[field]);
                    sbMap[field].AppendLine();
                    RaisePropertyChanged(field);
                });
        }

        private void ClearTextFields()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    _consoleContents.Clear();
                    RaisePropertyChanged("ConsoleContents");
                    _stepResultAddendum.Clear();
                    RaisePropertyChanged("StepResultAddendum");
                });
        }

        #region IEngineHost Members

        public bool CanContinue()
        {
            return !_currentRunCancelled;
        }

        public void RunStateChanged(RunState rs)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    RunState = rs;
                });
        }

        public void RunStarting(RunData rd)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    RaisePropertyChanged("IsRunInProgress");
                    ClearTextFields();
                    AddTextToConsole(
                        sb => sb.AppendFormat("### Starting new run..."),
                        "ConsoleContents");
                });
        }

        public void RunStepStarting(RunStepEventArg rsea)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    AddTextToConsole(
                        sb => sb.AppendFormat(
                            "### ### Starting run step : {0}...",
                            rsea.name.Item),
                        "ConsoleContents");
                });
        }

        public void OnRunStepError(RunStepResult rss)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    AddTextToConsole(
                        sb => sb.AppendFormat(
                            "### ### Error in step: {0}, {1}",
                            rss.name.ToString(),
                            rss.kind.ToString()),
                        "ConsoleContents");
                });
        }

        public void RunStepEnded(RunStepResult rss)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    AddTextToConsole(
                        sb => sb.AppendFormat(
                            "### ### Finished run step : {0}...",
                            rss.name.Item),
                        "ConsoleContents");
                    PrependTextToConsole(
                        sb => sb.AppendFormat(
                            "### ### Additional run step info: {0}, {1}:{2}{3}",
                            rss.name.ToString(),
                            rss.kind.ToString(),
                            Environment.NewLine,
                            rss.addendum.ToString()),
                        "StepResultAddendum");
                });
        }

        public void OnRunError(Exception e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    AddTextToConsole(
                        sb => sb.AppendFormat(
                            "### Run had error : {0}...",
                            e),
                        "ConsoleContents");
                });
        }

        public void RunEnded(RunData rd)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    AddTextToConsole(sb => sb.AppendFormat("### Ended run."),
                        "ConsoleContents");
                });
        }

        #endregion
    }
}
