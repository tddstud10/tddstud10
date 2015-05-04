using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.FSharp.Core;
using R4nd0mApps.TddStud10.Engine.Core;
using R4nd0mApps.TddStud10.Engine.Diagnostics;
using R4nd0mApps.TddStud10.TestHost;

/*
    TODO:
    √ extract methods
    √ transform signatures
    √ support cancellation
    √ return ds directly, not write and read xml's, serialize in parallel threads
    √ capture return value errors from build adn test
    √ capture console output from build adn test
    √ events should have rundata
    - gaurd from reentrancy
    - morph engine to runexeutor
      - wire up handlers
      - cleanup engine
      - implement IRunExecutorHost
      - make the switch
    - remove the reading xmls from engineloader
    - write errors in toolwindow, clean for every session
    - click on the dots should open the toolwindow
    - bitmaps
 */

namespace R4nd0mApps.TddStud10.Engine
{
    public class RunExecutorHost : IRunExecutorHost
    {

        #region IRunExecutorHost Members

        public bool CanContinue()
        {
            return true;
        }

        #endregion
    }

    public sealed class Engine
    {
        private string _solutionPath;
        private string _solutionGrandParentPath;

        public event EventHandler RunStarting;
        public event EventHandler<string> RunStepStarting;
        public event EventHandler RunEnded;

        public Engine(IEngineHost host, string solutionPath, DateTime sessionStartTime)
        {
            _sessionStartTime = sessionStartTime;
            _solutionPath = solutionPath;
            _solutionGrandParentPath = Path.GetDirectoryName(Path.GetDirectoryName(_solutionPath));
        }

        private string _snapshotRoot = @"d:\tddstud10";
        private bool _running;
        private DateTime _sessionStartTime;

        public string SolutionBuildRoot
        {
            get
            {
                return Path.Combine(_snapshotRoot, Path.GetFileName(Path.GetDirectoryName(_solutionPath)) + ".out");
            }
        }
        private string solutionSnapShotPath
        {
            get
            {
                return Path.Combine(_snapshotRoot, Path.GetFileName(Path.GetDirectoryName(_solutionPath)), Path.GetFileName(_solutionPath));
            }
        }

        public string SequencePointStore
        {
            get
            {
                return Path.Combine(SolutionBuildRoot, "Z_sequencePointStore.xml");
            }
        }

        public string CoverageResults
        {
            get
            {
                return Path.Combine(SolutionBuildRoot, "Z_coverageresults.xml");
            }
        }

        public string TestResults
        {
            get
            {
                return Path.Combine(SolutionBuildRoot, "Z_testresults.xml");
            }
        }

        public string DiscoveredUnitTestsStore
        {
            get
            {
                return Path.Combine(SolutionBuildRoot, "Z_discoveredUnitTests.xml");
            }
        }

        public static Engine Instance { get; set; }

        public bool ArePathsTheSame(string path1, string path2)
        {
            if (path1.Equals(path2, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (path1.ToUpperInvariant().Replace(_snapshotRoot.ToUpperInvariant(), "")
                .Equals(path2.ToUpperInvariant().Replace(_solutionGrandParentPath.ToUpperInvariant(), "")))
            {
                return true;
            }

            if (path2.ToUpperInvariant().Replace(_snapshotRoot.ToUpperInvariant(), "")
                .Equals(path1.ToUpperInvariant().Replace(_solutionGrandParentPath.ToUpperInvariant(), "")))
            {
                return true;
            }

            return false;
        }

        public bool Start()
        {
            lock (this)
            {
                if (_running)
                {
                    Logger.I.LogInfo("Ignoring start as engine is currently _running...");
                    return false;
                }

                _running = true;
            }

            try
            {
                OnRaiseRunStartingEvent();

                var reh = new RunExecutorHost();
                RunData rd = new RunData(
                                _sessionStartTime,
                                FilePath.NewFilePath(_solutionPath),
                                FilePath.NewFilePath(solutionSnapShotPath),
                                FilePath.NewFilePath(SolutionBuildRoot),
                                FSharpOption<SequencePoints>.None,
                                FSharpOption<DiscoveredUnitTests>.None,
                                FSharpOption<string>.None,
                                FSharpOption<CoverageSession>.None,
                                FSharpOption<TestResults>.None,
                                FSharpOption<string>.None);

                Stopwatch stopWatch = new Stopwatch();
                TimeSpan ts;
                string elapsedTime;

                // Delte files
                OnRaiseRunStepStarting("Deleting build output...");
                Logger.I.LogInfo("Deleting build output...");
                stopWatch.Start();
                rd = DeleteBuildOutput(reh, rd);
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.LogInfo("Done deleting build output! [" + elapsedTime + "]");
                Logger.I.LogInfo("/////////////////////////////////////////////////////////////////////////");
                Logger.I.LogInfo("/////////////////////////////////////////////////////////////////////////");

                OnRaiseRunStepStarting("Taking solution snapshot...");
                Logger.I.LogInfo("Taking solution snapshot...");
                stopWatch.Start();
                rd = TakeSolutionSnapshot(reh, rd);
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.LogInfo("Done taking solution snapshot! [" + elapsedTime + "]");
                Logger.I.LogInfo("/////////////////////////////////////////////////////////////////////////");
                Logger.I.LogInfo("/////////////////////////////////////////////////////////////////////////");

                OnRaiseRunStepStarting("Building project...");
                Logger.I.LogInfo("Building project...");
                stopWatch.Start();
                try
                {
                    rd = BuildSolutionSnapshot(reh, rd);
                }
                catch (Exception e)
                {
                    Logger.I.LogInfo(e.ToString());
                }
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.LogInfo("Done building project! [" + elapsedTime + "]");
                Logger.I.LogInfo("/////////////////////////////////////////////////////////////////////////");
                Logger.I.LogInfo("/////////////////////////////////////////////////////////////////////////");

                OnRaiseRunStepStarting("Instrumenting and discovering tests...");
                Logger.I.LogInfo("Instrumenting and discovering tests...");
                stopWatch.Start();
                rd = InstrumentBinaries(reh, rd);
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.LogInfo("Done instrumenting and discovering tests! [" + elapsedTime + "]");
                Logger.I.LogInfo("/////////////////////////////////////////////////////////////////////////");
                Logger.I.LogInfo("/////////////////////////////////////////////////////////////////////////");

                OnRaiseRunStepStarting("Executing tests...");
                Logger.I.LogInfo("Executing tests...");
                stopWatch.Start();
                try
                {
                    rd = RunTests(reh, rd);
                }
                catch (Exception e)
                {
                    Logger.I.LogInfo(e.ToString());
                }
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.LogInfo("Done executing tests! [" + elapsedTime + "]");
                Logger.I.LogInfo("");

                OnRaiseRunEndedEvent();
            }
            finally
            {
                lock (this)
                {
                    _running = false;
                }
            }

            return true;
        }

        private static RunData RunTests(IRunExecutorHost host, RunData rd)
        {
            string testRunnerPath = Path.GetFullPath(typeof(R4nd0mApps.TddStud10.TestHost.Program).Assembly.Location);
            var output = ExecuteProcess(
                testRunnerPath,
                string.Format(
                    @"execute {0} {1} {2} {3}",
                    rd.solutionBuildRoot.Item,
                    Path.Combine(rd.solutionBuildRoot.Item, "Z_coverageresults.xml"),
                    Path.Combine(rd.solutionBuildRoot.Item, "Z_testresults.xml"),
                    Path.Combine(rd.solutionBuildRoot.Item, "Z_discoveredUnitTests.xml")
                )
            );

            if (output.Item1 != 0)
            {
                throw new Exception(output.Item2);
            }

            return CreateRunDataForRunTest(rd, output.Item2);
        }

        private static RunData CreateRunDataForRunTest(RunData rd, string testConsoleOutput)
        {
            return new RunData(
                rd.startTime,
                rd.solutionPath,
                rd.solutionSnapshotPath,
                rd.solutionBuildRoot,
                rd.sequencePoints,
                rd.discoveredUnitTests,
                rd.buildConsoleOutput,
                rd.codeCoverageResults,
                rd.executedTests,
                new FSharpOption<string>(testConsoleOutput));
        }

        private static RunData InstrumentBinaries(IRunExecutorHost host, RunData rd)
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

            return CreateRunDataForInstrumentationStep(rd, dict, unitTests);
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
                rd.buildConsoleOutput,
                rd.codeCoverageResults,
                rd.executedTests,
                rd.testConoleOutput);
        }

        private static RunData BuildSolutionSnapshot(IRunExecutorHost host, RunData rd)
        {
            string testRunnerPath = Path.GetFullPath(typeof(R4nd0mApps.TddStud10.TestHost.Program).Assembly.Location);
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

            if (output.Item1 != 0)
            {
                throw new Exception(output.Item2);
            }

            return CreateRunDataForBuildSolution(rd, output.Item2);
        }

        private static RunData CreateRunDataForBuildSolution(RunData rd, string buildConsoleOutput)
        {
            return new RunData(
                rd.startTime,
                rd.solutionPath,
                rd.solutionSnapshotPath,
                rd.solutionBuildRoot,
                rd.sequencePoints,
                rd.discoveredUnitTests,
                new FSharpOption<string>(buildConsoleOutput),
                rd.codeCoverageResults,
                rd.executedTests,
                rd.buildConsoleOutput);
        }

        private static RunData TakeSolutionSnapshot(IRunExecutorHost host, RunData rd)
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
                    var dst = src.ToUpperInvariant().Replace(solutionGrandParentPath.ToUpperInvariant(), rd.solutionSnapshotPath.Item);
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

            return rd;
        }

        private static RunData DeleteBuildOutput(IRunExecutorHost host, RunData rd)
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

            return rd;
        }

        public bool IsRunInProgress()
        {
            return _running;
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

        private void OnRaiseRunStartingEvent()
        {
            var handler = RunStarting;

            // Event will be null if there are no subscribers 
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void OnRaiseRunStepStarting(string stepDetails)
        {
            var handler = RunStepStarting;

            // Event will be null if there are no subscribers 
            if (handler != null)
            {
                handler(this, stepDetails);
            }
        }

        private void OnRaiseRunEndedEvent()
        {
            var handler = RunEnded;

            // Event will be null if there are no subscribers 
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}
