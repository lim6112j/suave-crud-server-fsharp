namespace SuaveAPI

[<AutoOpen>]
module Algorithm =
    let insertDemandsBeweenWaypointsPair theta (wps: list<Loc>) (dmds: list<Loc>) input =
        input
        |> Seq.fold
            (fun acc pair ->
                match pair with
                | (x, y, i) when x > theta -> outsertAt i dmds[0] acc
                // inserted item makes insertion point increased
                | (x, y, i) when y > theta -> outsertAt (i + acc.Length - wps.Length) dmds[1] acc
                | _ -> acc)

            wps

    let validateWaypointsWithDemands (wps: Loc list) (dmds: Loc list) input =
        input
        |> fun (waypoint: Loc list) ->
            let len = wps.Length + dmds.Length

            match waypoint with
            | x when x.Length = len -> Success waypoint
            | x -> Failure x
