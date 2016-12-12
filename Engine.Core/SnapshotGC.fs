namespace R4nd0mApps.TddStud10.Engine.Core

open R4nd0mApps.TddStud10.Common.Domain
open System
open System.IO

module SnapshotGC = 
    let private markerFile = "__tddstud10.gc.marker__"
    let private stalingTime = TimeSpan.FromDays(1.0)
    
    let mark (FilePath sssr) = 
        let marker = Path.Combine(sssr, markerFile)
        File.WriteAllText(marker, "")
        FileInfo(marker).LastWriteTimeUtc <- DateTime.UtcNow.Subtract(stalingTime)
    
    let unmark (FilePath sssr) = File.WriteAllText(Path.Combine(sssr, markerFile), "")
    
    let collect (FilePath ssr) = 
        async { 
            let isStale d = 
                let marker = Path.Combine(d, markerFile)
                not <| File.Exists(marker) || (DateTime.UtcNow - FileInfo(marker).LastWriteTimeUtc >= stalingTime)
            
            let garbage = 
                Directory.EnumerateDirectories(ssr, "*", SearchOption.TopDirectoryOnly)
                |> Seq.filter isStale
                |> Seq.toList
            
            garbage |> Seq.iter (fun d -> Common.safeExec (fun () -> Directory.Delete(d, true)))
            return garbage
        }
