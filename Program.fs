namespace SuaveAPI

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

    [<EntryPoint>]
    let main argv =
        printfn "%s" (Environment.GetEnvironmentVariable "PATH")
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
