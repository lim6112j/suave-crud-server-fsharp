namespace SuaveAPI.UserService

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Successful

[<AutoOpen>]
module UserService =
    open Suave.Filters
    open Suave.RequestErrors
    // auxiliary methods
    let getUTF8 (str: byte[]) =
        System.Text.Encoding.UTF8.GetString(str)

    let jsonToObject<'t> json =
        JsonConvert.DeserializeObject(json, typeof<'t>) :?> 't

    let JSON v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings) |> OK
        >=> Writers.setMimeType "application/json"

    type Actions<'t> =
        { ListUsers: unit -> 't seq
          GetUser: int -> 't option
          AddUser: 't -> 't
          UpdateUser: 't -> 't option
          UpdateUserById: int -> 't -> 't option
          DeleteUser: int -> unit }

    let getActionData<'t> (req: HttpRequest) =
        req.rawForm |> getUTF8 |> jsonToObject<'t>

    let handle nameOfAction action =
        let badRequest = BAD_REQUEST "Oops, something went wrong here!"
        let notFound = NOT_FOUND "Oops, I couldn't find that!"

        let handleAction reqError =
            function
            | Some r -> r |> JSON
            | _ -> reqError

        let listAll = warbler (fun _ -> action.ListUsers() |> JSON)
        let getById = action.GetUser >> handleAction notFound

        let updateById id =
            request (getActionData >> (action.UpdateUserById id) >> handleAction badRequest)

        let deleteById id =
            action.DeleteUser id
            NO_CONTENT

        let actionPath = "/" + nameOfAction

        choose
            [ path actionPath
              >=> choose
                  [ GET >=> listAll
                    POST >=> request (getActionData >> action.AddUser >> JSON)
                    PUT >=> request (getActionData >> action.UpdateUser >> handleAction badRequest) ]
              DELETE >=> pathScan "/users/%d" (fun id -> deleteById id)
              GET >=> pathScan "/users/%d" (fun id -> getById id)
              PUT >=> pathScan "/users/%d" (fun id -> updateById id) ]
