using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
{
    public interface IIdeHost
    {
        void GotoTest(DTestCase testCase);

        void RunTest(DTestCase testCase);

        void DebugTest(DTestCase testCase);
    }
}
