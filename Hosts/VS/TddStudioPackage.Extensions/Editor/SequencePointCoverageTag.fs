namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Tagging
open R4nd0mApps.TddStud10.Common.Domain

type SequencePointCoverageTag = 
    { testCase : TestRunId seq }
    interface ITag
