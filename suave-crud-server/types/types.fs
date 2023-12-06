namespace SuaveAPI

[<AutoOpen>]
module Types =
    type Algorithm =
        | BetaSkeleton = 0
        | CombinationWithTime = 1
        | CombinationWithDistance = 2

    type Loc = { Lng: string; Lat: string }

    type Result<'TSuccess, 'TFailure> =
        | Success of 'TSuccess
        | Failure of 'TFailure

    type Leg =
        { steps: string
          distance: float
          duration: float
          summary: string
          weight: float }

    type Route =
        { legs: Leg[]
          distance: float
          duration: float
          weight_name: string
          weight: float }


    type Waypoint =
        { hint: string
          distance: float
          name: string
          location: int[] }

    type OsrmRes =
        { code: string
          routes: Route[]
          waypoints: Waypoint[] }

    // type OsrmReq =
    //     { waypoints: list<Loc>
    //       demands: list<Loc> }

    type OsrmReq =
        { waypoints: list<Loc>
          demands: list<Loc>
          algorithm: int }
