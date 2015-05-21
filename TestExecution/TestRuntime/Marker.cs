using System.Diagnostics;
using System.Globalization;
using System.ServiceModel;
using R4nd0mApps.TddStud10.TestRuntime.Diagnostics;

namespace R4nd0mApps.TddStud10.TestRuntime
{
    public static class Marker
    {
        //private static ConcurrentDictionary<int, Lazy<List<string[]>>> store = new ConcurrentDictionary<int, Lazy<List<string[]>>>();

        //private static Lazy<ICodeCoverageServer> channel = new Lazy<ICodeCoverageServer>(CreateChannel);

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
        }

        public static void EnterSequencePoint(string mvid, string mdToken, string spid)
        {
            //var list = store.GetOrAdd(Thread.CurrentThread.ManagedThreadId, new Lazy<List<string[]>>(() => new List<string[]>())).Value;
            //list.Add(new[] { mvid, mdToken, spid });
        }

        private static ICodeCoverageServer CreateChannel()
        {
            // TODO: Remove this DRY violation
            string address = string.Format(
                "net.pipe://localhost/r4nd0mapps/tddstud10/CodeCoverageDataCollector/{0}",
                Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture));

            Logger.I.LogInfo("Marker: Initiating connection to {0} ...", address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(address);
            var ret = ChannelFactory<ICodeCoverageServer>.CreateChannel(binding, ep);
            ret.Ping();
            Logger.I.LogInfo("Marker: Connected to server.", address);

            return ret;
        }
    }
}
