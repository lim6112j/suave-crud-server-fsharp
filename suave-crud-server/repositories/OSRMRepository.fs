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
    ///     ],
    ///     "algorithm": 1
    /// }
    /// </summary>
    let apiPostCall (param: OsrmReq) =

        let waypoints = param.waypoints
        let demands = param.demands
        let algorithmParam = param.algorithm
        let theta = 90 // TODO get from request


        async {
            let algorithmVar: Algorithm = enum algorithmParam
            let executeSelcetedAlgorithm = memoize executeAlgorithm
            let responses = executeSelcetedAlgorithm algorithmVar theta waypoints demands
            return responses
        }
        |> Async.RunSynchronously
