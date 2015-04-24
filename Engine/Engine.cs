using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace R4nd0mApps.TddStud10
{
    public enum TestResult
    {
        Failed,
        Skipped,
        Passed,
    }

    public class TestDetail
    {
        public string Assembly { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
        public string ReturnType { get; set; }
        public string Name { get; set; }
    }

    public class TestDetails
    {
        public SerializableDictionary<string, TestResult> Dictionary
        {
            get;
            set;
        }

        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(TestDetails));

        public TestDetails()
        {
            Dictionary = new SerializableDictionary<string, TestResult>();
        }
    }

    public class Engine
    {
        private string _solutionPath;
        private string _solutionGrandParentPath;

        public Engine(string solutionPath)
        {
            _solutionPath = solutionPath;
            _solutionGrandParentPath = Path.GetDirectoryName(Path.GetDirectoryName(_solutionPath));
        }

        private string snapshotRoot = @"d:\tddstud10";
        private string solutionBuildRoot
        {
            get
            {
                return Path.Combine(snapshotRoot, Path.GetFileName(Path.GetDirectoryName(_solutionPath)) + ".out");
                //return @"d:\tddstud10\xunit.out\";
            }
        }
        private string solutionSnapShotPath
        {
            get
            {
                return Path.Combine(snapshotRoot, Path.GetFileName(Path.GetDirectoryName(_solutionPath)), Path.GetFileName(_solutionPath));
                // return @"d:\tddstud10\xunit\xunit.vs2013.NoXamarin.sln";
            }
        }

        public string CoverageResults
        {
            get
            {
                return Path.Combine(solutionBuildRoot, "results.xml");
            }
        }

        public string TestResults
        {
            get
            {
                return Path.Combine(solutionBuildRoot, "testresults.txt");
            }
        }

        public static Engine Instance { get; set; }

        public bool ArePathsTheSame(string path1, string path2)
        {
            if (path1.Equals(path2, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (path1.ToUpperInvariant().Replace(snapshotRoot.ToUpperInvariant(), "")
                .Equals(path2.ToUpperInvariant().Replace(_solutionGrandParentPath.ToUpperInvariant(), "")))
            {
                return true;
            }

            if (path2.ToUpperInvariant().Replace(snapshotRoot.ToUpperInvariant(), "")
                .Equals(path1.ToUpperInvariant().Replace(_solutionGrandParentPath.ToUpperInvariant(), "")))
            {
                return true;
            }

            return false;
        }

        public void DisplayFileSystemWatcherInfo(Action<string> AddListLine)
        {
            string currFolder = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            string testRunnerPath = Path.Combine(Path.GetDirectoryName(currFolder), "TddStud10.TestHost.exe");

            // TODO: Stop a barrage of events
            // TODO: Stop duplicate events
            // TODO: VisualStudioVersion=12.0
            // TODO: Multi-threaded build of xunit.net is buggy
            Stopwatch stopWatch = new Stopwatch();
            TimeSpan ts;
            string elapsedTime;

            // TODO: File copy and file discovery in parallel
            // TODO: File copy can be multi-threaded
            AddListLine("Copying files...");
            stopWatch.Start();
            var sln = new Solution(_solutionPath);
            sln.Projects.ForEach(p =>
            {
                var projectFile = Path.Combine(Path.GetDirectoryName(_solutionPath), p.RelativePath);
                var folder = Path.GetDirectoryName(projectFile);
                foreach (var src in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                {
                    // TODO: Can filter out specific folders - e.g. .git
                    var dst = src.ToUpperInvariant().Replace(_solutionGrandParentPath.ToUpperInvariant(), snapshotRoot);
                    var srcInfo = new FileInfo(src);
                    var dstInfo = new FileInfo(dst);

                    if (srcInfo.LastWriteTimeUtc > dstInfo.LastWriteTimeUtc)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(dst));
                        AddListLine(dst);
                        File.Copy(src, dst, true);
                    }
                }
            });

            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            AddListLine("Done! [" + elapsedTime + "]");
            AddListLine("/////////////////////////////////////////////////////////////////////////");
            AddListLine("/////////////////////////////////////////////////////////////////////////");

            // TODO: Registration free COM for opencover

            // TODO: Just build the dirty projects
            // TODO: Get a robust stdout/err redirector from msbuild
            AddListLine("Building project...");
            stopWatch.Start();
            ExecuteProcess(
                @"c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe",
                string.Format(@"/m /v:minimal /p:VisualStudioVersion=12.0 /p:OutDir={0} {1}", solutionBuildRoot, solutionSnapShotPath),
                AddListLine
            );
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            AddListLine("Done! [" + elapsedTime + "]");
            AddListLine("/////////////////////////////////////////////////////////////////////////");
            AddListLine("/////////////////////////////////////////////////////////////////////////");

            AddListLine("Discovering tests...");
            stopWatch.Start();
            ExecuteProcess(
                testRunnerPath,
                string.Format(@"discover {0} {1}", solutionBuildRoot, Path.Combine(solutionBuildRoot, "testcases.txt")),
                AddListLine
            );
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            AddListLine("Done! [" + elapsedTime + "]");
            AddListLine("/////////////////////////////////////////////////////////////////////////");
            AddListLine("/////////////////////////////////////////////////////////////////////////");

            AddListLine("Executing tests...");
            stopWatch.Start();
            // TODO: -coverbytest
            // TODO: Ignore our namespaces
            // TODO: path32 or path64?
            // TODO: Coverbytest has hardcoded filter
            ExecuteProcess(
                @"D:\Users\ParthoP\Downloads\opencover.4.5.3522\OpenCover.Console.exe",
                string.Format(
                    @"-mergebyhash ""-output:{3}"" -register:user -returntargetcode:10000 ""-target:{0}"" ""-targetargs:{1}"" ""-targetdir:{2}"" -coverbytest:*.UnitTests.dll"
                    , testRunnerPath
                    , string.Format(@"execute {0} {1} {2}", solutionBuildRoot, Path.Combine(solutionBuildRoot, "testcases.txt"), TestResults)
                    , solutionBuildRoot
                    , CoverageResults
                ),
                AddListLine
            );
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            AddListLine("Done! [" + elapsedTime + "]");
            AddListLine("");
        }

        private void ExecuteProcess(string fileName, string arguments, Action<string> AddListLine)
        {
            AddListLine(string.Format("Executing: '{0}' '{1}'", fileName, arguments));
            ProcessStartInfo processStartInfo;
            Process process;

            processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false;
            // TODO: Multiprocess build for MSBuild sometimes causes trouble - put back the /m option.
            processStartInfo.Arguments = arguments;
            processStartInfo.FileName = fileName;

            process = new Process();
            process.StartInfo = processStartInfo;
            // enable raising events because Process does not raise events by default
            process.EnableRaisingEvents = true;
            // attach the event handler for OutputDataReceived before starting the process
            process.OutputDataReceived += new DataReceivedEventHandler
            (
                delegate(object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    AddListLine(e.Data);
                }
            );
            process.ErrorDataReceived += new DataReceivedEventHandler
            (
                delegate(object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    AddListLine(e.Data);
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
    }
}

/*

 C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe D:\src\r4nd0mkatas\fizzbuzz\FizzBuzz.sln /p:VisualStudioVersion=12.0 /t:rebuild /v:minimal                                                                                                                                                                
 D:\src\r4nd0mapps\WpfApplication2\packages\opencover.4.5.3522\OpenCover.Console.exe -register:user -target:D:\src\r4nd0mapps\WpfApplication2\packages\xunit.runner.console.2.0.0\tools\xunit.console.exe -targetargs:"D:\src\r4nd0mkatas\fizzbuzz\FizzBuzz.UnitTests\bin\Debug\FizzBuzz.UnitTests.dll" -targetdir:D:\src\r4nd0mkatas\fizzbuzz\FizzBuzz.UnitTests\bin\Debug -mergebyhash -output:D:\src\r4nd0mkatas\fizzbuzz\results.0.xml -showunvisited -returntargetcode:10000
 D:\src\r4nd0mapps\WpfApplication2\packages\ReportGenerator.2.1.4.0\ReportGenerator.exe -reports:D:\src\r4nd0mkatas\fizzbuzz\results.0.xml -targetdir:D:\src\r4nd0mkatas\fizzbuzz\rep
 */
