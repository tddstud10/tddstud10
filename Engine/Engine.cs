using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using R4nd0mApps.TddStud10.Engine.Diagnostics;


// TODO: Icons for ttdstud10 - fody pointers to icon generators
namespace R4nd0mApps.TddStud10.Engine
{
    public sealed class Engine
    {
        private string _solutionPath;
        private string _solutionGrandParentPath;

        public event EventHandler RunStarting;
        public event EventHandler<string> RunStepStarting;
        public event EventHandler RunEnded;

        public  Engine(DateTime sessionStartTime, string solutionPath)
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
                return Path.Combine(SolutionBuildRoot, "Z_seqpoints.xml");
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

        public void Start()
        {
            lock (this)
            {
                if (_running)
                {
                    Logger.I.Log("Ignoring start as engine is currently _running...");
                    return;
                }

                _running = true;
            }

            try
            {
                OnRaiseRunStartingEvent();

                string currFolder = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
                string testRunnerPath = Path.Combine(Path.GetDirectoryName(currFolder), "TddStud10.TestHost.exe");

                // TODO: Multi-threaded build of xunit.net is buggy
                Stopwatch stopWatch = new Stopwatch();
                TimeSpan ts;
                string elapsedTime;

                // Delte files
                OnRaiseRunStepStarting("Deleting build output...");
                Logger.I.Log("Deleting build output...");
                stopWatch.Start();
                foreach (var file in Directory.EnumerateFiles(SolutionBuildRoot, "*.pdb"))
                {
                    File.Delete(file);

                    var dll = Path.ChangeExtension(file, "dll");
                    if (File.Exists(dll))
                    {
                        File.Delete(dll);
                    }
                }
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.Log("Done deleting build output! [" + elapsedTime + "]");
                Logger.I.Log("/////////////////////////////////////////////////////////////////////////");
                Logger.I.Log("/////////////////////////////////////////////////////////////////////////");

                // TODO: File copy and file discovery in parallel
                // TODO: File copy can be multi-threaded
                OnRaiseRunStepStarting("Taking solution snapshot...");
                Logger.I.Log("Taking solution snapshot...");
                stopWatch.Start();
                var sln = new Solution(_solutionPath);
                sln.Projects.ForEach(p =>
                {
                    var projectFile = Path.Combine(Path.GetDirectoryName(_solutionPath), p.RelativePath);
                    var folder = Path.GetDirectoryName(projectFile);
                    foreach (var src in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                    {
                        // TODO: Can filter out specific folders - e.g. .git
                        var dst = src.ToUpperInvariant().Replace(_solutionGrandParentPath.ToUpperInvariant(), _snapshotRoot);
                        var srcInfo = new FileInfo(src);
                        var dstInfo = new FileInfo(dst);

                        if (srcInfo.LastWriteTimeUtc > dstInfo.LastWriteTimeUtc)
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(dst));
                            Logger.I.Log(dst);
                            File.Copy(src, dst, true);
                        }
                    }
                });

                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.Log("Done taking solution snapshot! [" + elapsedTime + "]");
                Logger.I.Log("/////////////////////////////////////////////////////////////////////////");
                Logger.I.Log("/////////////////////////////////////////////////////////////////////////");

                // TODO: Just build the dirty projects
                // TODO: Get a robust stdout/err redirector from msbuild
                OnRaiseRunStepStarting("Building project...");
                Logger.I.Log("Building project...");
                stopWatch.Start();
                ExecuteProcess(
                    @"c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe",
                    string.Format(@"/m /v:minimal /p:VisualStudioVersion=12.0 /p:OutDir={0} {1}", SolutionBuildRoot, solutionSnapShotPath)
                );
                if (File.Exists(Path.Combine(SolutionBuildRoot, Path.GetFileName(testRunnerPath))))
                {
                    File.Delete(Path.Combine(SolutionBuildRoot, Path.GetFileName(testRunnerPath)));
                }
                File.Copy(testRunnerPath, Path.Combine(SolutionBuildRoot, Path.GetFileName(testRunnerPath)));
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.Log("Done building project! [" + elapsedTime + "]");
                Logger.I.Log("/////////////////////////////////////////////////////////////////////////");
                Logger.I.Log("/////////////////////////////////////////////////////////////////////////");

                OnRaiseRunStepStarting("Instrumenting and discovering tests...");
                Logger.I.Log("Instrumenting and discovering tests...");
                stopWatch.Start();
                Instrumentation.GenerateSequencePointInfo(_sessionStartTime, SolutionBuildRoot, SequencePointStore);
                Instrumentation.Instrument(_sessionStartTime, SolutionBuildRoot, DiscoveredUnitTestsStore);
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.Log("Done instrumenting and discovering tests! [" + elapsedTime + "]");
                Logger.I.Log("/////////////////////////////////////////////////////////////////////////");
                Logger.I.Log("/////////////////////////////////////////////////////////////////////////");

                OnRaiseRunStepStarting("Executing tests...");
                Logger.I.Log("Executing tests...");
                stopWatch.Start();
                ExecuteProcess(
                    testRunnerPath,
                    string.Format(
                        @"execute {0} {1} {2} {3}",
                        SolutionBuildRoot, 
                        CoverageResults, 
                        TestResults,
                        DiscoveredUnitTestsStore
                    )
                );
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                Logger.I.Log("Done executing tests! [" + elapsedTime + "]");
                Logger.I.Log("");

                OnRaiseRunEndedEvent();
            }
            finally
            {
                lock (this)
                {
                    _running = false;
                }
            }
        }

        private void ExecuteProcess(string fileName, string arguments)
        {
            Logger.I.Log(string.Format("Executing: '{0}' '{1}'", fileName, arguments));
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
                    Logger.I.Log(e.Data);
                }
            );
            process.ErrorDataReceived += new DataReceivedEventHandler
            (
                delegate(object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    Logger.I.Log(e.Data);
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

/*

 C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe D:\src\r4nd0mkatas\fizzbuzz\FizzBuzz.sln /p:VisualStudioVersion=12.0 /t:rebuild /v:minimal                                                                                                                                                                
 D:\src\r4nd0mapps\WpfApplication2\packages\opencover.4.5.3522\OpenCover.Console.exe -register:user -target:D:\src\r4nd0mapps\WpfApplication2\packages\xunit.runner.console.2.0.0\tools\xunit.console.exe -targetargs:"D:\src\r4nd0mkatas\fizzbuzz\FizzBuzz.UnitTests\bin\Debug\FizzBuzz.UnitTests.dll" -targetdir:D:\src\r4nd0mkatas\fizzbuzz\FizzBuzz.UnitTests\bin\Debug -mergebyhash -output:D:\src\r4nd0mkatas\fizzbuzz\results.0.xml -showunvisited -returntargetcode:10000
 D:\src\r4nd0mapps\WpfApplication2\packages\ReportGenerator.2.1.4.0\ReportGenerator.exe -reports:D:\src\r4nd0mkatas\fizzbuzz\results.0.xml -targetdir:D:\src\r4nd0mkatas\fizzbuzz\rep
 */
