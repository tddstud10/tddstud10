module R4nd0mApps.TddStud10.Engine.TestFramework

type CallSpy<'T>() = 
    member val Called = false with get, set
    member val CalledWith = None with get, set
    member public t.Func(arg : 'T) : 'T = 
        t.Called <- true
        t.CalledWith <- Some arg
        arg

