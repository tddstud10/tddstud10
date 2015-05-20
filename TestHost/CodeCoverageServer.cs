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
using R4nd0mApps.TddStud10.TestHost.Diagnostics;
using R4nd0mApps.TddStud10.Common.Domain;

namespace Server
{
    [ServiceContract(Namespace = "https://gorillacoding.wordpress.com")]
    public interface ICodeCoverageServer
    {
        [OperationContract]
        void Ping();

        [OperationContract]
        void EnterSequencePoint(string mvid, string mdToken, string spid, string source, string document, string line);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CodeCoverageServer : ICodeCoverageServer
    {
        private PerAssemblySequencePointsCoverage store = new PerAssemblySequencePointsCoverage();

        public void EnterSequencePoint(string mvid, string mdToken, string spid, string source, string document, string line)
        {
            var assemId = AssemblyId.NewAssemblyId(Guid.Parse(mvid));
            if (!store.ContainsKey(assemId))
            {
                store[assemId] = new List<SequencePointCoverage>();
            }

            store[assemId].Add(
                new SequencePointCoverage
                {
                    methodId = new MethodId
                    {
                        assemblyId = assemId,
                        mdTokenRid = MdTokenRid.NewMdTokenRid(uint.Parse(mdToken))
                    },
                    sequencePointId = SequencePointId.NewSequencePointId(int.Parse(spid)),
                    testId = new TestId
                    {
                        source = FilePath.NewFilePath(source),
                        document = FilePath.NewFilePath(document),
                        line = DocumentCoordinate.NewDocumentCoordinate(int.Parse(line))
                    }
                });
        }

        public void SaveTestCases(string codeCoverageStore)
        {
            store.Serialize(codeCoverageStore);
        }

        #region ICodeCoverageServer Members

        public void Ping()
        {
            Logger.I.LogInfo("ICodeCoverageServer - responding to ping.");
        }

        #endregion
    }
}
