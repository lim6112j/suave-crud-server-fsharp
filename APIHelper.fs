namespace SuaveAPI

open FSharp.Configuration
open System
open System.Net.Http
open System.IO

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Successful
open Suave.Operators
open System.Text.RegularExpressions
open System.Text.Json
open System.Collections.Generic
open SuaveAPI.Utils


type Idx = { I: int; J: int }

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
                    Lat = "37.665543301893806" }
                  { Lng = "127.16568457951413"
                    Lat = "37.565543301893806" } ]

            let demands: list<Loc> =
                [ { Lng = "126.80939689052419"
                    Lat = "37.547319039426736" }
                  { Lng = "126.90491792408139"
                    Lat = "37.657320777159646" } ]

            /// <summary>
            /// mobble beta-skeleton
            /// 2 vectors inner theta calculation (waypoints pair with demand point)
            /// </summary>
            let thetas (wps: list<Loc>) (dmds: list<Loc>) =
                traversePair wps
                |> Seq.map (fun pair ->
                    let thetaOrigin = getTheta pair dmds[0]
                    let thetaDestination = getTheta pair dmds[1]
                    (thetaOrigin, thetaDestination))

            printfn "%A" (thetas waypoints demands)

            /// <summary>
            /// get combinational waypoints list for cost calculation ( Mobble algorithm )
            /// number of waypoints = n+1 C r  (n : waypoints number, r : demands number)
            /// </summary>
            let getCombinationOfWaypoints (wp: list<Loc>) (dmds: list<Loc>) =
                seq {
                    for i in 0 .. wp.Length - 1 do
                        for j in i + 1 .. wp.Length do
                            let result = outsertAt i dmds[0] wp |> outsertAt j dmds[1]
                            yield result
                }

            let responses =
                getCombinationOfWaypoints waypoints demands
                |> Seq.map (fun r -> getUrl (r))
                |> Seq.map (fun r -> client.GetStringAsync(r))
                |> Seq.map (fun r -> r.Result)

            return responses |> JSON
        }
        |> Async.RunSynchronously
