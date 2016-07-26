using R4nd0mApps.TddStud10.Common.Domain;
using System.Windows;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests
{
    public class NullIdeHost : IIdeHost
    {
        public void GotoTest(DTestCase testCase)
        {
            MessageBox.Show(string.Format("Goto test... {0}", testCase.DisplayName));
        }

        public void DebugTest(DTestCase testCase)
        {
            MessageBox.Show(string.Format("Start with debugging... {0}", testCase.DisplayName));
        }

        public void RunTest(DTestCase testCase)
        {
            MessageBox.Show(string.Format("Start without debugging... {0}", testCase.DisplayName));
        }
    }
}
