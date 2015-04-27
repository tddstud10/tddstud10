using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using R4nd0mApps.TddStud10.Engine;
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using Server;
using Xunit;
using Xunit.Abstractions;

namespace R4nd0mApps.TddStud10.TestHost
{
    // TODO: Telemetry
    // TODO: Add todo list to tddstudio
    // TODO: Change target version of all the dlls to 3.5
    class Program
    {
        private static XmlSerializer serializer = new XmlSerializer(typeof(List<string>));

        private static string solutionBuildRoot;
        private static string codeCoverageStore;
        private static string testResultsStore;
        private static string discoveredUnitTestsStore;

        static void Main(string[] args)
        {
            solutionBuildRoot = args[1];
            codeCoverageStore = args[2];
            testResultsStore = args[3];
            discoveredUnitTestsStore = args[4];
            var ccServer = new CodeCoverageServer();
            using (ServiceHost serviceHost = new ServiceHost(ccServer))
            {
                string address = "net.pipe://localhost/gorillacoding/IPCTest";
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                serviceHost.AddServiceEndpoint(typeof(ICodeCoverageServer), binding, address);
                serviceHost.Open();

                RunTests();
            }
            ccServer.SaveTestCases(codeCoverageStore);
        }

        private static void RunTests()
        {
            Stopwatch stopWatch = new Stopwatch();
            TimeSpan ts;
            string elapsedTime;

            Logger.I.Log("TestHost executing tests...");
            stopWatch.Start();
            // TODO: Multi-threaded test discovery/execution
            // TODO: Can also get base test execution metrics - timing etc
            var testResults = new TestDetails();
            var unitTests = LoadUnitTestCases();
            foreach (var asm in unitTests.Keys)
            {
                Logger.I.Log("Executing tests in {0}.", asm);

                using (var controller = new XunitFrontController(asm))
                using (var resultsVisitor = new StandardOutputVisitor(new object(), false, solutionBuildRoot, () => false, Logger.I.Log, testResults.Dictionary))
                {
                    controller.RunAll(resultsVisitor, TestFrameworkOptions.ForDiscovery(), TestFrameworkOptions.ForExecution());
                    resultsVisitor.Finished.WaitOne();
                }
            }

            SaveTestResults(testResults);

            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            Logger.I.Log("Done TestHost executing tests! [" + elapsedTime + "]");
            Logger.I.Log("");
        }

        // TODO: This pattern is repeated everywhere. Unify it.
        private static DiscoveredUnitTests LoadUnitTestCases()
        {
            var testCases = File.ReadAllText(discoveredUnitTestsStore);
            StringReader reader = new StringReader(testCases);
            XmlTextReader xmlReader = new XmlTextReader(reader);
            try
            {
                return DiscoveredUnitTests.Serializer.Deserialize(xmlReader) as DiscoveredUnitTests;
            }
            finally
            {
                xmlReader.Close();
                reader.Close();
            }
        }

        private static void SaveTestResults(TestDetails testDetails)
        {
            StringWriter writer = new StringWriter();

            TestDetails.Serializer.Serialize(writer, testDetails);
            File.WriteAllText(testResultsStore, writer.ToString());
        }
    }

    // TODO: Cleanup - remove unnecesary classes
    internal class TestDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        private Predicate<ITestCase> _filter;

        public TestDiscoveryVisitor(Predicate<ITestCase> filter)
        {
            TestCases = new List<ITestCase>();
            _filter = filter;
        }

        public List<ITestCase> TestCases { get; private set; }

        protected override bool Visit(ITestCaseDiscoveryMessage discovery)
        {
            if (_filter(discovery.TestCase))
            {
                TestCases.Add(discovery.TestCase);
            }

            return base.Visit(discovery);
        }
    }

    public class StandardOutputVisitor : XmlTestExecutionVisitor
    {
        readonly object consoleLock;
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages;
        readonly string defaultDirectory;
        readonly bool quiet;
        readonly Action<string> consoleWriter;

        public ConcurrentDictionary<string, TestResult> testResults;

        public StandardOutputVisitor(object consoleLock,
                                     bool quiet,
                                     string defaultDirectory,
                                     Func<bool> cancelThunk,
                                     Action<string> consoleWriter,
                                     ConcurrentDictionary<string, TestResult> testResults,
                                     ConcurrentDictionary<string, ExecutionSummary> completionMessages = null)
            : base(new XElement("Assembly"), cancelThunk)
        {
            this.consoleLock = consoleLock;
            this.quiet = quiet;
            this.defaultDirectory = defaultDirectory;
            this.testResults = testResults;
            this.completionMessages = completionMessages;
            this.consoleWriter = consoleWriter;
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            if (!quiet)
                lock (consoleLock)
                    consoleWriter(string.Format("Starting:    {0}", Path.GetFileNameWithoutExtension(assemblyStarting.TestAssembly.Assembly.AssemblyPath)));

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            // Base class does computation of results, so call it first.
            var result = base.Visit(assemblyFinished);
            var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyFinished.TestAssembly.Assembly.AssemblyPath);

            if (!quiet)
                lock (consoleLock)
                    consoleWriter(string.Format("Finished:    {0}", assemblyDisplayName));

            if (completionMessages != null)
                completionMessages.TryAdd(assemblyDisplayName, new ExecutionSummary
                {
                    Total = assemblyFinished.TestsRun,
                    Failed = assemblyFinished.TestsFailed,
                    Skipped = assemblyFinished.TestsSkipped,
                    Time = assemblyFinished.ExecutionTime,
                    //Errors = Errors
                });

            return result;
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            testResults[CreateTestDetail(testFailed).Name] = TestResult.Failed;

            lock (consoleLock)
            {
                // TODO: Thread-safe way to figure out the default foreground color
                Console.ForegroundColor = ConsoleColor.Red;
                consoleWriter(string.Format("   {0} [FAIL]", Escape(testFailed.Test.DisplayName)));
                Console.ForegroundColor = ConsoleColor.Gray;
                consoleWriter(string.Format("      {0}", ExceptionUtility.CombineMessages(testFailed).Replace(Environment.NewLine, Environment.NewLine + "      ")));

                WriteStackTrace(ExceptionUtility.CombineStackTraces(testFailed));
            }

            return base.Visit(testFailed);
        }

        private static TestDetail CreateTestDetail(ITestMessage testMessage)
        {
            var td = new TestDetail
            {
                Assembly = testMessage.Test.TestCase.TestMethod.TestClass.Class.Assembly.Name,
                Class = testMessage.Test.TestCase.TestMethod.TestClass.Class.Name,
                ReturnType = testMessage.Test.TestCase.TestMethod.Method.ReturnType.Name,
                Method = testMessage.Test.TestCase.TestMethod.Method.Name,
            };

            td.Name = string.Format("{0} {1}::{2}", td.ReturnType, td.Class, td.Method);

            return td;
        }

        // TODO: Dont even build the vsix everytime

        protected override bool Visit(ITestPassed testPassed)
        {
            testResults[CreateTestDetail(testPassed).Name] = TestResult.Passed;

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            testResults[CreateTestDetail(testSkipped).Name] = TestResult.Skipped;

            lock (consoleLock)
            {
                // TODO: Thread-safe way to figure out the default foreground color
                Console.ForegroundColor = ConsoleColor.Yellow;
                consoleWriter(string.Format("   {0} [SKIP]", Escape(testSkipped.Test.DisplayName)));
                Console.ForegroundColor = ConsoleColor.Gray;
                consoleWriter(string.Format("      {0}", Escape(testSkipped.Reason)));
            }

            return base.Visit(testSkipped);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            consoleWriter(string.Format("   Running {0}...", Escape(testStarting.Test.DisplayName)));
            return base.Visit(testStarting);
        }

        protected override bool Visit(IErrorMessage error)
        {
            WriteError("FATAL", error);

            return base.Visit(error);
        }

        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Assembly Cleanup Failure ({0})", cleanupFailure.TestAssembly.Assembly.AssemblyPath), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Case Cleanup Failure ({0})", cleanupFailure.TestCase.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Class Cleanup Failure ({0})", cleanupFailure.TestClass.Class.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Collection Cleanup Failure ({0})", cleanupFailure.TestCollection.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Cleanup Failure ({0})", cleanupFailure.Test.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            WriteError(String.Format("Test Method Cleanup Failure ({0})", cleanupFailure.TestMethod.Method.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected void WriteError(string failureName, IFailureInformation failureInfo)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                consoleWriter(string.Format("   [{0}] {1}", failureName, Escape(failureInfo.ExceptionTypes[0])));
                Console.ForegroundColor = ConsoleColor.Gray;
                consoleWriter(string.Format("      {0}", Escape(ExceptionUtility.CombineMessages(failureInfo))));

                WriteStackTrace(ExceptionUtility.CombineStackTraces(failureInfo));
            }
        }

        void WriteStackTrace(string stackTrace)
        {
            if (String.IsNullOrWhiteSpace(stackTrace))
                return;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            consoleWriter(string.Format("      Stack Trace:"));

            Console.ForegroundColor = ConsoleColor.Gray;
            Array.ForEach(stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                          stackFrame => Console.Error.WriteLine("         {0}", TransformFrame(stackFrame, defaultDirectory)));
        }

        private string TransformFrame(string stackFrame, string defaultDirectory)
        {
            return stackFrame + defaultDirectory;
        }
    }

    public class XmlTestExecutionVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        readonly XElement assemblyElement;
        readonly XElement errorsElement;
        readonly ConcurrentDictionary<ITestCollection, XElement> testCollectionElements = new ConcurrentDictionary<ITestCollection, XElement>();

        public XmlTestExecutionVisitor(XElement assemblyElement, Func<bool> cancelThunk)
        {
            CancelThunk = cancelThunk ?? (() => false);

            this.assemblyElement = assemblyElement;

            if (this.assemblyElement != null)
            {
                errorsElement = new XElement("errors");
                this.assemblyElement.Add(errorsElement);
            }
        }

        public readonly Func<bool> CancelThunk;
        public int Errors;
        public int Failed;
        public int Skipped;
        public decimal Time;
        public int Total;

        XElement CreateTestResultElement(ITestResultMessage testResult, string resultText)
        {
            var collectionElement = GetTestCollectionElement(testResult.TestCase.TestMethod.TestClass.TestCollection);
            var testResultElement =
                new XElement("test",
                    new XAttribute("name", XmlEscape(testResult.Test.DisplayName)),
                    new XAttribute("type", testResult.TestCase.TestMethod.TestClass.Class.Name),
                    new XAttribute("method", testResult.TestCase.TestMethod.Method.Name),
                    new XAttribute("time", testResult.ExecutionTime.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("result", resultText)
                );

            if (!string.IsNullOrWhiteSpace(testResult.Output))
            {
                testResultElement.Add(new XElement("output", new XCData(testResult.Output)));
            }

            if (testResult.TestCase.SourceInformation != null)
            {
                if (testResult.TestCase.SourceInformation.FileName != null)
                    testResultElement.Add(new XAttribute("source-file", testResult.TestCase.SourceInformation.FileName));
                if (testResult.TestCase.SourceInformation.LineNumber != null)
                    testResultElement.Add(new XAttribute("source-line", testResult.TestCase.SourceInformation.LineNumber.GetValueOrDefault()));
            }

            if (testResult.TestCase.Traits != null && testResult.TestCase.Traits.Count > 0)
            {
                var traitsElement = new XElement("traits");

                foreach (var key in testResult.TestCase.Traits.Keys)
                    foreach (var value in testResult.TestCase.Traits[key])
                        traitsElement.Add(
                            new XElement("trait",
                                new XAttribute("name", XmlEscape(key)),
                                new XAttribute("value", XmlEscape(value))
                            )
                        );

                testResultElement.Add(traitsElement);
            }

            collectionElement.Add(testResultElement);

            return testResultElement;
        }

        XElement GetTestCollectionElement(ITestCollection testCollection)
        {
            return testCollectionElements.GetOrAdd(testCollection, tc => new XElement("collection"));
        }

        public override bool OnMessage(IMessageSinkMessage message)
        {
            var result = base.OnMessage(message);
            if (result)
                result = !CancelThunk();

            return result;
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            Total += assemblyFinished.TestsRun;
            Failed += assemblyFinished.TestsFailed;
            Skipped += assemblyFinished.TestsSkipped;
            Time += assemblyFinished.ExecutionTime;

            if (assemblyElement != null)
            {
                assemblyElement.Add(
                    new XAttribute("total", Total),
                    new XAttribute("passed", Total - Failed - Skipped),
                    new XAttribute("failed", Failed),
                    new XAttribute("skipped", Skipped),
                    new XAttribute("time", Time.ToString("0.000", CultureInfo.InvariantCulture)),
                    new XAttribute("errors", Errors)
                );

                foreach (var element in testCollectionElements.Values)
                    assemblyElement.Add(element);
            }

            return base.Visit(assemblyFinished);
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            if (assemblyElement != null)
            {
                assemblyElement.Add(
                    new XAttribute("name", assemblyStarting.TestAssembly.Assembly.AssemblyPath),
                    new XAttribute("environment", assemblyStarting.TestEnvironment),
                    new XAttribute("test-framework", assemblyStarting.TestFrameworkDisplayName),
                    new XAttribute("run-date", assemblyStarting.StartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    new XAttribute("run-time", assemblyStarting.StartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
                );

                if (assemblyStarting.TestAssembly.ConfigFileName != null)
                    assemblyElement.Add(new XAttribute("config-file", assemblyStarting.TestAssembly.ConfigFileName));
            }

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestCollectionFinished testCollectionFinished)
        {
            if (assemblyElement != null)
            {
                var collectionElement = GetTestCollectionElement(testCollectionFinished.TestCollection);
                collectionElement.Add(
                    new XAttribute("total", testCollectionFinished.TestsRun),
                    new XAttribute("passed", testCollectionFinished.TestsRun - testCollectionFinished.TestsFailed - testCollectionFinished.TestsSkipped),
                    new XAttribute("failed", testCollectionFinished.TestsFailed),
                    new XAttribute("skipped", testCollectionFinished.TestsSkipped),
                    new XAttribute("name", XmlEscape(testCollectionFinished.TestCollection.DisplayName)),
                    new XAttribute("time", testCollectionFinished.ExecutionTime.ToString("0.000", CultureInfo.InvariantCulture))
                );
            }

            return base.Visit(testCollectionFinished);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            if (assemblyElement != null)
            {
                var testElement = CreateTestResultElement(testFailed, "Fail");
                testElement.Add(CreateFailureElement(testFailed));
            }

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            if (assemblyElement != null)
                CreateTestResultElement(testPassed, "Pass");

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            if (assemblyElement != null)
            {
                var testElement = CreateTestResultElement(testSkipped, "Skip");
                testElement.Add(new XElement("reason", new XCData(XmlEscape(testSkipped.Reason))));
            }

            return base.Visit(testSkipped);
        }

        protected override bool Visit(IErrorMessage error)
        {
            AddError("fatal", null, error);

            return base.Visit(error);
        }

        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            AddError("assembly-cleanup", cleanupFailure.TestAssembly.Assembly.AssemblyPath, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            AddError("test-case-cleanup", cleanupFailure.TestCase.DisplayName, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            AddError("test-class-cleanup", cleanupFailure.TestClass.Class.Name, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            AddError("test-collection-cleanup", cleanupFailure.TestCollection.DisplayName, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            AddError("test-cleanup", cleanupFailure.Test.DisplayName, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            AddError("test-method-cleanup", cleanupFailure.TestMethod.Method.Name, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        void AddError(string type, string name, IFailureInformation failureInfo)
        {
            Errors++;

            if (errorsElement == null)
                return;

            var errorElement = new XElement("error", new XAttribute("type", type), CreateFailureElement(failureInfo));
            if (name != null)
                errorElement.Add(new XAttribute("name", name));

            errorsElement.Add(errorElement);
        }

        static XElement CreateFailureElement(IFailureInformation failureInfo)
        {
            return new XElement("failure",
                new XAttribute("exception-type", failureInfo.ExceptionTypes[0]),
                new XElement("message", new XCData(XmlEscape(ExceptionUtility.CombineMessages(failureInfo)))),
                new XElement("stack-trace", new XCData(ExceptionUtility.CombineStackTraces(failureInfo) ?? String.Empty))
            );
        }

        protected static string Escape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\0", "\\0");
        }

        protected static string XmlEscape(string value)
        {
            if (value == null)
                return String.Empty;

            value = Escape(value);
            var escapedValue = new StringBuilder(value.Length);
            for (var idx = 0; idx < value.Length; ++idx)
                if (value[idx] < 32)
                    escapedValue.AppendFormat("\\x{0}", ((byte)value[idx]).ToString("x2"));
                else
                    escapedValue.Append(value[idx]);

            return escapedValue.ToString();
        }
    }
}

/*
 TODO: Cleanup: Remove these
 c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /m /v:minimal /p:VisualStudioVersion=12.0 /p:OutDir=d:\tddstud10\fizzbuzz.out d:\tddstud10\fizzbuzz\FizzBuzz.sln
 D:\src\r4nd0mapps\WpfApplication2\WpfApplication2\bin\Debug\TestRunner.exe discover d:\tddstud10\fizzbuzz.out d:\tddstud10\fizzbuzz.out\testcases.txt
 D:\src\r4nd0mapps\WpfApplication2\packages\opencover.4.5.3522\OpenCover.Console.exe -mergebyhash "-output:d:\tddstud10\fizzbuzz.out\results.xml" -register:user -returntargetcode:10000 "-target:D:\src\r4nd0mapps\WpfApplication2\WpfApplication2\bin\Debug\TestRunner.exe" "-targetargs:execute d:\tddstud10\fizzbuzz.out d:\tddstud10\fizzbuzz.out\testcases.txt" "-targetdir:d:\tddstud10\fizzbuzz.out"
 D:\src\r4nd0mapps\WpfApplication2\packages\ReportGenerator.2.1.4.0\ReportGenerator.exe -reports:D:\src\r4nd0mkatas\fizzbuzz\results.0.xml -targetdir:D:\src\r4nd0mkatas\fizzbuzz\rep
 */
