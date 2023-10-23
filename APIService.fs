namespace SuaveAPI.APIService

open Suave
open Suave.Operators

[<AutoOpen>]
module APIService =
    open Suave.Filters
    type Actions<'t> = { OSRMResponse: unit -> WebPart }


    let osrmHandle nameOfAction action =
        let getOSRM = action.OSRMResponse()
        let actionPath = "/" + nameOfAction
        choose [ path actionPath >=> choose [ GET >=> getOSRM ] ]
