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
                    Lat = "37.665543301893806" }
                  { Lng = "127.16568457951413"
                    Lat = "37.565543301893806" } ]

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
            /// mobble beta-skeleton
            /// 2 vectors inner theta calculation (waypoints pair with demand point)
            /// </summary>
            let thetas (wps: list<Loc>) (dmds: list<Loc>) =
                traversePair wps
                |> Seq.map (fun pair ->
                    let origin = (float dmds[0].Lat, float dmds[0].Lng)
                    let destination = (float dmds[1].Lat, float dmds[1].Lng)
                    let fstPoint = (float (fst pair).Lat, float (fst pair).Lng)
                    let sndPoint = (float (snd pair).Lat, float (snd pair).Lng)
                    let fstVectorOrigin = (fst fstPoint - fst origin, snd fstPoint - snd origin)
                    let sndVectorOrigin = (fst sndPoint - fst origin, snd sndPoint - snd origin)

                    let fstVectorDestination =
                        (fst fstPoint - fst destination, snd fstPoint - snd destination)

                    let sndVectorDestination =
                        (fst sndPoint - fst destination, snd sndPoint - snd destination)

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

                    let thetaDestination =
                        acos (
                            ((fst fstVectorDestination) * (fst sndVectorDestination)
                             + snd fstVectorDestination * snd sndVectorDestination)
                            / (sqrt (
                                fst fstVectorDestination * fst fstVectorDestination
                                + snd fstVectorDestination * snd fstVectorDestination
                               )
                               * sqrt (
                                   fst sndVectorDestination * fst sndVectorDestination
                                   + snd sndVectorDestination * snd sndVectorDestination
                               ))
                        )

                    (thetaOrigin * 180.0 / Math.PI, thetaDestination * 180.0 / Math.PI))

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

            let (|Regex|_|) pattern input =
                let m = Regex.Match(input, pattern)

                if m.Success then
                    Some(List.tail [ for g in m.Groups -> g.Value ])
                else
                    None

            let phone = "(444) 555-5555"

            match phone with
            | Regex @"\(([0-9]{3})\)[-. ]?([0-9]{3})[-. ]?([0-9]{4})" [ area; prefix; suffix ] ->
                printfn "Area: %s, Prefix: %s, Suffix: %s" area prefix suffix
            | _ -> printfn "Not a phone number"

            let responses =
                getCombinationOfWaypoints waypoints demands
                |> Seq.map (fun r -> getUrl (r))
                |> Seq.map (fun r -> client.GetStringAsync(r))
                |> Seq.map (fun r -> r.Result)
                |> Seq.map (fun r -> r.Trim())
                |> JSON

            return responses
        }
        |> Async.RunSynchronously
