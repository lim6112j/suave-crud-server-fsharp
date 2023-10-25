namespace SuaveAPI.APIService

open Suave
open Suave.Operators
open SuaveAPI.Types

[<AutoOpen>]
module APIService =
    open Suave.Filters

    type Actions<'t> =
        { OSRMResponse: list<Loc> -> list<Loc> -> WebPart }

    let waypoints: list<Loc> =
        [ { Lng = "126.79939689052419"
            Lat = "37.527319039426736" }
          { Lng = "126.87491792408139"
            Lat = "37.627320777159646" }
          { Lng = "127.06568457951413"
            Lat = "37.665543301893806" }
          { Lng = "127.16568457951413"
            Lat = "37.565543301893806" } ]

    let demands: list<Loc> =
        [ { Lng = "126.80939689052419"
            Lat = "37.547319039426736" }
          { Lng = "126.90491792408139"
            Lat = "37.657320777159646" } ]

    let osrmHandle nameOfAction action =
        let getOSRM = action.OSRMResponse waypoints demands
        let actionPath = "/" + nameOfAction
        choose [ path actionPath >=> choose [ GET >=> getOSRM ] ]
