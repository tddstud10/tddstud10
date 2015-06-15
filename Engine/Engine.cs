using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
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
                TddStud10Runner.CreateRunStep(RunStepKind.Build, RunStepSubKind.CreateSnapshot, "Creating Solution Snapshot".ToRSN(), TakeSolutionSnapshot)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, RunStepSubKind.DeleteBuildOutput, "Deleting Build Output".ToRSN(), DeleteBuildOutput)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, RunStepSubKind.BuildSnapshot, "Building Solution Snapshot".ToRSN(), BuildSolutionSnapshot)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, RunStepSubKind.RefreshTestRuntime, "Refresh Test Runtime".ToRSN(), RefreshTestRuntime)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, RunStepSubKind.DiscoverTests, "Discover Unit Tests".ToRSN(), DiscoverUnitTests)
                , TddStud10Runner.CreateRunStep(RunStepKind.Build, RunStepSubKind.InstrumentBinaries, "Instrument Binaries".ToRSN(), InstrumentBinaries)
                , TddStud10Runner.CreateRunStep(RunStepKind.Test, RunStepSubKind.RunTests, "Running Tests".ToRSN(), RunTests)
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

        private static RunStepResult RefreshTestRuntime(IRunExecutorHost host, RunStartParams rsp, RunStepName name, RunStepKind kind, RunStepSubKind subKind)
        {
            var output = TestRunTimeInstaller.Install(rsp.solutionBuildRoot.Item);

            return rsp.ToRSR(name, kind, subKind, RunStepStatus.Succeeded, RunData.NoData, string.Format("Copied Test Runtime: {0}", output));
        }

        private static RunStepName ToRSN(this string name)
        {
            return RunStepName.NewRunStepName(name);
        }

        private static RunStepResult ToRSR(this RunStartParams rsp, RunStepName name, RunStepKind kind, RunStepSubKind subKind, RunStepStatus runStepStatus, RunData rd, string addendum)
        {
            return new RunStepResult(
                rsp,
                name,
                kind,
                subKind,
                runStepStatus,
                rd,
                RunStepStatusAddendum.NewFreeFormatData(addendum));
        }

        private static RunStepResult RunTests(IRunExecutorHost host, RunStartParams rsp, RunStepName name, RunStepKind kind, RunStepSubKind subKind)
        {
            var coverageSessionStore = Path.Combine(rsp.solutionBuildRoot.Item, "Z_coverageresults.xml");
            var testResultsStore = Path.Combine(rsp.solutionBuildRoot.Item, "Z_testresults.xml");
            var discoveredUnitTestsStore = Path.Combine(rsp.solutionBuildRoot.Item, "Z_discoveredUnitTests.xml");
            string testRunnerPath = rsp.testHostPath.Item;
            // NOTE: We dont have a better option. VSIX does support installing non-assembly dependencies.
            File.WriteAllText(testRunnerPath + ".config", Properties.Resources.TestHostAppConfig);
            var output = ExecuteProcess(
                testRunnerPath,
                string.Format(
                    @"execute {0} {1} {2} {3}",
                    rsp.solutionBuildRoot.Item,
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

            var testResults = PerTestIdResults.Deserialize(FilePath.NewFilePath(testResultsStore));

            var coverageSession = PerAssemblySequencePointsCoverage.Deserialize(FilePath.NewFilePath(coverageSessionStore));

            return rsp.ToRSR(name, kind, subKind, rss, RunData.NewTestRunOutput(testResults, coverageSession), output.Item2);
        }

        private static RunStepResult DiscoverUnitTests(IRunExecutorHost host, RunStartParams rsp, RunStepName name, RunStepKind kind, RunStepSubKind subKind)
        {
            if (!host.CanContinue())
            {
                throw new OperationCanceledException();
            }

            var buildOutputRoot = rsp.solutionBuildRoot.Item;
            var timeFilter = rsp.startTime;

            Func<string, string> rebaseCodeFilePath =
                s => PathBuilder.rebaseCodeFilePath(rsp, FilePath.NewFilePath(s)).Item;

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

            var discoveredUnitTestsStore = Path.Combine(rsp.solutionBuildRoot.Item, "Z_discoveredUnitTests.xml");
            testsPerAssembly.Serialize(FilePath.NewFilePath(discoveredUnitTestsStore));

            Logger.I.LogInfo("Written discovered unit tests to {0}.", discoveredUnitTestsStore);

            return rsp.ToRSR(name, kind, subKind, RunStepStatus.Succeeded, RunData.NewTestCases(testsPerAssembly), "Unit Tests Discovered - which ones - TBD");
        }

        private static RunStepResult InstrumentBinaries(IRunExecutorHost host, RunStartParams rsp, RunStepName name, RunStepKind kind, RunStepSubKind subKind)
        {
            var sequencePointStore = Path.Combine(rsp.solutionBuildRoot.Item, "Z_sequencePointStore.xml");
            var dict = Instrumentation.GenerateSequencePointInfo(host, rsp);
            if (dict != null)
            {
                dict.Serialize(FilePath.NewFilePath(sequencePointStore));
            }

            if (!host.CanContinue())
            {
                throw new OperationCanceledException();
            }

            Instrumentation.Instrument(host, rsp, DataStore.Instance.FindTest);

            return rsp.ToRSR(name, kind, subKind, RunStepStatus.Succeeded, RunData.NewSequencePoints(dict), "Binaries Instrumented - which ones - TBD");
        }

        private static RunStepResult BuildSolutionSnapshot(IRunExecutorHost host, RunStartParams rsp, RunStepName name, RunStepKind kind, RunStepSubKind subKind)
        {
            var output = ExecuteProcess(
                @"c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe",
                string.Format(
                    @"/m /v:minimal /p:CreateVsixContainer=false /p:DeployExtension=false /p:CopyVsixExtensionFiles=false /p:VisualStudioVersion=12.0 /p:OutDir={0} {1}",
                    rsp.solutionBuildRoot.Item,
                    rsp.solutionSnapshotPath.Item)
            );

            RunStepStatus rss = RunStepStatus.Succeeded;
            if (output.Item1 != 0)
            {
                rss = RunStepStatus.Failed;
            }

            return rsp.ToRSR(name, kind, subKind, rss, RunData.NoData, output.Item2);
        }

        private static RunStepResult TakeSolutionSnapshot(IRunExecutorHost host, RunStartParams rsp, RunStepName name, RunStepKind kind, RunStepSubKind subKind)
        {
            var sln = new Solution(rsp.solutionPath.Item);
            var solutionGrandParentPath = Path.GetDirectoryName(Path.GetDirectoryName(rsp.solutionPath.Item));
            sln.Projects.ForEach(p =>
            {
                if (!host.CanContinue())
                {
                    throw new OperationCanceledException();
                }

                var projectFile = Path.Combine(Path.GetDirectoryName(rsp.solutionPath.Item), p.RelativePath);
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

            return rsp.ToRSR(name, kind, subKind, RunStepStatus.Succeeded, RunData.NoData, "What was done - TBD");
        }

        private static RunStepResult DeleteBuildOutput(IRunExecutorHost host, RunStartParams rsp, RunStepName name, RunStepKind kind, RunStepSubKind subKind)
        {
            if (Directory.Exists(rsp.solutionBuildRoot.Item))
            {
                foreach (var file in Directory.EnumerateFiles(rsp.solutionBuildRoot.Item, "*.pdb"))
                {
                    File.Delete(file);

                    var dll = Path.ChangeExtension(file, "dll");
                    if (File.Exists(dll))
                    {
                        File.Delete(dll);
                    }
                }
            }

            return rsp.ToRSR(name, kind, subKind, RunStepStatus.Succeeded, RunData.NoData, "What was done - TBD");
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
