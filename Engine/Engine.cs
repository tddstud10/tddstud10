using R4nd0mApps.TddStud10.Common.Domain;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Engine.Diagnostics;
using R4nd0mApps.TddStud10.TestRuntime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R4nd0mApps.TddStud10.Engine
{
    public static class Engine
    {
        public static RunStep[] CreateRunSteps()
        {
            return new[]
            {
                TddStud10Runner.CreateRunStep(new RunStepInfo("Creating Solution Snapshot".ToRSN(), RunStepKind.Build, RunStepSubKind.CreateSnapshot), TakeSolutionSnapshot)
                , TddStud10Runner.CreateRunStep(new RunStepInfo("Deleting Build Output".ToRSN(), RunStepKind.Build, RunStepSubKind.DeleteBuildOutput), DeleteBuildOutput)
                , TddStud10Runner.CreateRunStep(new RunStepInfo("Building Solution Snapshot".ToRSN(), RunStepKind.Build, RunStepSubKind.BuildSnapshot), BuildSolutionSnapshot)
                , TddStud10Runner.CreateRunStep(new RunStepInfo("Refresh Test Runtime".ToRSN(), RunStepKind.Build, RunStepSubKind.RefreshTestRuntime), RefreshTestRuntime)
                , TddStud10Runner.CreateRunStep(new RunStepInfo("Discover Sequence Points".ToRSN(), RunStepKind.Build, RunStepSubKind.DiscoverSequencePoints), DiscoverSequencePoints)
                , TddStud10Runner.CreateRunStep(new RunStepInfo("Discover Unit Tests".ToRSN(), RunStepKind.Build, RunStepSubKind.DiscoverTests), DiscoverUnitTests)
                , TddStud10Runner.CreateRunStep(new RunStepInfo("Instrument Binaries".ToRSN(), RunStepKind.Build, RunStepSubKind.InstrumentBinaries), InstrumentBinaries)
                , TddStud10Runner.CreateRunStep(new RunStepInfo("Running Tests".ToRSN(), RunStepKind.Test, RunStepSubKind.RunTests), RunTests)
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

        private static RunStepName ToRSN(this string name)
        {
            return RunStepName.NewRunStepName(name);
        }

        private static RunStepResult ToRSR(this RunStepStatus runStepStatus, RunData rd, string addendum)
        {
            return new RunStepResult(
                runStepStatus,
                rd,
                RunStepStatusAddendum.NewFreeFormatData(addendum));
        }

        private static RunStepResult RefreshTestRuntime(IRunExecutorHost host, RunStartParams rsp, RunStepInfo rsi)
        {
            var output = TestRunTimeInstaller.Install(rsp.Solution.BuildRoot.Item);

            return RunStepStatus.Succeeded.ToRSR(RunData.NoData, string.Format("Copied Test Runtime: {0}", output));
        }

        private static RunStepResult RunTests(IRunExecutorHost host, RunStartParams rsp, RunStepInfo rsi)
        {
            if (!host.CanContinue())
            {
                throw new OperationCanceledException();
            }

            var output = RunTestHost("execute", rsp);

            RunStepStatus rss = RunStepStatus.Succeeded;
            if (output.Item1 != 0)
            {
                rss = RunStepStatus.Failed;
            }

            var testResults = PerTestIdDResults.Deserialize(FilePath.NewFilePath(rsp.DataFiles.TestResultsStore.Item));
            var coverageSession = PerSequencePointIdTestRunId.Deserialize(FilePath.NewFilePath(rsp.DataFiles.CoverageSessionStore.Item));
            var testFailureInfo = PerDocumentLocationTestFailureInfo.Deserialize(FilePath.NewFilePath(rsp.DataFiles.TestFailureInfoStore.Item));

            return rss.ToRSR(RunData.NewTestRunOutput(testResults, testFailureInfo, coverageSession), output.Item2);
        }

        private static Tuple<int, string> RunTestHost(string command, RunStartParams rsp)
        {
            string testRunnerPath = rsp.TestHostPath.Item;
            var output = ExecuteProcess(
                testRunnerPath,
                Core.TestHost.buildCommandLine(command, rsp)
            );

            return output;
        }

        private static RunStepResult DiscoverSequencePoints(IRunExecutorHost host, RunStartParams rsp, RunStepInfo rsi)
        {
            var sequencePoint = Instrumentation.GenerateSequencePointInfo(host, rsp);
            if (sequencePoint != null)
            {
                sequencePoint.Serialize(rsp.DataFiles.SequencePointStore);
            }

            return RunStepStatus.Succeeded.ToRSR(RunData.NewSequencePoints(sequencePoint), "Binaries Instrumented - which ones - TBD");
        }

        private static RunStepResult DiscoverUnitTests(IRunExecutorHost host, RunStartParams rsp, RunStepInfo rsi)
        {
            if (!host.CanContinue())
            {
                throw new OperationCanceledException();
            }

            var output = RunTestHost("discover", rsp);

            RunStepStatus rss = RunStepStatus.Succeeded;
            if (output.Item1 != 0)
            {
                rss = RunStepStatus.Failed;
            }

            var testsPerAssembly = PerDocumentLocationDTestCases.Deserialize(FilePath.NewFilePath(rsp.DataFiles.DiscoveredUnitDTestsStore.Item));

            return rss.ToRSR(RunData.NewTestCases(testsPerAssembly), "Unit Tests Discovered - which ones - TBD");
        }

        private static RunStepResult InstrumentBinaries(IRunExecutorHost host, RunStartParams rsp, RunStepInfo rsi)
        {
            Instrumentation.Instrument(host, rsp, DataStore.Instance.FindTest);

            return RunStepStatus.Succeeded.ToRSR(RunData.NoData, "Binaries Instrumented - which ones - TBD");
        }

        private static RunStepResult BuildSolutionSnapshot(IRunExecutorHost host, RunStartParams rsp, RunStepInfo rsi)
        {
            var output = ExecuteProcess(
                Path.Combine(
                    Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
                    string.Format(@"MSBuild\{0}\Bin\msbuild.exe", host.HostVersion)),
                string.Format(
                    @"/m /v:minimal /p:Configuration=Debug /p:CreateVsixContainer=false /p:DeployExtension=false /p:CopyVsixExtensionFiles=false /p:OutDir=""{1}\\"" ""{2}""",
                    host.HostVersion.ToString(),
                    rsp.Solution.BuildRoot.Item,
                    rsp.Solution.SnapshotPath.Item)
            );

            RunStepStatus rss = RunStepStatus.Succeeded;
            if (output.Item1 != 0)
            {
                rss = RunStepStatus.Failed;
            }

            return rss.ToRSR(RunData.NoData, output.Item2);
        }

        private static RunStepResult TakeSolutionSnapshot(IRunExecutorHost host, RunStartParams rsp, RunStepInfo rsi)
        {
            var solutionGrandParentPath = Path.GetDirectoryName(Path.GetDirectoryName(rsp.Solution.Path.Item));
            VsSolution.GetProjects(host.HostVersion, rsp.Solution.Path.Item).ToList().ForEach(p =>
            {
                if (!host.CanContinue())
                {
                    throw new OperationCanceledException();
                }

                var projectFile = Path.Combine(Path.GetDirectoryName(rsp.Solution.Path.Item), p.RelativePath);
                var folder = Path.GetDirectoryName(projectFile);
                CopyFiles(rsp, solutionGrandParentPath, folder, SearchOption.AllDirectories);
            });
            CopyFiles(rsp, solutionGrandParentPath, Path.GetDirectoryName(rsp.Solution.Path.Item), SearchOption.TopDirectoryOnly);
            CopyFiles(rsp, solutionGrandParentPath, Path.Combine(Path.GetDirectoryName(rsp.Solution.Path.Item), "packages"), SearchOption.AllDirectories);

            return RunStepStatus.Succeeded.ToRSR(RunData.NoData, "What was done - TBD");
        }

        private static void CopyFiles(RunStartParams rsp, string solutionGrandParentPath, string folder, SearchOption searchOpt)
        {
            if (!new DirectoryInfo(folder).Exists)
            {
                return;
            }

            foreach (var src in Directory.EnumerateFiles(folder, "*", searchOpt))
            {
                if (src.IndexOf(@"\.git\", 0, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    continue;
                }

                var dst = src.ToUpperInvariant().Replace(solutionGrandParentPath.ToUpperInvariant(), rsp.SnapShotRoot.Item);
                var srcInfo = new FileInfo(src);
                var dstInfo = new FileInfo(dst);

                if (srcInfo.LastWriteTimeUtc > dstInfo.LastWriteTimeUtc)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dst));
                    Logger.I.LogInfo("Copying: {0} - {1}.", src, dst);
                    File.Copy(src, dst, true);
                }
            }
        }

        private static RunStepResult DeleteBuildOutput(IRunExecutorHost host, RunStartParams rsp, RunStepInfo rsi)
        {
            if (Directory.Exists(rsp.Solution.BuildRoot.Item))
            {
                foreach (var file in Directory.EnumerateFiles(rsp.Solution.BuildRoot.Item, "*.pdb"))
                {
                    File.Delete(file);

                    var dll = Path.ChangeExtension(file, "dll");
                    if (File.Exists(dll))
                    {
                        File.Delete(dll);
                    }
                }
            }

            return RunStepStatus.Succeeded.ToRSR(RunData.NoData, "What was done - TBD");
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
                delegate (object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    consoleOutput.Enqueue(e.Data);
                    Logger.I.LogInfo(e.Data);
                }
            );
            process.ErrorDataReceived += new DataReceivedEventHandler
            (
                delegate (object sender, DataReceivedEventArgs e)
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
