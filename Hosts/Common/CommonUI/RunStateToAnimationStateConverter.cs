using System;
using System.Globalization;
using System.Windows.Data;
using R4nd0mApps.TddStud10.Engine.Core;

namespace R4nd0mApps.TddStud10.Hosts.Common
{
    public class RunStateToAnimationStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rs = value as RunState;
            return (rs.IsEngineErrorDetected
                        || rs.IsBuildFailureDetected
                        || rs.IsFirstBuildRunning
                        || rs.IsBuildRunning
                        || rs.IsTestFailureDetected
                        || rs.IsTestRunning);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
