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

    [<EntryPoint>]
    let main argv =

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


        startWebServer defaultConfig userActions
        0
