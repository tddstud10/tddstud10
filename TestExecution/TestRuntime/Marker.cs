using System.Diagnostics;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using R4nd0mApps.TddStud10.TestRuntime.Diagnostics;

namespace R4nd0mApps.TddStud10.TestRuntime
{
    public static class Marker
    {
        //private static ConcurrentDictionary<int, Lazy<List<string[]>>> store = new ConcurrentDictionary<int, Lazy<List<string[]>>>();

        private const string TESTRUNID_SLOTNAME = "Marker.TestRunId";

        private static Lazy<ICoverageDataCollector> channel = new Lazy<ICoverageDataCollector>(CreateChannel);

        public static string TestRunId
        {
            get { return CallContext.LogicalGetData(TESTRUNID_SLOTNAME) as string; }
            set { CallContext.LogicalSetData(TESTRUNID_SLOTNAME, value); }
        }

        public static void EnterSequencePoint(string assemblyId, string methodMdRid, string spId)
        {
            //var list = store.GetOrAdd(Thread.CurrentThread.ManagedThreadId, new Lazy<List<string[]>>(() => new List<string[]>())).Value;
            //list.Add(new[] { mvid, mdToken, spid });
            if (TestRunId == null)
            {
                TestRunId = new object().GetHashCode().ToString(CultureInfo.InvariantCulture);
            }

            channel.Value.EnterSequencePoint(TestRunId, assemblyId, methodMdRid, spId);
        }

        public static void ExitUnitTest(string source, string document, string line)
        {
            //Logger.I.LogError("Marker: Exiting unit test {0},{1},{2}.", source, document, line);
            //var currThreadId = Thread.CurrentThread.ManagedThreadId;
            //Lazy<List<string[]>> list;
            //if (store.TryRemove(currThreadId, out list))
            //{
            //    channel.Value.ExitUnitTest(source, document, line, list.Value);
            //    Logger.I.LogError("Marker: Exiting unit test {0},{1},{2}. Sequence Points = {3}", source, document, line, list.Value.Count);
            //}
            //else
            //{
            //    Logger.I.LogError("Marker: Did not have any sequence points in thread {0} for {1},{2},{3}.", currThreadId, source, document, line);
            //}
            if (TestRunId == null)
            {
                Logger.I.LogError("Marker: Appears we did not have any sequence points for {0},{1},{2}.", source, document, line);
            }

            channel.Value.ExitUnitTest(TestRunId, source, document, line);
            TestRunId = null;
        }

        private static ICoverageDataCollector CreateChannel()
        {
            string address = string.Format(
                "net.pipe://localhost/r4nd0mapps/tddstud10/CodeCoverageDataCollector/{0}",
                Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture));

            Logger.I.LogInfo("Marker: Initiating connection to {0} ...", address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(address);
            var ret = ChannelFactory<ICoverageDataCollector>.CreateChannel(binding, ep);
            ret.Ping();
            Logger.I.LogInfo("Marker: Connected to server.", address);

            return ret;
        }
    }
}
