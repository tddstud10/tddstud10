module R4nd0mApps.TddStud10.Engine.Core.PathBuilderTests

open R4nd0mApps.TddStud10.Common.Domain
open Xunit
open System
open R4nd0mApps.TddStud10.Common

let inline (~~) s = FilePath s

[<Theory>]
[<InlineData("c:\\folder\\file.sln", "d:\\tddstud10\\folder\\file.sln")>]
[<InlineData("x:\\file.sln", "d:\\tddstud10\\file\\file.sln")>]
let ``Tests for makeSlnSnapshotPath`` (slnPath, snapShotPath) = 
    let (FilePath sp) = PathBuilder.makeSlnSnapshotPath (FilePath slnPath)
    Assert.Equal(snapShotPath, sp)

[<Theory>]
[<InlineData("c:\\folder\\file.sln", "d:\\tddstud10\\folder\\out")>]
[<InlineData("x:\\file.sln", "d:\\tddstud10\\file\\out")>]
let ``Tests for makeSlnBuildRoot`` (slnPath, buildRoot) = 
    let (FilePath sp) = PathBuilder.makeSlnBuildRoot (FilePath slnPath)
    Assert.Equal(buildRoot, sp)

[<Theory>]
[<InlineData(@"c:\sln\sln.sln", @"d:\tddstud10\sln\sln.sln", @"d:\tddstud10\sln\proj\a.cpp", @"c:\sln\proj\a.cpp")>]
[<InlineData(@"c:\sln\sln.sln", @"d:\tddstud10\sln\sln.sln", @"d:\tddstud10x\sln\proj\a.cpp", @"d:\tddstud10x\sln\proj\a.cpp")>]
// NOTE: We dont support cases where the sln file is at drive level.
let ``Tests for rebaseCodeFilePath`` (slnPath, slnSnapPath, inp, outp) =
    let rsp = { startTime = DateTime.Now
                testHostPath = ~~""
                solutionPath = ~~slnPath
                solutionSnapshotPath = ~~slnSnapPath
                solutionBuildRoot = ~~"" }
    let p = PathBuilder.rebaseCodeFilePath (rsp.solutionPath, rsp.solutionSnapshotPath) ~~inp
    Assert.Equal(~~outp, p)
