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

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IEngineHost
    {
        private bool _animation;
        public bool Animation
        {
            get { return _animation; }

            set
            {
                if (_animation == value)
                {
                    return;
                }

                _animation = value;
                RaisePropertyChanged(() => Animation);
            }
        }

        private char _stepKind;
        public char StepKind 
        { 
            get { return _stepKind; }
            set 
            {
                if (_stepKind == value)
                {
                    return;
                }

                _stepKind = value;
                RaisePropertyChanged(() => StepKind);
            }
        }

        private SolidColorBrush _rectangleColor;
        public SolidColorBrush RectangleColor 
        { 
            get { return _rectangleColor; }
            set 
            {
                if (_rectangleColor == value)
                {
                    return;
                }

                _rectangleColor = value;
                RaisePropertyChanged(() => RectangleColor);
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
            Animation = false;
            StepKind = '?';
            RectangleColor = new SolidColorBrush(Colors.LightGray);

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
            EngineLoader.Load(this, slnPath);

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

        public bool CanStart()
        {
            return CanContinue();
        }

        public void RunStateChanged(RunState rs)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    // Animating or not
                    Animation = (rs.IsEngineErrorDetected
                        || rs.IsBuildFailureDetected
                        || rs.IsFirstBuildRunning
                        || rs.IsBuildRunning
                        || rs.IsTestFailureDetected
                        || rs.IsTestRunning);
                    
                    // Build or Test
                    if (rs.IsFirstBuildRunning 
                        || rs.IsBuildRunning
                        || rs.IsBuildFailureDetected
                        || rs.IsBuildFailed
                        || rs.IsBuildPassed)
                    {
                        StepKind = 'B';
                    }
                    else if (rs.IsTestRunning
                        || rs.IsTestFailureDetected
                        || rs.IsTestFailed
                        || rs.IsTestPassed)
                    {
                        StepKind = 'T';
                    }
                    else if (rs.IsInitial
                        || rs.IsEngineError
                        || rs.IsEngineErrorDetected)
                    {
                        StepKind = '?';
                    }
                    else
                    {
                        Debug.Assert(false, "Cannot set step kind for the current state.");
                    }
                    
                    // Unknown or Red or Green
                    if (rs.IsInitial
                        || rs.IsEngineError
                        || rs.IsEngineErrorDetected
                        || rs.IsFirstBuildRunning)
                    {
                        RectangleColor = new SolidColorBrush(Colors.LightGray);
                    }
                    else if (rs.IsBuildFailureDetected 
                        || rs.IsBuildFailed
                        || rs.IsTestFailureDetected
                        || rs.IsTestFailed)
                    {
                        RectangleColor = new SolidColorBrush(Colors.Red);
                    }
                    else if (rs.IsBuildRunning
                        || rs.IsBuildPassed
                        || rs.IsTestRunning
                        || rs.IsTestPassed)
                    {
                        RectangleColor = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        Debug.Assert(false, "Cannot set a color for the current state.");
                    }
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
