namespace SuaveAPI

open Npgsql.FSharp
open Microsoft.Extensions.Configuration
open System.IO
open NetEscapades.Configuration.Yaml

[<AutoOpen>]
module PostgresCon =
    let configuration = 
        ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddYamlFile("config.yaml", optional = false, reloadOnChange = true)
            .AddEnvironmentVariables()
            .Build()

    let connectionString =
        let configString = configuration.GetConnectionString("DefaultConnection")
        if isNull configString then
            Sql.host "localhost"
            |> Sql.database "mobble"
            |> Sql.username "postgres"
            |> Sql.password "Vcc8tVfQRqmn4zfjM3wk"
            |> Sql.port 5432
            |> Sql.formatConnectionString
        else
            configString

    printfn "Using connection string: %s" connectionString

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