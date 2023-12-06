namespace SuaveAPI

open Suave
open Suave.Operators
open SuaveAPI.Types

[<AutoOpen>]
module OSRMService =
    open Suave.Filters
    open Suave.RequestErrors

    type Actions<'t> =
        { postOSRM: 't -> Result<string, string> }

    let osrmHandle nameOfAction action =
        let badRequest = BAD_REQUEST "Oops, something went wrong here!"

        let handleAction reqError =
            function
            | Success r -> r |> JSON
            | Failure _ -> reqError

        let postOSRM param = action.postOSRM
        let actionPath = "/" + nameOfAction

        choose
            [ path actionPath
              >=> choose [ POST >=> request (getActionData >> action.postOSRM >> handleAction badRequest) ] ]
