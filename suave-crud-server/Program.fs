namespace SuaveAPI

open FSharp.Configuration

module Program =
    open Suave
    open Suave.Web
    open Suave.Filters
    open Suave.Operators
    open Suave.Successful
    open SuaveAPI.UserService
    open SuaveAPI.UserRepository
    open SuaveAPI.VstopRepository
    open SuaveAPI.VstopService
    open SuaveAPI.OSRMRepository
    open SuaveAPI.OSRMService
    open SuaveAPI.Utils
    open EnvVariable.EnvVar
    open System
    open System.Threading

    // type Settings = AppSettings<"app.config">
    type YamlSettings = YamlConfig<"config.yaml">

    [<EntryPoint>]
    let main argv =

        // environment variable
        // printfn "%s" (Environment.GetEnvironmentVariable "PATH")
        // app settings (app.config)
        let config = YamlSettings()
        printfn "%s" config.DB.ConnectionString


        let osrmActions = osrmHandle "osrm" { postOSRM = OSRMRepository.apiPostCall }

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
                  GET >=> path "/users" >=> userActions
                  POST >=> path "/osrm" >=> osrmActions ]

        let cancellationTokenSource = new CancellationTokenSource()

        let customPort = 8085

        let webServerConfig =
            { defaultConfig with
                bindings = [ HttpBinding.createSimple HTTP "0.0.0.0" customPort ]
                cancellationToken = cancellationTokenSource.Token }

        let _, webServer = startWebServerAsync webServerConfig app
        Async.Start(webServer, cancellationTokenSource.Token) |> ignore
        Console.ReadKey true |> ignore
        cancellationTokenSource.Cancel()
        0
