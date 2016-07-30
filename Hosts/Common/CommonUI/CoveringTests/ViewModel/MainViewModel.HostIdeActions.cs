using Microsoft.FSharp.Core;
using R4nd0mApps.TddStud10.Common.Domain;
using System;
using System.Collections.Generic;

namespace R4nd0mApps.TddStud10.Hosts.Common.CoveringTests.ViewModel
{
    public class HostIdeActions
    {
        public FSharpFunc<Tuple<SequencePoint, DTestResult>, Unit> GotoTest { get; set; }
        public FSharpFunc<Tuple<SequencePoint, DTestResult>, Unit> DebugTest { get; set; }
        public FSharpFunc<Tuple<SequencePoint, DTestResult>, Unit> RunTest { get; set; }
        public FSharpFunc<Unit, IEnumerable<Tuple<SequencePoint, DTestResult>>> GetCoveringTestResults { get; set; }
        public FSharpFunc<Unit, bool> IdeInDebugMode { get; set; }
    }
}
