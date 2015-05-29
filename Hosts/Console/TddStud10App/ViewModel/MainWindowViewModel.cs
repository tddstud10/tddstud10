using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using EditorUtils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Win32;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine;

namespace R4nd0mApps.TddStud10.Hosts.Console.TddStud10App.ViewModel
{
    public class EditorTabViewModel
    {
        public EditorTabViewModel(string tabName, IWpfTextViewHost textViewHost)
        {
            TabName = tabName;
            TextViewHost = textViewHost;
        }

        public string TabName { get; private set; }

        public IWpfTextViewHost TextViewHost { get; private set; }

        public object TabData { get { return TextViewHost.HostControl; } }
    }

    public static class Constants
    {
        public static readonly FontFamily FontFamily = new FontFamily("Consolas");
        public static readonly double FontSize = 9;
    }

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

        public ObservableCollection<EditorTabViewModel> Tabs { get; set; }

        public RelayCommand OpenFileCommand { get; set; }

        public RelayCommand OpenSolutionCommand { get; set; }

        public RelayCommand SaveFileCommand { get; set; }

        public RelayCommand EnableDisableTddStud10Command { get; set; }

        public RelayCommand CancelRunCommand { get; set; }

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

        public MainWindowViewModel()
        {
            RunState = RunState.Initial;

            Tabs = new ObservableCollection<EditorTabViewModel>();

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
                    RaisePropertyChanged(() => IsRunInProgress);
                });

            OpenFileCommand = new RelayCommand(ExecuteOpenFileCommand);

            OpenSolutionCommand = new RelayCommand(ExecuteOpenSolutionCommand);

            SaveFileCommand = new RelayCommand(ExecuteSaveFileCommand);

            _editorHostLoader = new EditorHostLoader();
            _editorHost = _editorHostLoader.EditorHost;
            _classificationFormatMapService = _editorHostLoader.CompositionContainer.GetExportedValue<IClassificationFormatMapService>();
        }

        private void ExecuteOpenFileCommand()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            if ((bool)openFileDialog.ShowDialog())
            {
                // TODO: Get a real content type
                var filePath = openFileDialog.FileName;
                var fileName = Path.GetFileName(filePath);
                var textDocumentFactoryService = _editorHost.CompositionContainer.GetExportedValue<ITextDocumentFactoryService>();
                var textDocument = textDocumentFactoryService.CreateAndLoadTextDocument(filePath, _editorHost.ContentTypeRegistryService.GetContentType("code"));
                var wpfTextView = CreateTextView(textDocument.TextBuffer);

                var newTab = new EditorTabViewModel(fileName, CreateTextViewHost(wpfTextView));
                Tabs.Add(newTab);
                RaisePropertyChanged(() => Tabs);
                SelectedTab = newTab;
                RaisePropertyChanged(() => SelectedTab);
            }
        }

        private IWpfTextView CreateTextView(ITextBuffer textBuffer)
        {
            return CreateTextView(
                textBuffer,
                PredefinedTextViewRoles.PrimaryDocument,
                PredefinedTextViewRoles.Document,
                PredefinedTextViewRoles.Editable,
                PredefinedTextViewRoles.Interactive,
                PredefinedTextViewRoles.Structured,
                PredefinedTextViewRoles.Analyzable);
        }

        private IWpfTextView CreateTextView(ITextBuffer textBuffer, params string[] roles)
        {
            var textViewRoleSet = _editorHostLoader.EditorHost.TextEditorFactoryService.CreateTextViewRoleSet(roles);
            var textView = _editorHostLoader.EditorHost.TextEditorFactoryService.CreateTextView(
                textBuffer,
                textViewRoleSet);

            return textView;
        }

        private IWpfTextViewHost CreateTextViewHost(IWpfTextView textView)
        {
            var textViewHost = _editorHost.TextEditorFactoryService.CreateTextViewHost(textView, setFocus: true);

            return textViewHost;
        }

        private void ExecuteOpenSolutionCommand()
        {
            MessageBox.Show("Open Solution");
        }

        private void ExecuteSaveFileCommand()
        {
            MessageBox.Show("Save File");
        }

        private void EnableOrDisable()
        {
            if (EngineLoader.IsEngineEnabled())
            {
                EngineLoader.DisableEngine();
                RaisePropertyChanged(() => EngineState);
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

        public void RunStepStarting(RunStepEventArg rsea)
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
                });
        }

        #endregion
    }
}
