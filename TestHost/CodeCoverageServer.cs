using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Serialization;
using R4nd0mApps.TddStud10.Engine;

namespace Server
{
    [ServiceContract(Namespace = "https://gorillacoding.wordpress.com")]
    public interface ICodeCoverageServer
    {
        [OperationContract]
        void EnterSequencePoint(string mvid, string mdToken, string spid, string executingUnitTest);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CodeCoverageServer : ICodeCoverageServer
    {
        private CoverageSession store = new CoverageSession();

        public void EnterSequencePoint(string mvid, string mdToken, string spid, string executingUnitTest)
        {
            if (!store.ContainsKey(mvid))
            {
                store[mvid] = new List<CoverageHitInfo>();
            }

            // TODO: Compress this data.
            store[mvid].Add(new CoverageHitInfo { Method = new MethodId { Mvid = mvid, MdToken = mdToken }, SpId = spid, UnitTest = executingUnitTest });
        }

        public void SaveTestCases(string codeCoverageStore)
        {
            StringWriter writer = new StringWriter();

            serializer.Serialize(writer, store);
            File.WriteAllText(codeCoverageStore, writer.ToString());
        }

        private static XmlSerializer serializer = new XmlSerializer(typeof(CoverageSession));
    }
}
