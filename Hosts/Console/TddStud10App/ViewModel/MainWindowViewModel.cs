using System;
using System.Text;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Engine.Core;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IEngineHost
    {
        private bool _canContinueRun;

        public string SolutionPath { get; set; }

        private StringBuilder _consoleContents = new StringBuilder();
        public string ConsoleContents
        {
            get
            {
                return _consoleContents.ToString();
            }
        }

        public RelayCommand EnableDisableTddStud10Command { get; set; }

        public MainWindowViewModel()
        {
            SolutionPath = @"d:\src\r4nd0mkatas\fizzbuzz\FizzBuzz.sln";
            EnableDisableTddStud10Command = new RelayCommand(
                Start,
                () =>
                {
                    return !string.IsNullOrWhiteSpace(SolutionPath);
                });
        }

        private void Start()
        {
            if (EngineLoader.IsRunInProgress())
            {
                var res = MessageBox.Show("Run in progress. Cancel?", "TDD Studio", MessageBoxButton.OKCancel);
                _canContinueRun = res == MessageBoxResult.Cancel;
                return;
            }

            _canContinueRun = true;

            var slnPath = SolutionPath;
            if (EngineLoader.IsEngineLoaded())
            {
                EngineLoader.DisableEngine();
                EngineLoader.Unload();
            }
            EngineLoader.Load(this, slnPath);

            EngineLoader.EnableEngine();
        }

        private void AddTextToConsole(Action<StringBuilder> textAdder)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    _consoleContents.AppendLine();
                    textAdder(_consoleContents);
                    RaisePropertyChanged("ConsoleContents");
                });
        }

        #region IEngineHost Members

        public bool CanContinue()
        {
            return _canContinueRun;
        }

        public bool CanStart()
        {
            return CanContinue();
        }

        public void RunStarting(RunData rd)
        {
            AddTextToConsole(
                sb => sb.AppendFormat("### Starting new run..."));
        }

        public void RunStepStarting(string stepDetails, RunData rd)
        {
            AddTextToConsole(
                sb => sb.AppendFormat(
                    "### ### Starting run step : {0}...",
                    stepDetails));
        }

        public void RunStepEnded(string stepDetails, RunData rd)
        {
            AddTextToConsole(
                sb => sb.AppendFormat(
                    "### ### Ended run step : {0}...",
                    stepDetails));
        }

        public void OnRunError(Exception e)
        {
            AddTextToConsole(
                sb => sb.AppendFormat(
                    "### Run had an error: {0}.",
                    e));
        }

        public void RunEnded(RunData rd)
        {
            AddTextToConsole(sb => sb.AppendFormat("### Ended run."));
        }

        #endregion
    }
}
