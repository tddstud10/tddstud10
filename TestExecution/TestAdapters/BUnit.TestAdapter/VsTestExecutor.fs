namespace BUnit.TestAdapter

open Microsoft.VisualStudio.TestPlatform.ObjectModel
open Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter

type VsTestExecutor() = 
    let rt = Runtime.Class1()
    interface ITestExecutor with
        member __.Cancel() : unit = failwith "Not implemented yet"
        member __.RunTests(_ : System.Collections.Generic.IEnumerable<TestCase>, _ : IRunContext, _ : IFrameworkHandle) : unit = 
            failwith "Not implemented yet"
        member __.RunTests(_ : System.Collections.Generic.IEnumerable<string>, _ : IRunContext, _ : IFrameworkHandle) : unit = 
            failwith "Not implemented yet"
