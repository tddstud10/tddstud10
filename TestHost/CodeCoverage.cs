using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Server;

namespace R4nd0mApps.TddStud10.TestHost
{
    public static class Marker
    {
        private static ICodeCoverageServer channel;

        public static void EnterSequencePoint(string mvid, string mdToken, string spid)
        {
            if (channel == null)
            {
                string address = "net.pipe://localhost/gorillacoding/IPCTest";

                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                EndpointAddress ep = new EndpointAddress(address);
                channel = ChannelFactory<ICodeCoverageServer>.CreateChannel(binding, ep);
            }

            channel.EnterSequencePoint(mvid, mdToken, spid);
        }
    }
}
