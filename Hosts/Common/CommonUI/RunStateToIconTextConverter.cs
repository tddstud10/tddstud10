using System;
using System.Globalization;
using System.Windows.Data;
using R4nd0mApps.TddStud10.Engine.Core;

namespace R4nd0mApps.TddStud10.Hosts.Common
{
    public class RunStateToIconTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rs = value as RunState;
            if (rs.IsFirstBuildRunning
                || rs.IsBuildRunning
                || rs.IsBuildFailureDetected
                || rs.IsBuildFailed
                || rs.IsBuildPassed)
            {
                return "B";
            }
            else if (rs.IsTestRunning
                || rs.IsTestFailureDetected
                || rs.IsTestFailed
                || rs.IsTestPassed)
            {
                return "T";
            }
            else if (rs.IsInitial
                || rs.IsEngineError
                || rs.IsEngineErrorDetected)
            {
                return "?";
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
