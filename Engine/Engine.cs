using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Engine.Diagnostics;
using R4nd0mApps.TddStud10.TestExecution.Adapters;
using R4nd0mApps.TddStud10.TestHost;

namespace R4nd0mApps.TddStud10.Engine
{
    // TODO
    // - make the stages assembly parallel 
    //   - Marker/code cooverage server/client ned to be multi threaded - lazy kvp with valuefactory - http://arbel.net/2013/02/03/best-practices-for-using-concurrentdictionary/
    // - Run pipeline assembly by assembly
    // - rewrite the DiscoverUnitTest in FSharp and house it in testhost.core
    // - Combine the instrument and seq pt passes
    // - make the assembly selection logic DRY
    // - can we borrow code from gallio to make codecoverage comm faster
    // - support nunit
    //   - remove xunit from vsix root
    //
    // - refactor out the domain model
    //   v Move to domain: case insensitive comparision for FilePath, hashcode, comparable, etc.
    //   v Rename to Session*
    //   v Change to MethodId
    //   v Change to MethodRid
    //   v Make strongly typed
    //     v all strings -> FilePath
    //   v remove referece to testhost from package
    //   v write discovered tests in z_disctests for diagnostic purposes
    //   v change Marker calls in generated il
    //   v dry violation - UnitTestStartLocation
    //   - Move session* to engine, POCOS to domain model
    //   - create seperate testruntime binary
    //     - Put logger calls into marker
    //     - dont copy test host into outdir
    //   - Classify the domain
    //   x move test host binaries into subfolder
    public static class Engine
    {
        public static RunStep[] CreateRunSteps()
        {
            return new[] 
            {
                TddStud10Runner.CreateRunStep(RunStepKind.Build, "Creating Solution Snapshot".ToRSN(), TakeSolutionSnapshot)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, "Deleting Build Output".ToRSN(), DeleteBuildOutput)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, "Building Solution Snapshot".ToRSN(), BuildSolutionSnapshot)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, "Discover Unit Tests".ToRSN(), DiscoverUnitTests)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, "Instrument Binaries".ToRSN(), InstrumentBinaries)
                , TddStud10Runner.CreateRunStep(RunStepKind.Test, "Running Tests".ToRSN(), RunTests)
            };
        }

        private static RunStepName ToRSN(this string name)
        {
            return RunStepName.NewRunStepName(name);
        }

        private static RunStepResult ToRSR(this RunData rd, RunStepName name, RunStepKind kind, RunStepStatus runStepStatus, string addendum)
        {
            return new RunStepResult(
                name,
                kind,
                runStepStatus,
                RunStepStatusAddendum.NewFreeFormatData(addendum),
                rd);
        }

        private static RunStepResult RunTests(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            var coverageSessionStore = Path.Combine(rd.solutionBuildRoot.Item, "Z_coverageresults.xml");
            var testResultsStore = Path.Combine(rd.solutionBuildRoot.Item, "Z_testresults.xml");
            var discoveredUnitTestsStore = Path.Combine(rd.solutionBuildRoot.Item, "Z_discoveredUnitTests.xml");
            string testRunnerPath = Path.GetFullPath(typeof(R4nd0mApps.TddStud10.TestHost.Marker).Assembly.Location);
            var output = ExecuteProcess(
                testRunnerPath,
                string.Format(
                    @"execute {0} {1} {2} {3}",
                    rd.solutionBuildRoot.Item,
                    coverageSessionStore,
                    testResultsStore,
                    discoveredUnitTestsStore
                )
            );

            RunStepStatus rss = RunStepStatus.Succeeded;
            if (output.Item1 != 0)
            {
                rss = RunStepStatus.Failed;
            }

            var retRd = CreateRunDataForRunTest(rd, coverageSessionStore, testResultsStore, discoveredUnitTestsStore);
            return retRd.ToRSR(name, kind, rss, output.Item2);
        }

        private static RunData CreateRunDataForRunTest(RunData rd, string coverageSessionStore, string testResultsStore, string discoveredUnitTestsStore)
        {
            PerTestIdResults testResults = null;
            var res = File.ReadAllText(testResultsStore);
            var reader = new StringReader(res);
            var xmlReader = new XmlTextReader(reader);
            try
            {
                testResults = PerTestIdResults.Serializer.Deserialize(xmlReader) as PerTestIdResults;
            }
            finally
            {
                xmlReader.Close();
                reader.Close();
            }

            PerAssemblySequencePointsCoverage coverageSession = null;
            res = File.ReadAllText(coverageSessionStore);
            reader = new StringReader(res);
            xmlReader = new XmlTextReader(reader);
            try
            {
                coverageSession = PerAssemblySequencePointsCoverage.Serializer.Deserialize(xmlReader) as PerAssemblySequencePointsCoverage;
            }
            finally
            {
                xmlReader.Close();
                reader.Close();
            }

            return new RunData(
                rd.startTime,
                rd.solutionPath,
                rd.solutionSnapshotPath,
                rd.solutionBuildRoot,
                rd.testsPerAssembly,
                rd.sequencePoints,
                new FSharpOption<PerAssemblySequencePointsCoverage>(coverageSession),
                new FSharpOption<PerTestIdResults>(testResults));
        }

        private static RunStepResult DiscoverUnitTests(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            if (!host.CanContinue())
            {
                throw new OperationCanceledException();
            }

            var buildOutputRoot = rd.solutionBuildRoot.Item;
            var timeFilter = rd.startTime;
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".dll", ".exe" };

            var testsPerAssembly = new ConcurrentDictionary<FilePath, IEnumerable<TestCase>>();
            foreach (var assemblyPath in Directory.EnumerateFiles(buildOutputRoot, "*").Where(s => extensions.Contains(Path.GetExtension(s))))
            {
                if (!File.Exists(Path.ChangeExtension(assemblyPath, ".pdb")))
                {
                    continue;
                }

                var lastWriteTime = File.GetLastWriteTimeUtc(assemblyPath);
                if (lastWriteTime < timeFilter)
                {
                    continue;
                }

                Logger.I.LogInfo("Discovering unit tests for {0}. Last write time: {1}.", assemblyPath, lastWriteTime.ToLocalTime());

                var tests = new ConcurrentBag<TestCase>();
                var disc = new XUnitTestDiscoverer();
                disc.TestDiscovered.AddHandler(new FSharpHandler<TestCase>((o, ea) => tests.Add(ea)));
                disc.DiscoverTests(FilePath.NewFilePath(assemblyPath));
                if (tests.Count > 0)
                {
                    testsPerAssembly.TryAdd(FilePath.NewFilePath(assemblyPath), tests);
                }
            }

            return new RunData(
                rd.startTime,
                rd.solutionPath,
                rd.solutionSnapshotPath,
                rd.solutionBuildRoot,
                new ReadOnlyDictionary<FilePath, IEnumerable<TestCase>>(testsPerAssembly),
                rd.sequencePoints,
                rd.codeCoverageResults,
                rd.executedTests).ToRSR(name, kind, RunStepStatus.Succeeded, "Unit Tests Discovered - which ones - TBD");

        }

        private static RunStepResult InstrumentBinaries(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            var sequencePointStore = Path.Combine(rd.solutionBuildRoot.Item, "Z_sequencePointStore.xml");
            var dict = Instrumentation.GenerateSequencePointInfo(rd.startTime, rd.solutionBuildRoot.Item);
            if (dict != null)
            {
                using (StringWriter writer = new StringWriter())
                {
                    PerAssemblySequencePoints.Serializer.Serialize(writer, dict);
                    File.WriteAllText(sequencePointStore, writer.ToString());
                    Logger.I.LogInfo("Written sequence points to {0}.", sequencePointStore);
                }
            }

            if (!host.CanContinue())
            {
                throw new OperationCanceledException();
            }

            Instrumentation.Instrument(rd.startTime, Path.GetDirectoryName(rd.solutionPath.Item), rd.solutionBuildRoot.Item, rd.testsPerAssembly);

            using (StringWriter writer = new StringWriter())
            {
                var unitTestAssemblies = new PerAssemblyTestIds(
                    rd.testsPerAssembly.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Select(
                            tc => new TestId(
                                FilePath.NewFilePath(tc.Source),
                                FilePath.NewFilePath(tc.CodeFilePath),
                                DocumentCoordinate.NewDocumentCoordinate(tc.LineNumber))).ToList()));

                PerAssemblyTestIds.Serializer.Serialize(writer, unitTestAssemblies);
                var discoveredUnitTestsStore = Path.Combine(rd.solutionBuildRoot.Item, "Z_discoveredUnitTests.xml");
                File.WriteAllText(discoveredUnitTestsStore, writer.ToString());
                Logger.I.LogInfo("Written discovered unit tests to {0}.", discoveredUnitTestsStore);
            }

            var retRd = CreateRunDataForInstrumentationStep(rd, dict);

            return retRd.ToRSR(name, kind, RunStepStatus.Succeeded, "Binaries Instrumented - which ones - TBD");
        }

        private static RunData CreateRunDataForInstrumentationStep(RunData rd, PerAssemblySequencePoints sequencePoints)
        {
            return new RunData(
                rd.startTime,
                rd.solutionPath,
                rd.solutionSnapshotPath,
                rd.solutionBuildRoot,
                rd.testsPerAssembly,
                new FSharpOption<PerAssemblySequencePoints>(sequencePoints),
                rd.codeCoverageResults,
                rd.executedTests);
        }

        private static RunStepResult BuildSolutionSnapshot(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            string testRunnerPath = Path.GetFullPath(typeof(R4nd0mApps.TddStud10.TestHost.Marker).Assembly.Location);
            var output = ExecuteProcess(
                @"c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe",
                string.Format(
                    @"/m /v:minimal /p:CreateVsixContainer=false /p:DeployExtension=false /p:CopyVsixExtensionFiles=false /p:VisualStudioVersion=12.0 /p:OutDir={0} {1}",
                    rd.solutionBuildRoot.Item,
                    rd.solutionSnapshotPath.Item)
            );

            if (File.Exists(Path.Combine(rd.solutionBuildRoot.Item, Path.GetFileName(testRunnerPath))))
            {
                File.Delete(Path.Combine(rd.solutionBuildRoot.Item, Path.GetFileName(testRunnerPath)));
            }
            File.Copy(testRunnerPath, Path.Combine(rd.solutionBuildRoot.Item, Path.GetFileName(testRunnerPath)));

            RunStepStatus rss = RunStepStatus.Succeeded;
            if (output.Item1 != 0)
            {
                rss = RunStepStatus.Failed;
            }

            var rdRet = CreateRunDataForBuildSolution(rd);

            return rdRet.ToRSR(name, kind, rss, output.Item2);
        }

        private static RunData CreateRunDataForBuildSolution(RunData rd)
        {
            return new RunData(
                rd.startTime,
                rd.solutionPath,
                rd.solutionSnapshotPath,
                rd.solutionBuildRoot,
                rd.testsPerAssembly,
                rd.sequencePoints,
                rd.codeCoverageResults,
                rd.executedTests);
        }

        private static RunStepResult TakeSolutionSnapshot(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            var sln = new Solution(rd.solutionPath.Item);
            var solutionGrandParentPath = Path.GetDirectoryName(Path.GetDirectoryName(rd.solutionPath.Item));
            sln.Projects.ForEach(p =>
            {
                if (!host.CanContinue())
                {
                    throw new OperationCanceledException();
                }

                var projectFile = Path.Combine(Path.GetDirectoryName(rd.solutionPath.Item), p.RelativePath);
                var folder = Path.GetDirectoryName(projectFile);
                foreach (var src in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                {
                    var dst = src.ToUpperInvariant().Replace(solutionGrandParentPath.ToUpperInvariant(), PathBuilder.snapShotRoot);
                    var srcInfo = new FileInfo(src);
                    var dstInfo = new FileInfo(dst);

                    if (srcInfo.LastWriteTimeUtc > dstInfo.LastWriteTimeUtc)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(dst));
                        Logger.I.LogInfo("Copying: {0} - {1}.", src, dst);
                        File.Copy(src, dst, true);
                    }
                }
            });

            return rd.ToRSR(name, kind, RunStepStatus.Succeeded, "What was done - TBD");
        }

        private static RunStepResult DeleteBuildOutput(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            if (Directory.Exists(rd.solutionBuildRoot.Item))
            {
                foreach (var file in Directory.EnumerateFiles(rd.solutionBuildRoot.Item, "*.pdb"))
                {
                    File.Delete(file);

                    var dll = Path.ChangeExtension(file, "dll");
                    if (File.Exists(dll))
                    {
                        File.Delete(dll);
                    }
                }
            }

            return rd.ToRSR(name, kind, RunStepStatus.Succeeded, "What was done - TBD");
        }

        private static Tuple<int, string> ExecuteProcess(string fileName, string arguments)
        {
            ConcurrentQueue<string> consoleOutput = new ConcurrentQueue<string>();
            var commandLine = string.Format("Executing: '{0}' '{1}'", fileName, arguments);
            consoleOutput.Enqueue(commandLine);
            Logger.I.LogInfo(commandLine);

            ProcessStartInfo processStartInfo;
            Process process;

            processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = arguments;
            processStartInfo.FileName = fileName;

            process = new Process();
            process.StartInfo = processStartInfo;
            // enable raising events because Process does not raise events by default
            process.EnableRaisingEvents = true;
            // attach the event handler for OutputDataReceived before runStarting the process
            process.OutputDataReceived += new DataReceivedEventHandler
            (
                delegate(object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    consoleOutput.Enqueue(e.Data);
                    Logger.I.LogInfo(e.Data);
                }
            );
            process.ErrorDataReceived += new DataReceivedEventHandler
            (
                delegate(object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    consoleOutput.Enqueue(e.Data);
                    Logger.I.LogInfo(e.Data);
                }
            );
            // start the process
            // then begin asynchronously reading the output
            // then wait for the process to exit
            // then cancel asynchronously reading the output
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.CancelOutputRead();

            var sb = new StringBuilder();
            Array.ForEach(consoleOutput.ToArray(), s => sb.AppendLine(s));

            return new Tuple<int, string>(process.ExitCode, sb.ToString());
        }
    }
}
