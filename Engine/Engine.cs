using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Engine.Diagnostics;
using R4nd0mApps.TddStud10.TestExecution.Adapters;
using R4nd0mApps.TddStud10.TestRuntime;

namespace R4nd0mApps.TddStud10.Engine
{
    public static class Engine
    {
        public static RunStep[] CreateRunSteps()
        {
            return new[] 
            {
                TddStud10Runner.CreateRunStep(RunStepKind.Build, "Creating Solution Snapshot".ToRSN(), TakeSolutionSnapshot)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, "Deleting Build Output".ToRSN(), DeleteBuildOutput)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, "Building Solution Snapshot".ToRSN(), BuildSolutionSnapshot)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, "Refresh Test Runtime".ToRSN(), RefreshTestRuntime)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, "Discover Unit Tests".ToRSN(), DiscoverUnitTests)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, "Instrument Binaries".ToRSN(), InstrumentBinaries)
                , TddStud10Runner.CreateRunStep(RunStepKind.Test, "Running Tests".ToRSN(), RunTests)
            };
        }

        public static void FindAndExecuteForEachAssembly(IRunExecutorHost host, string buildOutputRoot, DateTime timeFilter, Action<string> action, int? maxThreads = null)
        {
            int madDegreeOfParallelism = maxThreads.HasValue ? maxThreads.Value : Environment.ProcessorCount;
            Logger.I.LogInfo("FindAndExecuteForEachAssembly: Running with {0} threads.", madDegreeOfParallelism);
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".dll", ".exe" };
            Parallel.ForEach(
                Directory.EnumerateFiles(buildOutputRoot, "*").Where(s => extensions.Contains(Path.GetExtension(s))),
                new ParallelOptions { MaxDegreeOfParallelism = madDegreeOfParallelism },
                assemblyPath =>
                {
                    if (!File.Exists(Path.ChangeExtension(assemblyPath, ".pdb")))
                    {
                        return;
                    }

                    var lastWriteTime = File.GetLastWriteTimeUtc(assemblyPath);
                    if (lastWriteTime < timeFilter)
                    {
                        return;
                    }

                    Logger.I.LogInfo("FindAndExecuteForEachAssembly: Running for assembly {0}. LastWriteTime: {1}.", assemblyPath, lastWriteTime.ToLocalTime());
                    action(assemblyPath);
                });
        }

        private static RunStepResult RefreshTestRuntime(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            var output = TestRunTimeInstaller.Install(rd.solutionBuildRoot.Item);

            return rd.ToRSR(name, kind, RunStepStatus.Succeeded, string.Format("Copied Test Runtime: {0}", output));
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
            string testRunnerPath = Path.GetFullPath(typeof(R4nd0mApps.TddStud10.TestHost.Program).Assembly.Location);
            // NOTE: We dont have a better option. VSIX does support installing non-assembly dependencies.
            File.WriteAllText(testRunnerPath + ".config", Properties.Resources.TestHostAppConfig);
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
            PerTestIdResults testResults = PerTestIdResults.Deserialize(FilePath.NewFilePath(testResultsStore));

            PerAssemblySequencePointsCoverage coverageSession = PerAssemblySequencePointsCoverage.Deserialize(FilePath.NewFilePath(coverageSessionStore));

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

            // Remove this duplication
            Func<string, string> rebaseCodeFilePath = s => s.ToUpperInvariant().Replace(
                Path.GetDirectoryName(rd.solutionSnapshotPath.Item).ToUpperInvariant(), Path.GetDirectoryName(rd.solutionPath.Item).ToUpperInvariant());

            var testsPerAssembly = new PerAssemblyTestCases();
            Engine.FindAndExecuteForEachAssembly(
                host,
                buildOutputRoot,
                timeFilter,
                (string assemblyPath) =>
                {
                    var tests = testsPerAssembly.GetOrAdd(FilePath.NewFilePath(assemblyPath), _ => new ConcurrentBag<TestCase>());
                    var disc = new XUnitTestDiscoverer();
                    disc.TestDiscovered.AddHandler(new FSharpHandler<TestCase>((o, ea) => { ea.CodeFilePath = rebaseCodeFilePath(ea.CodeFilePath); tests.Add(ea); }));
                    disc.DiscoverTests(FilePath.NewFilePath(assemblyPath));
                });

            var discoveredUnitTestsStore = Path.Combine(rd.solutionBuildRoot.Item, "Z_discoveredUnitTests.xml");
            testsPerAssembly.Serialize(FilePath.NewFilePath(discoveredUnitTestsStore));

            Logger.I.LogInfo("Written discovered unit tests to {0}.", discoveredUnitTestsStore);

            return new RunData(
                rd.startTime,
                rd.solutionPath,
                rd.solutionSnapshotPath,
                rd.solutionBuildRoot,
                new FSharpOption<PerAssemblyTestCases>(testsPerAssembly),
                rd.sequencePoints,
                rd.codeCoverageResults,
                rd.executedTests).ToRSR(name, kind, RunStepStatus.Succeeded, "Unit Tests Discovered - which ones - TBD");
        }


        private static RunStepResult InstrumentBinaries(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            var sequencePointStore = Path.Combine(rd.solutionBuildRoot.Item, "Z_sequencePointStore.xml");
            var dict = Instrumentation.GenerateSequencePointInfo(host, rd.startTime, rd.solutionBuildRoot.Item);
            if (dict != null)
            {
                dict.Serialize(FilePath.NewFilePath(sequencePointStore));
            }

            if (!host.CanContinue())
            {
                throw new OperationCanceledException();
            }

            Instrumentation.Instrument(host, rd.startTime, Path.GetDirectoryName(rd.solutionSnapshotPath.Item), Path.GetDirectoryName(rd.solutionPath.Item), rd.solutionBuildRoot.Item, rd.testsPerAssembly.Value);

            var retRd = CreateRunDataForInstrumentationStep(rd, dict);

            return retRd.ToRSR(name, kind, RunStepStatus.Succeeded, "Binaries Instrumented - which ones - TBD");
        }

        private static RunData CreateRunDataForInstrumentationStep(RunData rd, PerDocumentSequencePoints sequencePoints)
        {
            return new RunData(
                rd.startTime,
                rd.solutionPath,
                rd.solutionSnapshotPath,
                rd.solutionBuildRoot,
                rd.testsPerAssembly,
                new FSharpOption<PerDocumentSequencePoints>(sequencePoints),
                rd.codeCoverageResults,
                rd.executedTests);
        }

        private static RunStepResult BuildSolutionSnapshot(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            var output = ExecuteProcess(
                @"c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe",
                string.Format(
                    @"/m /v:minimal /p:CreateVsixContainer=false /p:DeployExtension=false /p:CopyVsixExtensionFiles=false /p:VisualStudioVersion=12.0 /p:OutDir={0} {1}",
                    rd.solutionBuildRoot.Item,
                    rd.solutionSnapshotPath.Item)
            );

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
                    Logger.I.LogError(e.Data);
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
