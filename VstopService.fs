namespace SuaveAPI.VstopService

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Successful

[<AutoOpen>]
module VstopService =
    open Suave.Filters
    type Actions<'t> = { ListVstops: unit -> seq<'t> }

    let JSON v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings) |> OK
        >=> Writers.setMimeType "application/json"

    let vstopHandle nameOfAction action =
        let listAll = action.ListVstops() |> JSON
        let actionPath = "/" + nameOfAction
        choose [ path actionPath >=> choose [ GET >=> listAll ] ]
