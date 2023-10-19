namespace SuaveAPI

open FSharp.Configuration

module Program =
    open Suave.Web
    open Suave
    open Suave.Filters
    open Suave.Operators
    open SuaveAPI.UserService
    open SuaveAPI.UserRepository
    open SuaveAPI.VstopRepository
    open SuaveAPI.VstopService
    open EnvVariable.EnvVar
    open System

    [<Literal>]
    let appSettingsPath = __SOURCE_DIRECTORY__ + "/app.config"

    type Settings = AppSettings<appSettingsPath>

    [<EntryPoint>]
    let main argv =
        // environment variable
        printfn "%s" (Environment.GetEnvironmentVariable "PATH")
        // app settings (app.config)

        printfn "%s" Settings.ConfigFileName

        let configPath =
            System.IO.Path.Combine [| __SOURCE_DIRECTORY__; "bin"; "Debug"; "net7.0"; "suave-crud-server" |]

        printfn "%s" configPath
        Settings.SelectExecutableFile configPath
        printfn "%s" Settings.ConnectionStrings.LocalSqlServer
        let vstopActions = vstopHandle "vstops" { ListVstops = VstopRepository.getVstops }

        let userActions =
            handle
                "users"
                { ListUsers = UserRepository.getUsers
                  GetUser = UserRepository.getUser
                  AddUser = UserRepository.createUser
                  UpdateUser = UserRepository.updateUser
                  UpdateUserById = UserRepository.updateUserById
                  DeleteUser = UserRepository.deleteUser }

        let app =
            choose
                [ GET >=> path "/vstops" >=> vstopActions
                  GET >=> path "/users" >=> userActions ]

        startWebServer defaultConfig app
        0
