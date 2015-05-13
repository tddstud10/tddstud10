using System;
using System.Globalization;
using System.Windows.Data;
using R4nd0mApps.TddStud10.Engine.Core;

namespace R4nd0mApps.TddStud10.Hosts.Common
{
    public class RunStateToAnimationStateConverter : IValueConverter
    {
        public static readonly bool AnimationOff = false;
        public static readonly bool AnimationOn = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rs = value as RunState;
            if (rs.IsEngineErrorDetected
                        || rs.IsBuildFailureDetected
                        || rs.IsFirstBuildRunning
                        || rs.IsBuildRunning
                        || rs.IsTestFailureDetected
                        || rs.IsTestRunning)
            {
                return AnimationOn;
            }
            else
            {
                return AnimationOff;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
