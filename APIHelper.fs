namespace SuaveAPI

open FSharp.Configuration
open System.Net.Http
open System.IO

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Successful
open Suave.Operators

type Loc = { Lng: string; Lat: string }

type Idx = { I: int; J: int }
type YamlSettings = YamlConfig<"config.yaml">

[<AutoOpen>]
module APIHelper =

    let JSON v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings) |> OK
        >=> Writers.setMimeType "application/json"

    let apiCall () =
        async {
            use client = new HttpClient()

            let waypoints: list<Loc> =
                [ { Lng = "126.79939689052419"
                    Lat = "37.527319039426736" }
                  { Lng = "126.87491792408139"
                    Lat = "37.627320777159646" }
                  { Lng = "127.06568457951413"
                    Lat = "37.665543301893806" } ]

            let demands: list<Loc> =
                [ { Lng = "126.80939689052419"
                    Lat = "37.547319039426736" }
                  { Lng = "126.90491792408139"
                    Lat = "37.657320777159646" } ]

            let config = YamlSettings()
            let apiHead = config.OSRM.Server.Host.ToString()
            let apiTail = config.OSRM.Server.Tail

            let getUrl (wps: list<Loc>) =
                wps
                |> Seq.fold
                    (fun acc (waypoint: Loc) ->
                        match waypoint with
                        | { Lng = x; Lat = y } -> acc + x + "," + y + ";")
                    ""
                |> fun s -> s.Remove(s.Length - 1)
                |> fun s -> apiHead + s + apiTail

            let insertAt index newEl input =
                input
                |> List.mapi (fun i el -> if i = index then [ newEl; el ] else [ el ])
                |> List.concat

            let getCombinationOfWaypoints (wp: list<Loc>) =
                seq {
                    for i in 0 .. wp.Length - 1 do
                        for j in i + 1 .. wp.Length - 1 do
                            let result = insertAt i demands[0] wp |> insertAt j demands[1]
                            yield result
                }


            let responses =
                getCombinationOfWaypoints waypoints
                // |> Seq.map (fun x ->
                //     printfn "%A" x
                //     x)
                |> Seq.map (fun r -> getUrl (r))
                // |> Seq.map (fun x ->
                //     printfn "%A" x
                //     x)
                |> Seq.map (fun r -> client.GetStringAsync(r))
                |> Seq.map (fun r -> r.Result)

            return responses |> JSON
        }
        |> Async.RunSynchronously
