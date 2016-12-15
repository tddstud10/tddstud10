using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using R4nd0mApps.TddStud10;

namespace RealTimeEtwListener
{
    public static class Program
    {
        private const string SessionName = Constants.RealTimeSessionName;

        private static readonly IReadOnlyDictionary<TraceEventLevel, ConsoleColor> eventLevelColorMap = new Dictionary<TraceEventLevel, ConsoleColor>()
        {
            { TraceEventLevel.Always, ConsoleColor.DarkGray }, // Not meant to use Always
            { TraceEventLevel.Critical, ConsoleColor.Red },
            { TraceEventLevel.Error, ConsoleColor.Red },
            { TraceEventLevel.Informational, ConsoleColor.White },
            { TraceEventLevel.Verbose, ConsoleColor.Gray },
            { TraceEventLevel.Warning, ConsoleColor.Yellow },
        };

        public static void Main(string[] args)
        {
            if (!(TraceEventSession.IsElevated() ?? false))
            {
                Console.WriteLine("To turn on ETW events you need to be Administrator, please run from an Admin process.");
                return;
            }

            StartTraceSession();
        }

        private static void StartTraceSession()
        {
            Console.WriteLine("Creating a '{0}' session", SessionName);
            using (var session = new TraceEventSession(SessionName, null /* = Realtime session */))
            {
                session.StopOnDispose = true;

                Console.Title = Constants.ProductName;
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) { session.Dispose(); };

                using (var source = new ETWTraceEventSource(SessionName, TraceEventSourceType.Session))
                {
                    new DynamicTraceEventParser(source).All += delegate (TraceEvent data)
                    {
                        ProcessTraceEvent(data);

                        if (data.EventName == "Stop")
                        {
                            source.StopProcessing();
                        }
                    };

                    EnableProviders(session);

                    Console.WriteLine("Staring Listing for events");
                    source.Process();
                    Console.WriteLine();
                    Console.WriteLine("Stopping the collection of events.");
                }
            }
        }

        private static void EnableProviders(TraceEventSession session)
        {
            session.EnableProvider(Constants.EtwProviderNameAllLogs);
        }

        private static void ProcessTraceEvent(TraceEvent data)
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

            WriteStringWithColor(data.Level, sb.ToString().Trim().Trim(','));
        }

        private static void WriteStringWithColor(TraceEventLevel eventLevel, string str)
        {
            var originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = eventLevelColorMap[eventLevel];
                Console.WriteLine(str);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
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
