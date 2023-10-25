namespace SuaveAPI

open System
open FSharp.Configuration
open System.Collections.Generic
open System.Net.Http
open SuaveAPI.Types
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Successful

type YamlSettings = YamlConfig<"config.yaml">

[<AutoOpen>]
module Utils =

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

    /// <summary>
    /// insert item behind of index, named as outersert not insert
    /// [el: newEl] => behind of index
    /// [newEl; el] => fore of index
    /// </summary>
    let outsertAt index newEl input =
        input
        |> List.mapi (fun i el -> if i = index then [ el; newEl ] else [ el ])
        |> List.concat

    ///
    /// <summary>
    /// [1,2,3,4] => [(1,2), (2,3), (3,4)]
    /// vector계산을 위해 좌표를 페어로 만드는데 사용
    /// </summary>
    let rec traversePair (wps: list<Loc>) =
        match wps with
        | x :: y :: tail -> (x, y) :: traversePair (y :: tail)
        | _ -> []

    // printfn "%A" (traversePair waypoints)
    /// <summary>
    /// waypoints start, end and new point as origin
    /// calculate vector theta
    /// </summary>
    let getTheta pointsPair origin =
        let origin = (float origin.Lat, float origin.Lng)
        let fstPoint = (float (fst pointsPair).Lat, float (fst pointsPair).Lng)
        let sndPoint = (float (snd pointsPair).Lat, float (snd pointsPair).Lng)
        let fstVectorOrigin = (fst fstPoint - fst origin, snd fstPoint - snd origin)
        let sndVectorOrigin = (fst sndPoint - fst origin, snd sndPoint - snd origin)


        let thetaOrigin =
            acos (
                ((fst fstVectorOrigin) * (fst sndVectorOrigin)
                 + snd fstVectorOrigin * snd sndVectorOrigin)
                / (sqrt (
                    fst fstVectorOrigin * fst fstVectorOrigin
                    + snd fstVectorOrigin * snd fstVectorOrigin
                   )
                   * sqrt (
                       fst sndVectorOrigin * fst sndVectorOrigin
                       + snd sndVectorOrigin * snd sndVectorOrigin
                   ))
            )

        thetaOrigin * 180.0 / Math.PI

    let getFromAsyncHttp (query: string) =
        use client = new HttpClient()
        let result = client.GetStringAsync(query).Result
        result

    let bind func optionInput =
        match optionInput with
        | Success x -> func x
        | Failure f -> f

    let getUTF8 (str: byte[]) =
        System.Text.Encoding.UTF8.GetString(str)

    let jsonToObject<'t> json =
        JsonConvert.DeserializeObject(json, typeof<'t>) :?> 't

    let getActionData<'t> (req: HttpRequest) =
        req.rawForm
        |> getUTF8
        |> jsonToObject<'t>
        |> fun s ->
            printfn "%A" s
            s
