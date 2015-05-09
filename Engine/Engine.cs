using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.FSharp.Core;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Engine.Diagnostics;
using R4nd0mApps.TddStud10.TestHost;

/*
EngineOK, EngineNotOK 
Unknown, Red, Green
None, Build, Test
Running, Idle

 * 
 * [Final State] -> [Initial state] : Only by run start run
 * Idle -> Runing : Start step
 * Runing -> Idle : End step
 * 
 * [EngineNotOK]
 * Unknown, None, Idle          [Final State]

 * [EngineOK]
 * Unknown, None, Idle          [Initial state]
 * Unknown, None, Running
 * Unknown, Build, Running
 * ☒ Unknown, Build, Idle
 * ☒ Unknown, Test, Running
 * ☒ Unknown, Test, Idle
 * ☒ Red, None, Running
 * ☒ Red, None, Idle
 * Red, Build, Running
 * Red, Build, Idle             [Final State]
 * Red, Test, Running
 * Red, Test, Idle              [Final State]
 * ☒ Green, None, Running
 * ☒ Green, None, Idle
 * Green, Build, Running
 * ☒ Green, Build, Idle
 * Green, Test, Running
 * Green, Test, Idle            [Final State]

    TODO:
    ☑ Author state management in WPF
    ☐ Abstract state management for VS
    ☐ test VS Integration
    ☐ bitmaps for vs animation

    ☒ TRIAGED OUT:
        ☒ Engine loading/unloading in app is buggy - enable/disable/enable - two seperate agents launched
        ☒ fix fsunit
        ☒ host should not be able to change its mind about cancellation
        ☒ cancellationtoken wireup
        ☒ http://fsharp.org/specs/component-design-guidelines/
 
        ☒ write errors in toolwindow, clean for every session
        ☒ click on the dots should open the toolwindow
        ☒ Errors in red, Warnings in yellow - remove training ","
        ☒ Cheap debug - right click on one of the green, set bp, launch db
        ☒ Support theory
        ☒ Move stuff from engineloader to Runner
 */

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
            TestResults testResults = null;
            var res = File.ReadAllText(testResultsStore);
            var reader = new StringReader(res);
            var xmlReader = new XmlTextReader(reader);
            try
            {
                testResults = TestResults.Serializer.Deserialize(xmlReader) as TestResults;
            }
            finally
            {
                xmlReader.Close();
                reader.Close();
            }

            CoverageSession coverageSession = null;
            res = File.ReadAllText(coverageSessionStore);
            reader = new StringReader(res);
            xmlReader = new XmlTextReader(reader);
            try
            {
                coverageSession = CoverageSession.Serializer.Deserialize(xmlReader) as CoverageSession;
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
                rd.sequencePoints,
                rd.discoveredUnitTests,
                new FSharpOption<CoverageSession>(coverageSession),
                new FSharpOption<TestResults>(testResults));
        }

        private static RunStepResult InstrumentBinaries(IRunExecutorHost host, RunStepName name, RunStepKind kind, RunData rd)
        {
            var sequencePointStore = Path.Combine(rd.solutionBuildRoot.Item, "Z_sequencePointStore.xml");
            var dict = Instrumentation.GenerateSequencePointInfo(rd.startTime, rd.solutionBuildRoot.Item);
            if (dict != null)
            {
                using (StringWriter writer = new StringWriter())
                {
                    SequencePoints.Serializer.Serialize(writer, dict);
                    File.WriteAllText(sequencePointStore, writer.ToString());
                    Logger.I.LogInfo("Written sequence points to {0}.", sequencePointStore);
                }
            }

            if (!host.CanContinue())
            {
                throw new OperationCanceledException();
            }

            var discoveredUnitTestsStore = Path.Combine(rd.solutionBuildRoot.Item, "Z_discoveredUnitTests.xml");
            var unitTests = Instrumentation.Instrument(rd.startTime, Path.GetDirectoryName(rd.solutionPath.Item), rd.solutionBuildRoot.Item);
            using (StringWriter writer = new StringWriter())
            {
                DiscoveredUnitTests.Serializer.Serialize(writer, unitTests);
                File.WriteAllText(discoveredUnitTestsStore, writer.ToString());
                Logger.I.LogInfo("Written discovered unit tests to {0}.", discoveredUnitTestsStore);
            }

            var retRd = CreateRunDataForInstrumentationStep(rd, dict, unitTests);

            return retRd.ToRSR(name, kind, RunStepStatus.Succeeded, "Binaries Instrumented - which ones - TBD");
        }

        private static RunData CreateRunDataForInstrumentationStep(RunData rd, SequencePoints sequencePoints, DiscoveredUnitTests unitTests)
        {
            return new RunData(
                rd.startTime,
                rd.solutionPath,
                rd.solutionSnapshotPath,
                rd.solutionBuildRoot,
                new FSharpOption<SequencePoints>(sequencePoints),
                new FSharpOption<DiscoveredUnitTests>(unitTests),
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
                rd.sequencePoints,
                rd.discoveredUnitTests,
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
