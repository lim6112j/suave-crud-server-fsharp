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


            let config = YamlSettings()
            let apiHead = config.OSRM.Server.Host.ToString()
            let apiTail = config.OSRM.Server.Tail

            let url =
                waypoints
                |> Seq.fold
                    (fun acc (waypoint: Loc) ->
                        match waypoint with
                        | { Lng = x; Lat = y } -> acc + x + "," + y + ";")
                    ""
                |> fun s -> s.Remove(s.Length - 1)
                |> fun s -> apiHead + s + apiTail

            // printfn "%s" url

            let! response = client.GetStringAsync(url)

            do! File.WriteAllTextAsync("./response.html", response)
            printfn "%s" response
            return response |> JSON
        }
        |> Async.RunSynchronously
