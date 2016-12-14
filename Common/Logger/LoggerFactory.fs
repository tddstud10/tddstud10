module R4nd0mApps.TddStud10.Logger.LoggerFactory

let logger : ILogger = XFactory.X "R4nd0mApps.TddStud10.Logger.WindowsLogger" (NullLogger() :> _)
