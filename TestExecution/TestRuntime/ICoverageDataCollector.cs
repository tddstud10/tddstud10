using System.Collections.Generic;
using System.ServiceModel;

namespace R4nd0mApps.TddStud10.TestRuntime
{
    [ServiceContract(Namespace = "https://www.tddstud10.r4nd0mapps.com")]
    public interface ICoverageDataCollector
    {
        [OperationContract]
        void Ping();

        [OperationContract]
        void EnterSequencePoint(string testRunId, string assemblyId, string methodMdRid, string spId);

        [OperationContract]
        void ExitUnitTest(string testRunId, string source, string document, string line);
    }
}
