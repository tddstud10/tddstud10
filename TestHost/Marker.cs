using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using R4nd0mApps.TddStud10.Engine;
using Server;

namespace R4nd0mApps.TddStud10.TestHost
{
    public static class Marker
    {
        private static object syncObject = new Object();

        [ThreadStatic]
        private static string executingUnitTest;

        private static ICodeCoverageServer channel;

        public static void EnterUnitTest(string unitTestName)
        {
            executingUnitTest = unitTestName;
        }

        public static void EnterSequencePoint(string mvid, string mdToken, string spid)
        {
            lock (syncObject)
            {
                if (channel == null)
                {
                    CreateChannel();
                }
            }

            channel.EnterSequencePoint(mvid, mdToken, spid, executingUnitTest);
        }

        private static void CreateChannel()
        {
            string address = "net.pipe://localhost/gorillacoding/IPCTest";

            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(address);
            channel = ChannelFactory<ICodeCoverageServer>.CreateChannel(binding, ep);
        }
    }
}
