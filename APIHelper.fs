namespace SuaveAPI

open FSharp.Configuration
open System.Net.Http

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Successful
open Suave.Operators
open SuaveAPI.Utils
open SuaveAPI.Types


type Idx = { I: int; J: int }

[<AutoOpen>]
module APIHelper =

    let JSON v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings) |> OK
        >=> Writers.setMimeType "application/json"

    let apiCall waypoints demands =
        async {
            use client = new HttpClient()


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
