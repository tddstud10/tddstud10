using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Server;

namespace Client
{
    class Client
    {
        static void MainX(string[] args)
        {
            string address = "net.pipe://localhost/gorillacoding/IPCTest";

            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(address);
            ICodeCoverageServer channel = ChannelFactory<ICodeCoverageServer>.CreateChannel(binding, ep);

            Console.WriteLine("Client Connected");

            //Console.WriteLine(" 2 + 2 = {0}", channel.EnterSequencePoint(2, 2));

            Console.ReadLine();
        }
    }
}
