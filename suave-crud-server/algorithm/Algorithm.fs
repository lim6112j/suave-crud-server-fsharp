namespace SuaveAPI

open System.Text.Json

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
module Algorithm =
    let insertDemandsBeweenWaypointsPair theta (wps: list<Loc>) (dmds: list<Loc>) input =
        input
        |> Seq.fold
            (fun acc pair ->
                match pair with
                | (x, y, i) when x > theta && y > theta -> outsertAt (i + 1) dmds[1] (outsertAt i dmds[0] acc)
                | (x, y, i) when x > theta -> outsertAt i dmds[0] acc
                // inserted item makes insertion point increased
                | (x, y, i) when y > theta -> outsertAt (i + acc.Length - wps.Length) dmds[1] acc
                | _ -> acc)

            wps
        |> function
            | x when x.Length = wps.Length + dmds.Length -> Success x
            | x -> Failure "demands insertion failed between waypoints accroding to theta"

    /// <summary>
    /// mobble beta-skeleton
    /// 2 vectors inner theta calculation (waypoints pair with demand point)
    ///
    /// </summary>
    let getOptimalWaypointsWithTheta theta (wps: list<Loc>) (dmds: list<Loc>) =
        traversePair wps
        |> Seq.mapi (fun i pair ->
            let thetaOrigin = getTheta pair dmds[0]
            let thetaDestination = getTheta pair dmds[1]
            (thetaOrigin, thetaDestination, i))

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

    type LocWithLabel =
        { Lat: string
          Lng: string
          Kind: string }

    /// <summary>
    /// vectorize demand and supply , compare vectors for determine dispatch
    /// TODO not working properly
    /// </summary>
    let getVectorizedWaypoints (wps: Loc list) (dmds: Loc list) =
        let labeledWps =
            wps
            |> Seq.map (fun w ->
                { Lat = w.Lat
                  Lng = w.Lng
                  Kind = "waypoints" })
            |> Seq.toList

        let labeledDmds =
            dmds
            |> Seq.map (fun d ->
                { Lat = d.Lat
                  Lng = d.Lng
                  Kind = "demands" })
            |> Seq.toList

        labeledWps @ labeledDmds
        |> Seq.sortBy (fun x -> x.Lat)
        |> printfn "getting vectorized waypoints %A\n"

    /// <summary>
    /// memoize algorithm execution
    /// </summary>
    let memoize f =
        let cache = System.Collections.Generic.Dictionary()

        fun (algo: Algorithm) (theta': float) (waypoints': list<Loc>) (demands': list<Loc>) ->
            let args =
                (waypoints' @ demands')
                |> List.map (fun r -> r.ToString())
                |> System.String.Concat
                |> fun r -> algo.ToString() + theta'.ToString() + r

            match cache.TryGetValue(args) with
            | true, v ->
                printfn "cache returns!!"
                v
            | _ ->
                let v = f algo theta' waypoints' demands'
                cache.Add(args, v)
                v

    /// <summary>
    /// select algorithm and run selected algorithm
    /// 0: BetaSkeleton, 1: CombinationWithTime, 2: CombinationWithDistance , default: CombinationWithTime
    /// add more algorithm with modifying types for Algorithm in types/types.fs and here
    /// </summary>
    let executeAlgorithm (algorithm: Algorithm) (theta': float) (waypoints': list<Loc>) (demands': list<Loc>) =
        match algorithm with
        | (Algorithm.BetaSkeleton) ->
            try
                getOptimalWaypointsWithTheta theta' waypoints' demands'
                |> insertDemandsBeweenWaypointsPair theta' waypoints' demands'
                |> bind getUrl
                |> bind getFromAsyncHttp
            with _ ->
                Failure "Beta skeleton waypoints optimzation failed"
        | (Algorithm.CombinationWithDistance) ->
            try
                getCombinationOfWaypoints waypoints' demands'
                |> Seq.map (fun r -> getUrl r)
                |> Seq.map (fun r -> getFromAsyncHttp r)
                |> Seq.map (fun r -> JsonSerializer.Deserialize<Response> r)
                |> Seq.minBy (fun r -> r.routes[0].distance)
                |> JsonSerializer.Serialize
                |> Success
            with _ ->
                Failure "combination waypoints calculation failed"
        | _ ->
            try
                getCombinationOfWaypoints waypoints' demands'
                |> Seq.map (fun r -> getUrl r)
                |> Seq.map (fun r -> getFromAsyncHttp r)
                |> Seq.map (fun r -> JsonSerializer.Deserialize<Response> r)
                |> Seq.minBy (fun r -> r.routes[0].duration)
                |> JsonSerializer.Serialize
                |> Success
            with _ ->
                Failure "combination waypoints calculation failed"
