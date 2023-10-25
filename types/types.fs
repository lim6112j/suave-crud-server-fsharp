namespace SuaveAPI

[<AutoOpen>]
module Types =
    type Loc = { Lng: string; Lat: string }

    type Result<'TSuccess, 'TFailure> =
        | Success of 'TSuccess
        | Failure of 'TFailure
