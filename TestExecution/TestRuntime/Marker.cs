using System.Diagnostics;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using R4nd0mApps.TddStud10.TestRuntime.Diagnostics;

namespace R4nd0mApps.TddStud10.TestRuntime
{
    public static class Marker
    {
        private const string TESTRUNID_SLOTNAME = "Marker.TestRunId";

        private static LazyObject<ICoverageDataCollector> channel = new LazyObject<ICoverageDataCollector>(CreateChannel);

        private static string TestRunId
        {
            get { return CallContext.LogicalGetData(TESTRUNID_SLOTNAME) as string; }
            set { CallContext.LogicalSetData(TESTRUNID_SLOTNAME, value); }
        }

        public static void EnterSequencePoint(string assemblyId, string methodMdRid, string spId)
        {
            if (TestRunId == null)
            {
                TestRunId = new object().GetHashCode().ToString(CultureInfo.InvariantCulture);
            }

            channel.Value.EnterSequencePoint(TestRunId, assemblyId, methodMdRid, spId);
        }

        public static void ExitUnitTest(string source, string document, string line)
        {
            if (TestRunId == null)
            {
                Logger.I.LogError("Marker: Appears we did not have any sequence points for {0},{1},{2}.", source, document, line);
            }

            channel.Value.ExitUnitTest(TestRunId, source, document, line);
            TestRunId = null;
        }

        public static string CreateCodeCoverageDataCollectorEndpointAddress()
        {
            return string.Format(
                "net.pipe://localhost/r4nd0mapps/tddstud10/CodeCoverageDataCollector/{0}",
                Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture));
        }

        private static ICoverageDataCollector CreateChannel()
        {
            string address = CreateCodeCoverageDataCollectorEndpointAddress();

            Logger.I.LogInfo("Marker: Initiating connection to {0} ...", address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress epa = new EndpointAddress(address);
            var ret = ChannelFactory<ICoverageDataCollector>.CreateChannel(binding, epa);
            ret.Ping();
            Logger.I.LogInfo("Marker: Connected to server.", address);

            return ret;
        }
    }
}
