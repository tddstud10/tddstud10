using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;

namespace R4nd0mApps.TddStud10.TestRuntime
{
    public class Marker
    {
        private const string TESTRUNID_SLOTNAME = "Marker.TestRunId";

        private static LazyObject<Marker> instance = new LazyObject<Marker>(
            () => new Marker(CreateChannel, Debugger.IsAttached, CallContext.LogicalGetData, CallContext.LogicalSetData));

        private LazyObject<ICoverageDataCollector> _channel;
        private bool _isDebuggerAttached;
        private Func<string, object> _ccGetData;
        private Action<string, object> _ccSetData;

        private string TestRunId
        {
            get { return _ccGetData(TESTRUNID_SLOTNAME) as string; }
            set { _ccSetData(TESTRUNID_SLOTNAME, value); }
        }

        public Marker(Func<ICoverageDataCollector> channelCreator, bool isDebuggerAttached, Func<string, object> ccGetData, Action<string, object> ccSetData)
        {
            _channel = new LazyObject<ICoverageDataCollector>(channelCreator);
            _isDebuggerAttached = isDebuggerAttached;
            _ccGetData = ccGetData;
            _ccSetData = ccSetData;
        }

        public void RegisterEnterSequencePoint(string assemblyId, string methodMdRid, string spId)
        {
            if (_isDebuggerAttached)
            {
                Trace.TraceInformation("Marker: Ignoring call as debugger is attached.");
                return;
            }

            if (TestRunId == null)
            {
                TestRunId = new object().GetHashCode().ToString(CultureInfo.InvariantCulture);
            }

            _channel.Value.EnterSequencePoint(TestRunId, assemblyId, methodMdRid, spId);
        }

        public void RegisterExitUnitTest(string source, string document, string line)
        {
            if (_isDebuggerAttached)
            {
                Trace.TraceInformation("Marker: Ignoring call as debugger is attached.");
                return;
            }

            if (TestRunId == null)
            {
                Trace.TraceError("Marker: Appears we did not have any sequence points for {0},{1},{2}.", source, document, line);
            }

            _channel.Value.ExitUnitTest(TestRunId, source, document, line);
            TestRunId = null;
        }

        [DebuggerNonUserCode]
        public static void EnterSequencePoint(string assemblyId, string methodMdRid, string spId)
        {
            Marker.instance.Value.RegisterEnterSequencePoint(assemblyId, methodMdRid, spId);
        }

        [DebuggerNonUserCode]
        public static void ExitUnitTest(string source, string document, string line)
        {
            Marker.instance.Value.RegisterExitUnitTest(source, document, line);
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

            Trace.TraceInformation("Marker: Initiating connection to {0} ...", address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress epa = new EndpointAddress(address);
            var ret = ChannelFactory<ICoverageDataCollector>.CreateChannel(binding, epa);
            ret.Ping();
            Trace.TraceInformation("Marker: Connected to server.", address);

            return ret;
        }
    }
}
