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
open System.Text.Json


type Idx = { I: int; J: int }

type Response =
    { code: string
      routes: Route list
      waypoints: Waypoint list }

and Route =
    { leg: Leg list
      distance: float32
      duration: float32
      weight_name: string
      weight: float32 }

and Leg = { duration: float32 }

and Waypoint =
    { hint: string
      distance: float32
      name: string
      location: float32[] }

[<AutoOpen>]
module OSRMRepository =

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
        let algorithmParam = param.algorithm
        let theta = 90 // TODO get from request


        async {
            use client = new HttpClient()
            //
            let funcForAlgorithm algorithm theta' waypoints' demands' =
                match algorithm with
                | (Algorithm.BetaSkeleton) ->
                    getOptimalWaypointsWithTheta theta' waypoints' demands'
                    |> insertDemandsBeweenWaypointsPair theta' waypoints' demands'
                    |> bind getUrl
                    |> bind getFromAsyncHttp
                | _ ->
                    getCombinationOfWaypoints waypoints' demands'
                    |> Seq.map (fun r -> getUrl (r))
                    |> Seq.map (fun r -> getFromAsyncHttp (r))
                    |> Seq.map (fun r -> JsonSerializer.Deserialize<Response> r)
                    |> Seq.minBy (fun r -> r.routes[0].duration)
                    |> JsonSerializer.Serialize
                    |> Success

            // let responses =
            //     getCombinationOfWaypoints waypoints demands
            //     |> Seq.map (fun r -> getUrl (r))
            //     |> Seq.map (fun r -> client.GetStringAsync(r))
            //     |> Seq.map (fun r -> r.Result)

            // let testres = getVectorizedWaypoints waypoints demands

            // let responses =
            //     getOptimalWaypointsWithTheta theta waypoints demands
            //     |> insertDemandsBeweenWaypointsPair theta waypoints demands
            //     |> bind getUrl
            //     |> bind getFromAsyncHttp
            let algorithmVar: Algorithm = enum algorithmParam
            let responses = funcForAlgorithm algorithmVar theta waypoints demands
            return responses
        }
        |> Async.RunSynchronously
