using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
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
            Console.WriteLine(format, args);
        }

        private static void LogError(string format, params object[] args)
        {
            Logger.I.LogError(format, args);
            Console.WriteLine(format, args);
        }

        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static int Main(string[] args)
        {
            Logger.I.LogError("TestHost: Entering Main");
            bool runFailed = true;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);
            codeCoverageStore = args[2];
            testResultsStore = args[3];
            discoveredUnitTestsStore = args[4];
            var ccServer = new CoverageDataCollector();
            using (ServiceHost serviceHost = new ServiceHost(ccServer))
            {
                Logger.I.LogError("TestHost: Created Service Host.");
                string address = string.Format(
                    "net.pipe://localhost/gorillacoding/IPCTest/{0}",
                    Process.GetCurrentProcess().Id.ToString());
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                serviceHost.AddServiceEndpoint(typeof(ICoverageDataCollector), binding, address);
                serviceHost.Open();
                Logger.I.LogError("TestHost: Opened channel.");

                runFailed = RunTests();
                Logger.I.LogError("TestHost: Finished test cases.");
            }
            ccServer.SaveTestCases(codeCoverageStore);

            Logger.I.LogError("TestHost: Done. Exiting process.");
            return runFailed ? 1 : 0;
        }

        private static ICoverageDataCollector CreateChannel()
        {
            string address = string.Format(
                "net.pipe://localhost/gorillacoding/IPCTest/{0}",
                Process.GetCurrentProcess().Id.ToString());

            Logger.I.LogInfo("Marker: Initiating connection to {0} ...", address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(address);
            var ret = ChannelFactory<ICoverageDataCollector>.CreateChannel(binding, ep);
            ret.Ping();
            Logger.I.LogInfo("Marker: Connected to server.", address);

            return ret;
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
            foreach (var asm in utAssemblies.Keys)
            {
                LogInfo("Executing tests in {0}.", asm);

                var exec = new XUnitTestExecutor();
                exec.TestExecuted.AddHandler(new FSharpHandler<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult>((o, ea) => NoteTestResults(testResults, ea)));
                exec.ExecuteTests(asm);
            }

            testResults.Serialize(testResultsStore);

            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            LogInfo("Done TestHost executing tests! [" + elapsedTime + "]");
            LogInfo("");

            return testResults.Values.Contains(TestOutcome.Failed);
        }

        private static void NoteTestResults(PerTestIdResults testResults, Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult ea)
        {
            LogInfo("Test: {0} - {1}", ea.DisplayName, ea.Outcome);

            var testId = new TestId(
                FilePath.NewFilePath(ea.TestCase.Source), 
                FilePath.NewFilePath(ea.TestCase.CodeFilePath), 
                DocumentCoordinate.NewDocumentCoordinate(ea.TestCase.LineNumber));

            testResults.TryAdd(testId, ea.Outcome);
        }
    }
}
