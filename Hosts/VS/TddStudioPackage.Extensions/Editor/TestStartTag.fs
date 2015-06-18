namespace R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor

open Microsoft.VisualStudio.Text.Tagging
open Microsoft.VisualStudio.TestPlatform.ObjectModel

type TestStartTag = 
    { testCase : TestCase
      textHash : int }
    interface ITag
