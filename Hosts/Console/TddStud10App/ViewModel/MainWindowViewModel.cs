using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using EditorUtils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.Engine.Core;

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

        public ObservableCollection<EditorTabViewModel> Tabs { get; set; }

        private EditorTabViewModel _selectedTab;
        public EditorTabViewModel SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                if (_selectedTab == value)
                {
                    return;
                }

                _selectedTab = value;
                RaisePropertyChanged(() => SelectedTab);
            }
        }

        public RelayCommand OpenFileCommand { get; set; }

        public RelayCommand OpenSolutionCommand { get; set; }

        public RelayCommand SaveAllCommand { get; set; }

        public RelayCommand EnableDisableTddStud10Command { get; set; }

        public RelayCommand CancelRunCommand { get; set; }

        private string _solutionPath;
        public string SolutionPath
        {
            get { return _solutionPath; }
            set
            {
                if (_solutionPath == value)
                {
                    return;
                }

                _solutionPath = value;
                RaisePropertyChanged(() => SolutionPath);
            }
        }

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

        private bool _currentRunCancelled;
        private readonly EditorHostLoader _editorHostLoader;
        private readonly EditorHost _editorHost;
        private IClassificationFormatMapService _classificationFormatMapService;
        private IUIServices _uiServices;

        public MainWindowViewModel()
            : this(new UIServices())
        {
        }

        protected MainWindowViewModel(IUIServices uiServices)
        {
            _uiServices = uiServices;
            _editorHostLoader = new EditorHostLoader();
            _editorHost = _editorHostLoader.EditorHost;
            _classificationFormatMapService = _editorHostLoader.CompositionContainer.GetExportedValue<IClassificationFormatMapService>();

            InitializeViewModelState();

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
                    RaisePropertyChanged(() => IsRunInProgress);
                });

            OpenFileCommand = new RelayCommand(ExecuteOpenFileCommand);

            OpenSolutionCommand = new RelayCommand(ExecuteOpenSolutionCommand);

            SaveAllCommand = new RelayCommand(ExecuteSaveAllCommand);
        }

        private void InitializeViewModelState()
        {
            RunState = RunState.Initial;

            SolutionPath = null;

            Tabs = new ObservableCollection<EditorTabViewModel>();
            RaisePropertyChanged(() => Tabs);

            SelectedTab = null;

            RaisePropertyChanged(() => EngineState);

            _consoleContents.Clear();
            RaisePropertyChanged(() => ConsoleContents);
            _stepResultAddendum.Clear();
            RaisePropertyChanged(() => StepResultAddendum);
        }

        private void ExecuteOpenFileCommand()
        {
            var file = _uiServices.OpenFile("All Files|*.*");
            if (file == null)
            {
                return;
            }

            OpenFile(file);
        }

        private void OpenFile(string filePath)
        {
            var textViewHost = CreateEditorForFile(filePath);

            var newTab = new EditorTabViewModel(filePath, textViewHost);
            Tabs.Add(newTab);
            RaisePropertyChanged(() => Tabs);
            SelectedTab = newTab;
            RaisePropertyChanged(() => SelectedTab);
        }

        private IWpfTextViewHost CreateEditorForFile(string filePath)
        {
            var textDocumentFactoryService = _editorHost.CompositionContainer.GetExportedValue<ITextDocumentFactoryService>();
            var textDocument = textDocumentFactoryService.CreateAndLoadTextDocument(filePath, _editorHost.ContentTypeRegistryService.GetContentType("code"));
            var roles = new[] 
                {
                    PredefinedTextViewRoles.PrimaryDocument,
                    PredefinedTextViewRoles.Document,
                    PredefinedTextViewRoles.Editable,
                    PredefinedTextViewRoles.Interactive,
                    PredefinedTextViewRoles.Structured,
                    PredefinedTextViewRoles.Analyzable,
                };
            var textViewRoleSet = _editorHostLoader.EditorHost.TextEditorFactoryService.CreateTextViewRoleSet(roles);
            var wpfTextView = _editorHostLoader.EditorHost.TextEditorFactoryService.CreateTextView(
                textDocument.TextBuffer,
                textViewRoleSet);
            var textViewHost = _editorHost.TextEditorFactoryService.CreateTextViewHost(wpfTextView, setFocus: true);
            return textViewHost;
        }

        private void ExecuteOpenSolutionCommand()
        {
            if (SolutionPath != null)
            {
                if (EngineLoader.IsRunInProgress())
                {
                    _uiServices.ShowMessageBox("Cannot close solution as a run is in progress");
                    return;
                }

                EngineLoader.DisableEngine();
                EngineLoader.Unload();
                InitializeViewModelState();
            }

            var slnPath = _uiServices.OpenFile("Solution Files|*.sln");
            if (slnPath == null)
            {
                return;
            }

            SolutionPath = slnPath;
            EngineLoader.Load(this, DataStore.Instance, slnPath, DateTime.UtcNow);
            EngineLoader.EnableEngine();
            RaisePropertyChanged(() => EngineState);
            CommandManager.InvalidateRequerySuggested();

            OpenFile(slnPath);
        }

        private void ExecuteSaveAllCommand()
        {
            foreach (var tab in Tabs)
            {
                var contents = tab.TextViewHost.TextView.TextBuffer.CurrentSnapshot.GetText();
                File.WriteAllText(tab.FilePath, contents);
            }
        }

        private void EnableOrDisable()
        {
            if (EngineLoader.IsEngineEnabled())
            {
                EngineLoader.DisableEngine();
            }
            else
            {
                EngineLoader.EnableEngine();
            }

            RaisePropertyChanged(() => EngineState);
        }

        private void AddTextToStepResultAddendum(Action<StringBuilder> textAdder)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    string oldStr = _stepResultAddendum.ToString();
                    _stepResultAddendum.Clear();
                    textAdder(_stepResultAddendum);
                    string newStr = _stepResultAddendum.ToString();
                    _stepResultAddendum.Clear();
                    _stepResultAddendum.AppendFormat("{0}{1}{2}", newStr, Environment.NewLine, oldStr);
                    RaisePropertyChanged(() => StepResultAddendum);
                });
        }

        private void AddTextToConsole(Action<StringBuilder> textAdder)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    textAdder(_consoleContents);
                    _consoleContents.AppendLine();
                    RaisePropertyChanged(() => ConsoleContents);
                });
        }

        private void ClearTextFields()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    _consoleContents.Clear();
                    RaisePropertyChanged(() => ConsoleContents);
                    _stepResultAddendum.Clear();
                    RaisePropertyChanged(() => StepResultAddendum);
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
                    RaisePropertyChanged(() => IsRunInProgress);
                    ClearTextFields();
                    AddTextToConsole(
                        sb => sb.AppendFormat("### Starting new run..."));
                });
        }

        public void RunStepStarting(RunStepStartingEventArg rsea)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    AddTextToConsole(
                        sb => sb.AppendFormat(
                            "### ### Starting run step : {0}...",
                            rsea.name.Item));
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
                            rss.kind.ToString()));
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
                            rss.name.Item));
                    AddTextToStepResultAddendum(
                        sb => sb.AppendFormat(
                            "### ### Additional run step info: {0}, {1}:{2}{3}",
                            rss.name.ToString(),
                            rss.kind.ToString(),
                            Environment.NewLine,
                            rss.addendum.ToString()));
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
                            e));
                });
        }

        public void RunEnded(RunData rd)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    AddTextToConsole(sb => sb.AppendFormat("### Ended run."));

                    CoverageData.Instance.UpdateCoverageResults(rd);
                    _currentRunCancelled = false;
                });
        }

        #endregion
    }
}
