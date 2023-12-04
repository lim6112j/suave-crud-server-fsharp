module suava_crud_server.Tests

open NUnit.Framework
open Microsoft.FSharp.Reflection
open SuaveAPI

[<SetUp>]
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
    let result = SuaveAPI.Algorithm.getCombinationOfWaypoints waypoints demands
    let lenResult = (result |> Seq.length)
    Assert.AreEqual(lenResult, 10)

[<Test>]
let ``calculate theta between waypoints part and demands point, result size = waypoints.length - 1 `` () =
    let result = SuaveAPI.Algorithm.getOptimalWaypointsWithTheta 90.0 waypoints demands
    Assert.AreEqual((result |> Seq.length), (waypoints |> Seq.length) - 1)

[<Test>]
let ``input should contains 2 values bigger than theta`` () =
    let goodResult =
        SuaveAPI.Algorithm.insertDemandsBeweenWaypointsPair 90.0 waypoints demands goodInput

    let badResult =
        SuaveAPI.Algorithm.insertDemandsBeweenWaypointsPair 90.0 waypoints demands badInput

    // printfn "%A" goodResult
    // printfn "%A" badResult
    match goodResult with
    | Success res -> Assert.True(true)
    | Failure res -> Assert.True(false)

    match badResult with
    | Success res -> Assert.True(false)
    | Failure res -> Assert.True(true)
