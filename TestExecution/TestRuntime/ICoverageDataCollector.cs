using System.Collections.Generic;
using System.ServiceModel;

namespace R4nd0mApps.TddStud10.TestRuntime
{
    [ServiceContract(Namespace = "https://www.tddstud10.r4nd0mapps.com")]
    public interface ICodeCoverageServer
    {
        [OperationContract]
        void Ping();

        [OperationContract]
        void ExitUnitTest(string source, string document, string line, List<string[]> seqencePoints);
    }
}
