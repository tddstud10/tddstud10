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
using R4nd0mApps.TddStud10.TestExecution.Adapters;
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using Server;

namespace R4nd0mApps.TddStud10.TestHost
{
    public class Program
    {
        private static XmlSerializer serializer = new XmlSerializer(typeof(List<string>));

        private static string solutionBuildRoot;
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

        public static int Main(string[] args)
        {
            bool runFailed = true;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);
            solutionBuildRoot = args[1];
            codeCoverageStore = args[2];
            testResultsStore = args[3];
            discoveredUnitTestsStore = args[4];
            var ccServer = new CodeCoverageServer();
            using (ServiceHost serviceHost = new ServiceHost(ccServer))
            {
                string address = string.Format(
                    "net.pipe://localhost/gorillacoding/IPCTest/{0}",
                    Process.GetCurrentProcess().Id.ToString());
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                serviceHost.AddServiceEndpoint(typeof(ICodeCoverageServer), binding, address);
                serviceHost.Open();

                runFailed = RunTests();
            }
            ccServer.SaveTestCases(codeCoverageStore);

            return runFailed ? 1 : 0;
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
            var testResults = new TestResults();
            var utAssemblies = LoadUnitTestAssemblies();
            foreach (var asm in utAssemblies)
            {
                LogInfo("Executing tests in {0}.", asm);

                var exec = new XUnitTestExecutor();
                exec.TestExecuted.AddHandler(new FSharpHandler<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult>((o, ea) => NoteTestResults(testResults, ea)));
                exec.ExecuteTests(asm);
            }

            SaveTestResults(testResults);

            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            LogInfo("Done TestHost executing tests! [" + elapsedTime + "]");
            LogInfo("");

            return testResults.Values.Contains(TestResult.Failed);
        }

        private static void NoteTestResults(TestResults testResults, Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult ea)
        {
            LogInfo("Test: {0} - {1}", ea.DisplayName, ea.Outcome);

            var hash = ea.TestCase.Source.ToUpperInvariant() + "|" + ea.TestCase.CodeFilePath.ToUpperInvariant() + "|" + ea.TestCase.LineNumber.ToString(CultureInfo.InvariantCulture);

            var testResult = TestResult.Failed;
            switch (ea.Outcome)
            {
                case TestOutcome.None:
                    testResult = TestResult.Failed;
                    break;
                case TestOutcome.NotFound:
                    testResult = TestResult.Failed;
                    break;
                case TestOutcome.Skipped:
                    testResult = TestResult.Skipped;
                    break;
                case TestOutcome.Failed:
                    testResult = TestResult.Failed;
                    break;
                case TestOutcome.Passed:
                    testResult = TestResult.Passed;
                    break;
            }

            testResults.TryAdd(hash.ToString(CultureInfo.InvariantCulture), testResult);
        }

        private static IEnumerable<string> LoadUnitTestAssemblies()
        {
            var testCases = File.ReadAllText(discoveredUnitTestsStore);
            StringReader reader = new StringReader(testCases);
            XmlTextReader xmlReader = new XmlTextReader(reader);
            try
            {
                var serializer = new XmlSerializer(typeof(List<string>));
                return serializer.Deserialize(xmlReader) as List<string>;
            }
            finally
            {
                xmlReader.Close();
                reader.Close();
            }
        }

        private static void SaveTestResults(TestResults testDetails)
        {
            StringWriter writer = new StringWriter();

            TestResults.Serializer.Serialize(writer, testDetails);
            File.WriteAllText(testResultsStore, writer.ToString());
        }
    }
}
