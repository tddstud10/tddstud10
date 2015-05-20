using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using Server;

namespace R4nd0mApps.TddStud10.TestHost
{
    public static class Marker
    {
        private static object syncObject = new Object();

        public static string Source
        {
            get { return CallContext.LogicalGetData("Marker.Source") as string; }
            set { CallContext.LogicalSetData("Marker.Source", value); }
        }

        public static string Document
        {
            get { return CallContext.LogicalGetData("Marker.Document") as string; }
            set { CallContext.LogicalSetData("Marker.Document", value); }
        }

        public static string Line
        {
            get { return CallContext.LogicalGetData("Marker.Line") as string; }
            set { CallContext.LogicalSetData("Marker.Line", value); }
        }
        

        private static ICodeCoverageServer channel;

        public static void EnterUnitTest(string source, string document, string line)
        {
            Source = source;
            Document = document;
            Line = line;
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

            channel.EnterSequencePoint(mvid, mdToken, spid, Source, Document, Line);
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
