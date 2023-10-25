namespace SuaveAPI

open Suave
open Suave.Operators
open SuaveAPI.Types

[<AutoOpen>]
module OSRMService =
    open Suave.Filters

    type Actions<'t> = { postOSRM: 't -> WebPart }

    let osrmHandle nameOfAction action =
        let postOSRM param = action.postOSRM
        let actionPath = "/" + nameOfAction

        choose
            [ path actionPath
              >=> choose [ POST >=> request (getActionData >> action.postOSRM >> JSON) ] ]
