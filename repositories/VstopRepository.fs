namespace SuaveAPI.VstopRepository

open SuaveAPI.PostgresCon

module VstopRepository =
    let getVstops () =
        getAllVstops connectionString :> seq<Vstops>
