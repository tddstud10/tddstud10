using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;

namespace R4nd0mApps.TddStud10.Hosts.VS.Diagnostics
{
    [EventSource(Name = "R4nd0mApps-TddStud10-Hosts-VS")]
    public sealed class Logger : EventSource
    {
        public static Logger I = new Logger();

        public void Log(string line)
        {
            WriteEvent(1, line);
        }
    }
}
