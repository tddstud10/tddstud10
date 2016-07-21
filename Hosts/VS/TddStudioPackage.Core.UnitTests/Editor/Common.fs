module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Core.Editor.TestCommon

open System
open R4nd0mApps.TddStud10.Common.Domain
open Xunit

type SimpleTestCase = 
    { fqn : string
      src : FilePath
      file : FilePath
      ln : DocumentCoordinate }
    
    member self.toTC() = 
        { FullyQualifiedName = self.fqn; DisplayName = ""; Source = self.src; CodeFilePath = self.file; LineNumber = self.ln }
    
    member self.toTID() = 
        { source = self.src 
          location = { document = self.file
                       line = self.ln } }
    
    static member fromTC (tc : DTestCase) = 
        { fqn = tc.FullyQualifiedName
          src = tc.Source
          file = tc.CodeFilePath
          ln = tc.LineNumber }

type SimpleTestResult = 
    { name : string
      outcome : DTestOutcome }
    
    member self.toTR tc = 
        { DisplayName = self.name; TestCase = tc; Outcome = self.outcome; ErrorMessage = ""; ErrorStackTrace = "" }
    
    static member fromTR (tr : DTestResult) = { name = tr.DisplayName; outcome = tr.Outcome }

type Assert with
    static member Equal<'T>((e : 'T), (ao : 'T option)) = 
        match ao with
        | None -> failwithf "actual is None" 
        | Some a -> Assert.Equal<'T>(e, a)
