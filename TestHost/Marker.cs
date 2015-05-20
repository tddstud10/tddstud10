using System;
using System.Diagnostics;
using System.ServiceModel;
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using Server;

namespace R4nd0mApps.TddStud10.TestHost
{
    public static class Marker
    {
        private static object syncObject = new Object();

        [ThreadStatic]
        private static string source;
        [ThreadStatic]
        private static string document;
        [ThreadStatic]
        private static string line;

        private static ICodeCoverageServer channel;

        public static void EnterUnitTest(string source, string document, string line)
        {
            Marker.source = source;
            Marker.document = document;
            Marker.line = line;
        }

        public static void EnterSequencePoint(string mvid, string mdToken, string spid)
        {
            lock (syncObject)
            {
                if (channel == null)
                {
                    channel = CreateChannel();
                }
            }

            channel.EnterSequencePoint(mvid, mdToken, spid, source, document, line);
        }

        private static ICodeCoverageServer CreateChannel()
        {
            string address = string.Format(
                "net.pipe://localhost/gorillacoding/IPCTest/{0}",
                Process.GetCurrentProcess().Id.ToString());

            //Logger.I.LogInfo("ICodeCoverageClient: Initiating connection to {0} ...", address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(address);
            var ret = ChannelFactory<ICodeCoverageServer>.CreateChannel(binding, ep);
            ret.Ping();
            //Logger.I.LogInfo("ICodeCoverageClient: Connected to server.", address);

            return ret;
        }
    }
}
