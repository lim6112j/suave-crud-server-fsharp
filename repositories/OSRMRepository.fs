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
module OSRMRepository =

    let JSON v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings) |> OK
        >=> Writers.setMimeType "application/json"


    /// <summary>
    /// request body
    ///     {
    ///     "waypoints": [
    ///           { "Lng" : "126.79939689052419", "Lat" : "37.527319039426736" },
    ///           { "Lng" : "126.87491792408139","Lat" : "37.627320777159646" },
    ///           { "Lng" : "127.06568457951413","Lat" : "37.665543301893806" },
    ///           { "Lng" : "127.16568457951413", "Lat" : "37.565543301893806" }
    ///     ],
    ///     "demands": [
    ///           { "Lng" : "126.80939689052419", "Lat" : "37.547319039426736" },
    ///           { "Lng" : "126.90491792408139", "Lat" : "37.657320777159646" }
    ///     ]
    /// }
    /// </summary>
    let apiPostCall (param: OsrmReq) =

        let waypoints = param.waypoints
        let demands = param.demands

        async {
            use client = new HttpClient()


            /// <summary>
            /// mobble beta-skeleton
            /// 2 vectors inner theta calculation (waypoints pair with demand point)
            ///
            /// </summary>
            let getOptimalWaypointsWithTheta (wps: list<Loc>) (dmds: list<Loc>) =
                traversePair wps
                |> Seq.mapi (fun i pair ->
                    let thetaOrigin = getTheta pair dmds[0]
                    let thetaDestination = getTheta pair dmds[1]
                    (thetaOrigin, thetaDestination, i))
                |> insertDemandsBeweenWaypointsPair 90 wps dmds
                |> validateWaypointsWithDemands wps dmds

            printfn "%A" (getOptimalWaypointsWithTheta waypoints demands)

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

            // let responses =
            //     getCombinationOfWaypoints waypoints demands
            //     |> Seq.map (fun r -> getUrl (r))
            //     |> Seq.map (fun r -> client.GetStringAsync(r))
            //     |> Seq.map (fun r -> r.Result)

            let responses =
                getOptimalWaypointsWithTheta waypoints demands
                |> bind getUrl
                |> bind getFromAsyncHttp
                |> unwrap
                |> fun s ->
                    printfn "%A" s
                    s

            return responses |> JSON
        }
        |> Async.RunSynchronously
