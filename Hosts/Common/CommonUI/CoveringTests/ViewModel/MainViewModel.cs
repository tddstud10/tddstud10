using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using R4nd0mApps.TddStud10.Common.Domain;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public partial class MainViewModel : ViewModelBase
    {
        private bool _popupVisible = false;

        public bool PopupVisible
        {
            get { return _popupVisible; }
            set
            {
                _popupVisible = value;
                RaisePropertyChanged(() => PopupVisible);
            }
        }

        private Geometry _glyphShape;

        public Geometry GlyphShape
        {
            get { return _glyphShape; }
            set
            {
                _glyphShape = value;
                RaisePropertyChanged(() => GlyphShape);
            }
        }

        private double _glyphStrokeThickness;

        public double GlyphStrokeThickness
        {
            get { return _glyphStrokeThickness; }
            set
            {
                _glyphStrokeThickness = value;
                RaisePropertyChanged(() => GlyphStrokeThickness);
            }
        }

        private SolidColorBrush _glyphColor;

        public SolidColorBrush GlyphColor
        {
            get { return _glyphColor; }
            set
            {
                _glyphColor = value;
                RaisePropertyChanged(() => GlyphColor);
            }
        }

        private ObservableCollection<CoveringTestViewModel> _coveringTests;

        public ObservableCollection<CoveringTestViewModel> CoveringTests
        {
            get { return _coveringTests; }
            set
            {
                _coveringTests = value;
                RaisePropertyChanged(() => CoveringTests);
            }
        }

        public RelayCommand ShowPopupCommand { get; set; }

        [PreferredConstructor]
        public MainViewModel()
            : this(
                  Geometry.Parse(string.Format("M 0 0 L 8 8 M 0 8 L 8 0")),
                  Colors.Green,
                  2.0,
                  DesignTimeData)
        {
        }

        public MainViewModel(Geometry glyphShape, Color color, double strokeThickness, IEnumerable<DTestResult> coveringTestResults)
        {
            ShowPopupCommand = new RelayCommand(
                () => 
                {
                    PopupVisible = CoveringTests != null && CoveringTests.Any();
                });

            GlyphShape = glyphShape;
            GlyphColor = new SolidColorBrush(color);
            GlyphStrokeThickness = strokeThickness;
            _coveringTests = new ObservableCollection<CoveringTestViewModel>(
                coveringTestResults.Select(it => new CoveringTestViewModel
                {
                    TestResult = it,
                    TestPassed = it.Outcome.Equals(DTestOutcome.TOPassed) ? true : it.Outcome.Equals(DTestOutcome.TOFailed) ? (bool?)false : null,
                    DisplayName = it.TestCase.FullyQualifiedName,
                    ErrorMessage = it.ErrorMessage,
                    ErrorStackTrace = it.ErrorStackTrace,
                }));
        }
    }
}