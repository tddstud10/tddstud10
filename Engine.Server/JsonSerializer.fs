namespace R4nd0mApps.TddStud10.Engine

module JsonSerializer = 
    open Newtonsoft.Json
    
    let private jsonConverters = [||]
    let writeJson (o : obj) = JsonConvert.SerializeObject(o, jsonConverters)
