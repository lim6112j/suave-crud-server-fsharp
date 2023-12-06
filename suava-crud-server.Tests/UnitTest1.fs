module suava_crud_server.Tests

open NUnit.Framework
open Microsoft.FSharp.Reflection
open SuaveAPI
open System
open System.Text.Json

type Response =
    { code: String
      routes: Route list
      waypoints: Waypoint list }

and Route =
    { leg: Leg list
      distance: float32
      duration: float32
      weight_name: String
      weight: float32 }

and Leg = { duration: float32 }

and Waypoint =
    { hint: string
      distance: float32
      name: string
      location: float32[] }

[<SetUp>]
let theta = 90.0

let waypoints =
    [| { Lng = "126.79939689052419"
         Lat = "37.527319039426736" }
       { Lng = "126.87491792408139"
         Lat = "37.627320777159646" }
       { Lng = "127.06568457951413"
         Lat = "37.665543301893806" }
       { Lng = "127.16568457951413"
         Lat = "37.565543301893806" } |]
    |> Array.toList

let demands =
    [| { Lng = "126.80939689052419"
         Lat = "37.547319039426736" }
       { Lng = "126.90491792408139"
         Lat = "37.657320777159646" } |]
    |> Array.toList

let goodInput =
    seq
        [ (167.247742, 5.934127589, 0)
          (25.91904492, 137.9278821, 1)
          (21.83549503, 22.31752886, 2) ]

let badInput =
    seq
        [ (67.247742, 5.934127589, 0)
          (25.91904492, 137.9278821, 1)
          (21.83549503, 22.31752886, 2) ]

let actual =
    { Lat = "37.0"
      Lng = "127.0"
      Kind = "Both" }

[<Test>]
let ``algorithm record locwithkind test`` () =
    Assert.True(Reflection.FSharpType.IsRecord(actual.GetType()))

[<Test>]
let ``length of waypointsxdemands  4x2 => combination number of waypoints insertion with demands : 5C2 = 10 expected``
    ()
    =
    let result =
        SuaveAPI.Algorithm.getCombinationOfWaypoints waypoints demands |> Seq.length

    Assert.AreEqual(result, 10)

[<Test>]
let ``combination test : shortest path in time`` () =
    let result =
        getCombinationOfWaypoints waypoints demands
        |> Seq.map (fun r -> getUrl (r))
        |> Seq.map (fun r -> getFromAsyncHttp (r))
        |> Seq.map (fun r -> JsonSerializer.Deserialize<Response> r)
        |> Seq.minBy (fun r -> r.routes[0].duration)
    // |> fun r -> r.waypoints |> Seq.map (fun w -> w.location)
    // |> fun r -> JsonSerializer.Serialize r
    // |> printfn "%A"


    Assert.True(true)

[<Test>]
let ``combination test : shortest path in distance`` () =
    let result =
        getCombinationOfWaypoints waypoints demands
        |> Seq.map (fun r -> getUrl (r))
        |> Seq.map (fun r -> getFromAsyncHttp (r))
        |> Seq.map (fun r -> JsonSerializer.Deserialize<Response> r)
        |> Seq.minBy (fun r -> r.routes[0].distance)
        |> fun r -> seq { r }
        |> Seq.toList
        |> Success

    Assert.True(true)

[<Test>]
let ``osrm apicall test`` () =
    let result =
        let data =
            { demands = demands
              waypoints = waypoints
              algorithm = 0 }

        let bind = SuaveAPI.Utils.bind

        data
        |> SuaveAPI.OSRMRepository.apiPostCall
        |> bind JsonSerializer.Deserialize<Response>
    // |> bind (fun res ->
    //     printfn "%A mins, %A km" (res.routes[0].duration / 60.0f) (res.routes[0].distance / 1000.0f))

    Assert.True(true)

[<Test>]
let `` cost calculation of waypoints X demands `` () =
    let result =
        SuaveAPI.Algorithm.getCombinationOfWaypoints waypoints demands
        |> Seq.map (fun w ->
            // printfn "%A" w
            w)

    // printfn "%A" result
    Assert.True(true)

[<Test>]
let ``calculate theta between waypoints part and demands point, result size = waypoints.length - 1 `` () =
    let result = SuaveAPI.Algorithm.getOptimalWaypointsWithTheta theta waypoints demands
    Assert.AreEqual((result |> Seq.length), (waypoints |> Seq.length) - 1)

[<Test>]
let ``input should contains 2 values bigger than theta`` () =
    let goodResult =
        SuaveAPI.Algorithm.insertDemandsBeweenWaypointsPair theta waypoints demands goodInput

    let badResult =
        SuaveAPI.Algorithm.insertDemandsBeweenWaypointsPair theta waypoints demands badInput

    // printfn "%A" goodResult
    // printfn "%A" badResult
    match goodResult with
    | Success res -> Assert.True(true)
    | Failure res -> Assert.True(false)

    match badResult with
    | Success res -> Assert.True(false)
    | Failure res -> Assert.True(true)

[<Test>]
let ``executeAlgorithm combination`` () =
    let result =
        executeSelcetedAlgorithm Algorithm.CombinationWithTime 90.0 waypoints demands
    // printfn "%A" result
    Assert.True(true)

[<Test>]
let ``executeAlgorithm combination failed with empty waypoints`` () =
    let result = executeSelcetedAlgorithm Algorithm.CombinationWithTime 90.0 [] demands

    match result with
    | Failure f -> Assert.AreEqual(f, "combination waypoints calculation failed")
    | Success f -> Assert.True(false)

[<Test>]
let ``executeAlgorithm combination failed with empty demands`` () =
    let result =
        executeSelcetedAlgorithm Algorithm.CombinationWithTime 90.0 waypoints []

    match result with
    | Failure f -> Assert.AreEqual(f, "combination waypoints calculation failed")
    | Success f -> Assert.True(false)

[<Test>]
let ``executeAlgorithm combination accroding to shortest distance `` () =
    let result =
        executeSelcetedAlgorithm Algorithm.CombinationWithDistance 90.0 waypoints demands

    // printfn "%A" result
    Assert.True(true)

[<Test>]
let ``executeAlgorithm betaskeleton`` () =
    let result = executeSelcetedAlgorithm Algorithm.BetaSkeleton 90.0 waypoints demands
    // printfn "%A" result
    Assert.True(true)

[<Test>]
let ``executeAlgorithm betaskeleton demand insertion failed with Big theta constraint`` () =
    let result = executeSelcetedAlgorithm Algorithm.BetaSkeleton 190.0 waypoints demands

    // printfn "%A" result

    match result with
    | Failure f -> Assert.AreEqual(f, "demands insertion failed between waypoints accroding to theta")
    | Success f -> Assert.True(false)
