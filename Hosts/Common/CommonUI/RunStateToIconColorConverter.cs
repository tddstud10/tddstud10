using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.Hosts.Common
{
    public class RunStateToIconColorConverter : IValueConverter
    {
        public static readonly Color ColorForUnknown = Colors.DarkGray;
        public static readonly Color ColorForRed = new Color { A = 0xFF, R = 229, G = 20, B = 0 };
        public static readonly Color ColorForGreen = new Color { A = 0xFF, R = 51, G = 153, B = 51 };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rs = value as RunState;
            if (rs.IsInitial
                || rs.IsEngineError
                || rs.IsEngineErrorDetected
                || rs.IsFirstBuildRunning)
            {
                return new SolidColorBrush(ColorForUnknown);
            }
            else if (rs.IsBuildFailureDetected
                || rs.IsBuildFailed
                || rs.IsTestFailureDetected
                || rs.IsTestFailed)
            {
                return new SolidColorBrush(ColorForRed);
            }
            else if (rs.IsBuildRunning
                || rs.IsBuildPassed
                || rs.IsTestRunning
                || rs.IsTestPassed)
            {
                return new SolidColorBrush(ColorForGreen);
            }
            else
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
