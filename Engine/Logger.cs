using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;

namespace R4nd0mApps.TddStud10.Engine.Diagnostics
{
    [EventSource(Name = "R4nd0mApps-TddStud10-Engine")]
    internal sealed class Logger : EventSource
    {
        public static Logger I = new Logger();

        [Event(1, Level = EventLevel.Informational)]
        public void Log(string message)
        {
            WriteEvent(1, message);
        }

        [Event(2, Level = EventLevel.Error)]
        internal void LogError(string message)
        {
            base.WriteEvent(2, message);
        }

        [NonEvent]
        public void Log(string format, params object[] args)
        {
            if (IsEnabled())
            {
                Log(string.Format(format, args));
            }
        }

        [NonEvent]
        public void LogError(string format, params object[] args)
        {
            if (IsEnabled(EventLevel.Error, EventKeywords.All))
            {
                LogError(string.Format(format, args));
            }
        }
    }
}
