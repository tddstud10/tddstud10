namespace FsAutoComplete

module JsonSerializer = 
    open Newtonsoft.Json
    
    let private jsonConverters = [||]
    let writeJson (o : obj) = JsonConvert.SerializeObject(o, jsonConverters)
