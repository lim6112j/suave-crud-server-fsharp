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
