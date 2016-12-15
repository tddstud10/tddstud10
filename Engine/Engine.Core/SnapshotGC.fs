namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.Common.Domain
open System
open System.IO

module SnapshotGC = 
    let logger = R4nd0mApps.TddStud10.Logger.LoggerFactory.logger
    let tc = R4nd0mApps.TddStud10.Logger.TelemetryClientFactory.telemetryClient

    let private markerFile = "__gc_marker__"
    let private stalingTime = TimeSpan.FromDays(1.0)
    
    let unmark (FilePath sssr) = 
        let marker = Path.Combine(sssr, markerFile)
        File.WriteAllText(marker, "")
        FileInfo(marker).LastWriteTimeUtc <- DateTime.UtcNow.Subtract(stalingTime)
    
    let mark (FilePath sssr) = File.WriteAllText(Path.Combine(sssr, markerFile), "")
    
    let sweep (FilePath ssr) = 
        async { 
            logger.logInfof "SnapShotGC: Starting sweep"
            let isStale d = 
                let marker = Path.Combine(d, markerFile)
                not <| File.Exists(marker) || (DateTime.UtcNow - FileInfo(marker).LastWriteTimeUtc >= stalingTime)
            
            let garbage = 
                Directory.EnumerateDirectories(ssr, "*", SearchOption.TopDirectoryOnly)
                |> Seq.filter isStale
                |> Seq.toList

            logger.logInfof "SnapshotGC: detected %d snapshots to be GCed. Starting to delete them now." garbage.Length
            tc.TrackEvent("SnapshotGC", dict[], dict["SnapshotsDeleted", (float)garbage.Length])

            garbage |> Seq.iter (fun d -> Common.safeExec (fun () -> Directory.Delete(d, true)))
            return garbage
        }

    let SweepAsync ssr = ssr |> sweep |> Async.StartAsTask