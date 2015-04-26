using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R4nd0mApps.TddStud10.Engine.Diagnostics;

namespace R4nd0mApps.TddStud10.Engine
{
    public class EngineLoader
    {
        private static FileSystemWatcher fsWatcher;

        public static void Load(string solutionPath)
        {
            Engine.Instance = new Engine(solutionPath);
            Engine.Instance.Start(Logger.I.Log);

            fsWatcher = new FileSystemWatcher();
            fsWatcher.Filter = "*";
            fsWatcher.Path = Path.GetDirectoryName(solutionPath);

            SubscribeToEvents();
        }

        public static void Unload()
        {
            UnsubscribeToEvents();

            fsWatcher.Dispose();
        }

        public static void EnableEngine()
        {
            fsWatcher.EnableRaisingEvents = true;
        }

        private static void SubscribeToEvents()
        {
            fsWatcher.Created += fsWatcher_Created;
            fsWatcher.Changed += fsWatcher_Changed;
            fsWatcher.Renamed += fsWatcher_Renamed;
            fsWatcher.Deleted += fsWatcher_Deleted;
            fsWatcher.Error += fsWatcher_Error;
        }

        private static void UnsubscribeToEvents()
        {
            fsWatcher.Error -= fsWatcher_Error;
            fsWatcher.Deleted -= fsWatcher_Deleted;
            fsWatcher.Renamed -= fsWatcher_Renamed;
            fsWatcher.Changed -= fsWatcher_Changed;
            fsWatcher.Created -= fsWatcher_Created;
        }

        static void fsWatcher_Error(object sender, ErrorEventArgs e)
        {
            Logger.I.LogError(e.ToString());
        }

        static void fsWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Engine.Instance.Start(Logger.I.Log);
        }

        static void fsWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Engine.Instance.Start(Logger.I.Log);
        }

        static void fsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Engine.Instance.Start(Logger.I.Log);
        }

        static void fsWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Engine.Instance.Start(Logger.I.Log);
        }
    }
}
