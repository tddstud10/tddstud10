using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using R4nd0mApps.TddStud10.Engine.Diagnostics;
using R4nd0mApps.TddStud10.TestHost;

namespace R4nd0mApps.TddStud10.Engine
{
    // TODO: Move to fs
    public interface IEngineHost
    {
        bool CanStart();

        void RunStarting();

        void RunStepStarting(string stepDetails);

        void RunEnded();
    }

    // TODO: Cleanup: Move to fs
    // TODO: Cleanup: Remove the ugly delegates
    public static class EngineLoader
    {
        private static EventHandler runStartingHandler;
        private static EventHandler<string> runStepStartingHandler;
        private static EventHandler runEndedHandler;
        private static EngineFileSystemWatcher efsWatcher;
        private static IEngineHost _host;

        public static void Load(IEngineHost host, string solutionPath, DateTime sessionStartTime)
        {
            Logger.I.Log("Loading Engine with solution {0}", solutionPath);

            _host = host;
            runStartingHandler = (o, ea) => host.RunStarting();
            runStepStartingHandler = (o, ea) => host.RunStepStarting(ea);
            runEndedHandler = (o, ea) => host.RunEnded();

            Engine.Instance = new Engine(host, solutionPath, sessionStartTime);
            Engine.Instance.RunStarting += runStartingHandler;
            Engine.Instance.RunStepStarting += runStepStartingHandler;
            Engine.Instance.RunEnded += runEndedHandler;

            efsWatcher = EngineFileSystemWatcher.Create(solutionPath, RunEngine);
        }

        public static bool IsEngineEnabled()
        {
            var enabled = efsWatcher.IsEnabled();
            Logger.I.Log("Engine is {0}", enabled);

            return enabled;
        }

        public static void EnableEngine()
        {
            Logger.I.Log("Enable Engine...");
            efsWatcher.Enable();
        }

        public static void DisableEngine()
        {
            Logger.I.Log("Disable Engine...");
            efsWatcher.Disable();
        }

        public static void UpdateCoverageResults(SequencePointSession seqPtSession, CoverageSession coverageSession, TestDetails testDetails)
        {
            CoverageData.Instance.UpdateCoverageResults(seqPtSession, coverageSession, testDetails);
        }

        public static void Unload()
        {
            Logger.I.Log("Unloading Engine...");

            efsWatcher.Dispose();

            Engine.Instance.RunStarting -= runStartingHandler;
            Engine.Instance.RunStepStarting -= runStepStartingHandler;
            Engine.Instance.RunEnded -= runEndedHandler;

            runStartingHandler = null;
            runStepStartingHandler = null;
            runEndedHandler = null;
            Engine.Instance = null;
        }

        public static bool IsRunInProgress()
        {
            if (Engine.Instance != null && Engine.Instance.IsRunInProgress())
            {
                return true;
            }

            return false;
        }

        private static void RunEngine()
        {
            if (Engine.Instance != null)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {
                    if (!_host.CanStart())
                    {
                        Logger.I.Log("Cannot start engine. Host has denied request.");
                        return;
                    }

                    if (!Engine.Instance.Start())
                    {
                        Logger.I.Log("Cannot start engine. A run is already on.");
                        return;
                    }

                    var serializer = new XmlSerializer(typeof(SequencePointSession));
                    var res = File.ReadAllText(Engine.Instance.SequencePointStore);
                    SequencePointSession seqPtSession = null;
                    StringReader reader = new StringReader(res);
                    XmlTextReader xmlReader = new XmlTextReader(reader);
                    try
                    {
                        seqPtSession = serializer.Deserialize(xmlReader) as SequencePointSession;
                    }
                    finally
                    {
                        xmlReader.Close();
                        reader.Close();
                    }

                    serializer = new XmlSerializer(typeof(CoverageSession));
                    res = File.ReadAllText(Engine.Instance.CoverageResults);
                    CoverageSession coverageSession = null;
                    reader = new StringReader(res);
                    xmlReader = new XmlTextReader(reader);
                    try
                    {
                        coverageSession = serializer.Deserialize(xmlReader) as CoverageSession;
                    }
                    finally
                    {
                        xmlReader.Close();
                        reader.Close();
                    }

                    TestDetails testDetails = null;
                    res = File.ReadAllText(Engine.Instance.TestResults);
                    reader = new StringReader(res);
                    xmlReader = new XmlTextReader(reader);
                    try
                    {
                        testDetails = TestDetails.Serializer.Deserialize(xmlReader) as TestDetails;
                    }
                    finally
                    {
                        xmlReader.Close();
                        reader.Close();
                    }

                    UpdateCoverageResults(seqPtSession, coverageSession, testDetails);
                }, null);
            }
            else
            {
                Logger.I.Log("Engine is not loaded. Ignoring command.");
            }
        }
    }
}
