using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Collections.Concurrent;
using R4nd0mApps.TddStud10;
using System.IO;
using System.Xml.Serialization;

namespace Server
{
    [ServiceContract(Namespace = "https://gorillacoding.wordpress.com")]
    public interface ICodeCoverageServer
    {
        [OperationContract]
        void EnterSequencePoint(string mvid, string mdToken, string spid);
    }

    public class CoverageHitInfo
    {
        public string Mvid { get; set; }
        public string MdToken { get; set; }
        public string SpId { get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CodeCoverageServer : ICodeCoverageServer
    {
        private SerializableDictionary<string, List<CoverageHitInfo>> store = new SerializableDictionary<string, List<CoverageHitInfo>>();

        public CodeCoverageServer()
        {
        }

        public void EnterSequencePoint(string mvid, string mdToken, string spid)
        {
            if (!store.ContainsKey(mvid))
            {
                store[mvid] = new List<CoverageHitInfo>();
            }

            store[mvid].Add(new CoverageHitInfo { Mvid = mvid, MdToken = mdToken, SpId = spid });
        }

        public void SaveTestCases(string codeCoverageStore)
        {
            StringWriter writer = new StringWriter();

            serializer.Serialize(writer, store);
            File.WriteAllText(codeCoverageStore, writer.ToString());
        }

        private static XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, List<CoverageHitInfo>>));
    }

    class Server
    {
        static void MainX(string[] args)
        {
            using (ServiceHost serviceHost = new ServiceHost(typeof(CodeCoverageServer)))
            {
                string address = "net.pipe://localhost/gorillacoding/IPCTest";
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                serviceHost.AddServiceEndpoint(typeof(ICodeCoverageServer), binding, address);
                serviceHost.Open();

                Console.WriteLine("ServiceHost running. Press Return to Exit");
                Console.ReadLine();
            }
        }
    }
}
