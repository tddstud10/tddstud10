using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace R4nd0mApps.TddStud10.TestRuntime
{
    public static class TestRunTimeInstaller
    {
        public static string Install(string destination)
        {
            var files = new List<string>
                {
                    Path.GetFullPath(typeof(R4nd0mApps.TddStud10.TestRuntime.Marker).Assembly.Location),
                };

            var sb = new StringBuilder();
            files.ForEach(
                src =>
                {
                    var dst = Path.Combine(destination, Path.GetFileName(src));
                    if (File.Exists(dst))
                    {
                        File.Delete(dst);
                    }
                    File.Copy(src, dst);
                    sb.AppendFormat("{0} -> {1}{2}", src, dst, Environment.NewLine);
                });

            return sb.ToString();
        }
    }
}
