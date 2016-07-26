using R4nd0mApps.TddStud10.Common.Domain;
using System;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public class HostIdeActions
    {
        public Action<DTestCase> GotoTest { get; set; }
        public Action<DTestCase> DebugTest { get; set; }
        public Action<DTestCase> RunTest { get; set; }
    }
}
