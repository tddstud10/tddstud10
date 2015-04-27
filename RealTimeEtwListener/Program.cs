using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace RealTimeEtwListener
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (!(TraceEventSession.IsElevated() ?? false))
            {
                Console.WriteLine("To turn on ETW events you need to be Administrator, please run from an Admin process.");
                return;
            }

            var sessionName = "R4nd0mApps-TddStud10-Realtime-Session";
            Console.WriteLine("Creating a '{0}' session", sessionName);
            using (var session = new TraceEventSession(sessionName, null /* = Realtime session */))
            {
                session.StopOnDispose = true;

                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) { session.Dispose(); };

                using (var source = new ETWTraceEventSource(sessionName, TraceEventSourceType.Session))
                {
                    var parser = new DynamicTraceEventParser(source);
                    parser.All += delegate(TraceEvent data)
                    {
                        if (data.EventName == "ManifestData")
                        {
                            return;
                        }

                        var sb = new StringBuilder();
                        sb.AppendFormat("{0}-{1}-{2}: ", data.TimeStamp.ToString("mm:ss.fff"), data.ProcessID.ToString("D5"), data.ThreadID.ToString("D5"));
                        var pns = from pn in data.PayloadNames
                                where pn != "MSec" && pn != "PID" && pn != "TID"
                                select pn;
                        sb = pns.Aggregate(sb, (acc, pn) => acc.AppendFormat("{0}, ", data.GetValue(pn)));
                        Console.WriteLine(sb.ToString());

                        if (data.EventName == "Stop")
                        {
                            source.StopProcessing();
                        }
                    };

                    session.EnableProvider("R4nd0mApps-TddStud10-Hosts-VS");
                    session.EnableProvider("R4nd0mApps-TddStud10-Hosts-Console");
                    session.EnableProvider("R4nd0mApps-TddStud10-TestHost");
                    session.EnableProvider("R4nd0mApps-TddStud10-Engine");

                    Console.WriteLine("Staring Listing for events");
                    source.Process();
                    Console.WriteLine();
                    Console.WriteLine("Stopping the collection of events.");
                }
            }
        }

        private static object GetValue(this TraceEvent data, string payLoadName)
        {
            var index = data.PayloadIndex(payLoadName);
            if (index < 0)
            {
                return string.Format("Value with name '{0}' not found.", payLoadName);
            }

            return data.PayloadValue(index);
        }
    }
}
