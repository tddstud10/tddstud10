using Microsoft.FSharp.Core;
using R4nd0mApps.TddStud10.Common.Domain;
using System;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public class HostIdeActions
    {
        public FSharpFunc<Tuple<SequencePoint, DTestCase>, Unit> GotoTest { get; set; }
        public FSharpFunc<Tuple<SequencePoint, DTestCase>, Unit> DebugTest { get; set; }
        public FSharpFunc<Tuple<SequencePoint, DTestCase>, Unit> RunTest { get; set; }
        public FSharpFunc<Unit, bool> IdeInDebugMode { get; set; }
    }
}
