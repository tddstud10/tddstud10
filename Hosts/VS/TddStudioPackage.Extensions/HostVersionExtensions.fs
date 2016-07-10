module R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.HostVersionExtensions

open R4nd0mApps.TddStud10.Common.Domain

let fromDteVersion = function
    | "12.0" -> VS2013
    | "14.0" -> VS2015
    | v -> failwithf "%s Unknown DTE Version" v
