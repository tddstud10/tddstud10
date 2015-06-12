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

namespace R4nd0mApps.TddStud10.TestHost
{
    public class Program
    {
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
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);
            var command = args[1];
            var codeCoverageStore = args[2];
            var testResultsStore = args[3];
            var discoveredUnitTestsStore = args[4];

            var allTestsPassed = Debugger.IsAttached
                ? RunTests(testResultsStore, discoveredUnitTestsStore)
                : ExecuteTestWithCoverageDataCollection(() => RunTests(testResultsStore, discoveredUnitTestsStore), codeCoverageStore);

            LogInfo("TestHost: Exiting Main.");
            return allTestsPassed ? 0 : 1;
        }

        private static bool ExecuteTestWithCoverageDataCollection(Func<bool> runTests, string codeCoverageStore)
        {
            bool allTestsPassed = true;
            var ccServer = new CoverageDataCollector();
            using (ServiceHost serviceHost = new ServiceHost(ccServer))
            {
                LogInfo("TestHost: Created Service Host.");
                string address = Marker.CreateCodeCoverageDataCollectorEndpointAddress();
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                serviceHost.AddServiceEndpoint(typeof(ICoverageDataCollector), binding, address);
                serviceHost.Open();
                LogInfo("TestHost: Opened _channel.");

                allTestsPassed = runTests();
                LogInfo("TestHost: Finished running test cases.");
            }
            ccServer.CoverageData.Serialize(FilePath.NewFilePath(codeCoverageStore));
            return allTestsPassed;
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError("Exception thrown in InvokeEngine: {0}.", e.ExceptionObject);
        }

        private static bool RunTests(string testResultsStore, string discoveredUnitTestsStore)
        {
            Stopwatch stopWatch = new Stopwatch();

            LogInfo("TestHost executing tests...");
            stopWatch.Start();
            var testResults = new PerTestIdResults();
            var perAssemblyTestIds = PerAssemblyTestCases.Deserialize(FilePath.NewFilePath(discoveredUnitTestsStore));
            Parallel.ForEach(
                perAssemblyTestIds.Keys,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                asm =>
                {
                    if (perAssemblyTestIds[asm].Count == 0)
                    {
                        return;
                    }

                    LogInfo("Executing tests in {0}: Start.", asm);
                    var exec = new XUnitTestExecutor();
                    exec.TestExecuted.AddHandler(new FSharpHandler<TestResult>((o, ea) => NoteTestResults(testResults, ea)));
                    exec.ExecuteTests(perAssemblyTestIds[asm]);
                    LogInfo("Executing tests in {0}: Done.", asm);
                });

            if (!Debugger.IsAttached)
            {
                testResults.Serialize(FilePath.NewFilePath(testResultsStore));
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            LogInfo("Done TestHost executing tests! [" + elapsedTime + "]");
            LogInfo("");

            var rrs =
                from tr in testResults
                from rr in tr.Value
                where rr.result.Outcome == TestOutcome.Failed
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
            results.Add(new TestRunResult(ea));
        }
    }
}
