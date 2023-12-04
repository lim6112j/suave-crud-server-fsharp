namespace SuaveAPI.UserRepository

open System.Collections.Generic


type User =
    { UserId: int
      Name: string
      Age: int
      Address: string
      Salary: double }

module UserRepository =

    let users = new Dictionary<int, User>()
    let getUsers () = users.Values :> seq<User>

    let getUser id =
        if users.ContainsKey(id) then Some users.[id] else None

    let createUser user =
        let id = users.Values.Count + 1
        let newUser = { user with UserId = id }
        users.Add(id, newUser)
        newUser

    let updateUserById id userToUpdate =
        if users.ContainsKey(id) then
            let updatedUser = { userToUpdate with UserId = id }
            users.[id] <- updatedUser
            Some updatedUser
        else
            None

    let updateUser userToUpdate =
        updateUserById userToUpdate.UserId userToUpdate

    let deleteUser id = users.Remove(id) |> ignore
