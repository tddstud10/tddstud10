using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.TestExecution.Adapters;
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using R4nd0mApps.TddStud10.TestRuntime;
using Server;

namespace R4nd0mApps.TddStud10.TestHost
{
    public class Program
    {
        private static string codeCoverageStore;
        private static string testResultsStore;
        private static string discoveredUnitTestsStore;

        private static void LogInfo(string format, params object[] args)
        {
            Logger.I.LogInfo(format, args);
        }

        private static void LogError(string format, params object[] args)
        {
            Logger.I.LogError(format, args);
        }

        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static int Main(string[] args)
        {
            LogInfo("TestHost: Entering Main.");
            bool allTestsPassed = true;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);
            codeCoverageStore = args[2];
            testResultsStore = args[3];
            discoveredUnitTestsStore = args[4];
            var ccServer = new CoverageDataCollector();
            using (ServiceHost serviceHost = new ServiceHost(ccServer))
            {
                LogInfo("TestHost: Created Service Host.");
                string address = Marker.CreateCodeCoverageDataCollectorEndpointAddress();
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                serviceHost.AddServiceEndpoint(typeof(ICoverageDataCollector), binding, address);
                serviceHost.Open();
                LogInfo("TestHost: Opened channel.");

                allTestsPassed = RunTests();
                LogInfo("TestHost: Finished running test cases.");
            }
            ccServer.SaveTestCases(codeCoverageStore);

            LogInfo("TestHost: Exiting Main.");
            return allTestsPassed ? 0 : 1;
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError("Exception thrown in InvokeEngine: {0}.", e.ExceptionObject);
        }

        private static bool RunTests()
        {
            Stopwatch stopWatch = new Stopwatch();
            TimeSpan ts;
            string elapsedTime;

            LogInfo("TestHost executing tests...");
            stopWatch.Start();
            var testResults = new PerTestIdResults();
            var utAssemblies = PerAssemblyTestIds.Deserialize(discoveredUnitTestsStore);
            Parallel.ForEach(
                utAssemblies.Keys,
                asm =>
                {
                    LogInfo("Executing tests in {0}: Start.", asm);
                    var exec = new XUnitTestExecutor();
                    exec.TestExecuted.AddHandler(new FSharpHandler<TestResult>((o, ea) => NoteTestResults(testResults, ea)));
                    exec.ExecuteTests(asm);
                    LogInfo("Executing tests in {0}: Done.", asm);
                });

            testResults.Serialize(testResultsStore);

            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            LogInfo("Done TestHost executing tests! [" + elapsedTime + "]");
            LogInfo("");

            var rrs =
                from tr in testResults
                from rr in tr.Value
                where rr.result == TestOutcome.Failed
                select rr;

            return rrs.FirstOrDefault() == null;
        }

        private static void NoteTestResults(PerTestIdResults testResults, TestResult ea)
        {
            Console.WriteLine("Test: {0} - {1}", ea.DisplayName, ea.Outcome);

            var testId = new TestId(
                FilePath.NewFilePath(ea.TestCase.Source),
                FilePath.NewFilePath(ea.TestCase.CodeFilePath),
                DocumentCoordinate.NewDocumentCoordinate(ea.TestCase.LineNumber));

            var results = testResults.GetOrAdd(testId, _ => new ConcurrentBag<TestRunResult>());

            results.Add(new TestRunResult(ea.Outcome));
        }
    }
}
