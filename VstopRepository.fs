namespace SuaveAPI.VstopRepository

open SuaveAPI.PG

module VstopRepository =
    let getVstops () =
        getAllVstops connectionString :> seq<Vstops>
