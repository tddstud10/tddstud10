module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor.TestCommon

open Microsoft.VisualStudio.TestPlatform.ObjectModel
open System
open R4nd0mApps.TddStud10.Common.Domain
open Xunit

type SimpleTestCase = 
    { fqn : string
      src : string
      file : string
      ln : int }
    
    member self.toTC() = 
        let t = TestCase(self.fqn, Uri("exec://utf"), self.src)
        t.CodeFilePath <- self.file
        t.LineNumber <- self.ln
        t
    
    member self.toTID() = 
        { source = FilePath self.src 
          location = { document = FilePath self.file
                       line = DocumentCoordinate self.ln } }
    
    static member fromTC (tc : TestCase) = 
        { fqn = tc.FullyQualifiedName
          src = tc.Source
          file = tc.CodeFilePath
          ln = tc.LineNumber }

type SimpleTestResult = 
    { name : string
      outcome : TestOutcome }
    
    member self.toTR tc = 
        let tr = TestResult(tc)
        tr.DisplayName <- self.name
        tr.Outcome <- self.outcome
        tr
    
    static member fromTR (tr : TestResult) = { name = tr.DisplayName; outcome = tr.Outcome }

type Assert with
    static member Equal<'T>((e : 'T), (ao : 'T option)) = 
        match ao with
        | None -> failwithf "actual is None" 
        | Some a -> Assert.Equal<'T>(e, a)
