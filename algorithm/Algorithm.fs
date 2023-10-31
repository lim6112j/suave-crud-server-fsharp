namespace SuaveAPI

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
            | x -> Failure x

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
        |> Seq.map (fun pair ->
            printfn "%A" pair
            pair)
        |> insertDemandsBeweenWaypointsPair theta wps dmds

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

    /// <summary>
    /// vectorize demand and supply , compare vectors for determine dispatch
    /// TODO not working properly
    /// </summary>
    let getVectorizedWaypoints (wps: Loc list) (dmds: Loc list) =
        wps
        |> Seq.map (fun el ->
            match el with
            | el when float (el.Lat) < float (dmds[0].Lat) -> [ el ]
            | el when float (el.Lat) >= float (dmds[0].Lat) -> [ dmds[0]; el ]
            | el when float (el.Lat) < float (dmds[1].Lat) -> [ el ]
            | el when float (el.Lat) >= float (dmds[1].Lat) -> [ dmds[1]; el ]
            | _ -> [])
        |> Seq.concat
        |> printfn "getting vectorized waypoints %A\n"
