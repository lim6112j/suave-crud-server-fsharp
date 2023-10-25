namespace SuaveAPI

open Npgsql.FSharp

[<AutoOpen>]
module PostgresCon =
    let connectionString: string =
        Sql.host "localhost"
        |> Sql.database "mobble"
        // |> Sql.database "postgres"
        |> Sql.username "postgres"
        |> Sql.password ""
        |> Sql.port 5432
        |> Sql.formatConnectionString

    type Vstops =
        { Vstop_idx: int
          Vstop_name: string
          Vstop_loc: string }

    let getAllVstops (connectionString: string) : Vstops list =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM mobble_dispatcher.stops_virtual"
        |> Sql.execute (fun read ->
            { Vstop_idx = read.int "vstop_idx"
              Vstop_name = read.string "vstop_name"
              Vstop_loc = read.string "vstop_loc" })
