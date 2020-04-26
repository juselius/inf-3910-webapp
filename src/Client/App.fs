module Client

open Elmish
open Thoth.Json
open Thoth.Fetch
open Shared

type Msg =
    | Init of Result<Counter, FetchError>
    | Increment
    | Decrement
    | Load
    | LoadHandler of Result<Person list, FetchError>
    | Save
    | SaveHandler of Result<PersonId, FetchError>
    | Exn of System.Exception
    | Sort of bool option
    | UpdateFirst of string
    | UpdateLast of string
    | UpdateAlias of string
    | UpdateAge of int
    | UpdateHeight of int
    | Edit of Person
    | StartEdit of Result<PersonId * Person, FetchError>
    | Delete of Person
    | DoDelete of Result<PersonId * Person, FetchError>
    | Update
    | Login of LoginPage.Model
    | Logout
    | LoginHandler of Result<string option, FetchError>
    | UrlChanged of string list

let init () =
    let user = Decode.Auto.generateDecoder<string option> ()
    let counter = Decode.Auto.generateDecoder<Counter> ()
    let p1 () = promise { return! Fetch.tryGet("/api/me", decoder = user) }
    let p2 () = promise { return! Fetch.tryGet("/api/init", decoder = counter) }
    let model = {
        Count = 0
        People = []
        Sort = None
        NewPerson = None
        CurrentUrl = []
        User = None
    }
    let cmd = Cmd.batch [
        Cmd.OfPromise.either p1 () LoginHandler Exn
        Cmd.OfPromise.either p2 () Init Exn
    ]
    model, cmd

let handleInit model =
    function
    | Ok counter ->
        { model with Count = counter.Value }, Cmd.none
    | Error err ->
        printfn "ERROR: handleInit (): %A" err
        model, Cmd.none

let private trimPerson p =
    { p with
        First = p.First.Trim ()
        Last = p.Last.Trim ()
        Alias = p.Alias |> Option.map (fun x -> x.Trim ())
    }

let savePerson model =
    let decoder = Decode.Auto.generateDecoder<PersonId> ()
    match model.NewPerson with
    | Some (_, pers) ->
        let p () =
            promise {
                return! Fetch.tryPost (
                    "/api/person",
                    data = trimPerson pers,
                    decoder = decoder
                )
            }
        model, Cmd.OfPromise.either p () SaveHandler Exn
    | None ->
        printfn "WARNING: savePerson (): not reached."
        model, Cmd.none

let updatePerson model =
    let decoder = Decode.Auto.generateDecoder<PersonId> ()
    match model.NewPerson with
    | Some (pId, pers) ->
        let p () =
            promise {
                return! Fetch.tryPut (
                    "/api/person",
                    data = (pId, trimPerson pers),
                    decoder = decoder
                )
            }
        model, Cmd.OfPromise.either p () SaveHandler Exn
    | None ->
        printfn "WARNING: updatePerson (): not reached."
        model, Cmd.none

let commitPerson model =
    function
    | Ok _ ->
        if model.NewPerson.IsSome then
            { model with
                People = (snd model.NewPerson.Value) :: model.People
                NewPerson = None
            }, Cmd.none
        else
            model, Cmd.none
    | Error err ->
        printfn "ERROR: savePerson: %A" err
        model, Cmd.none

let fetchPeople model =
    let decoder = Decode.Auto.generateDecoder<Person list> ()
    let p () =
        promise {
            let people = Fetch.tryGet("/api/people", decoder = decoder)
            return! people
        }
    model, Cmd.OfPromise.either p () LoadHandler Exn

let toggleSortOrder (x : bool option) =
    if x.IsNone then
        Some true
    else if x.Value then
        Some false
    else
        None

let handleSort model order =
    match order with
    | Some up ->
        let people' =
            if up then
                List.sort model.People
            else
                List.sortDescending model.People
        { model with
            People = people'
            Sort = toggleSortOrder model.Sort
        } , Cmd.none
    | None -> fetchPeople { model with Sort = None }

let setPerson model (update : Person -> Person) =
    let (pId, p) = Option.defaultValue (0, Person.New) model.NewPerson
    let p' = update p
    let model' = { model with NewPerson = Some (pId, p') }
    model', Cmd.none

let handleLoad model =
    function
    | Ok p -> { model with People = p }, Cmd.none
    | Error err -> printfn "ERROR: %A" err; model, Cmd.none

let updateFirst model (s : string) =
    setPerson model (fun p -> { p with First = s })

let updateLast model (s : string) =
    setPerson model (fun p -> { p with Last = s })

let updateAlias model (s : string) =
    setPerson model (fun p -> { p with Alias = Some s })

let updateAge model n =
    setPerson model (fun p -> { p with Age = n })

let updateHeight model n =
    setPerson model (fun p -> { p with Height = n })

type Promp = (unit -> Fable.Core.JS.Promise<Result<(PersonId * Person), FetchError>>) ->  Cmd<Msg>

let getPerson (model : Model) (person : Person) (cmd : Promp) =
    let decoder = Decode.Auto.generateDecoder<PersonId * Person> ()
    let req = sprintf "/api/person/%s/%s" person.First person.Last
    let p () =
        promise {
            return! Fetch.tryGet(req, decoder = decoder)
        }
    model, cmd p

let handleEdit model (person : Person) =
    getPerson model person (fun p -> Cmd.OfPromise.either p () StartEdit Exn)

let handleStartEdit model (p : Result<PersonId * Person, FetchError>) =
    match p with
    | Ok person ->
        let model' =
            { model with
                NewPerson = Some person
                People =
                    model.People
                    |> List.filter ((<>) (snd person) )
        }
        model', Cmd.none
    | Error err ->
        printfn "ERROR: handleStartEdit: %A" err
        model, Cmd.none

let handleDelete model (person : Person) =
    getPerson model person (fun p -> Cmd.OfPromise.either p () DoDelete Exn)

let handleCommitDelete model (p : Result<PersonId * Person, FetchError>) =
    let decoder = Decode.Auto.generateDecoder<unit> ()
    match p with
    | Ok (pId, person) ->
        let p () =
            promise {
              return! Fetch.delete ("/api/person", pId, decoder = decoder)
            }
        let people = model.People |> List.filter ((<>) person)
        let model' = { model with People = people }
        model', Cmd.OfPromise.attempt p () Exn
    | Error err ->
        printfn "ERROR: handleCommitDelete: %A" err
        model, Cmd.none

let handleLogin (model : Model) (m : LoginPage.Model) =
    let decoder = Decode.Auto.generateDecoder<string option> ()
    let p () =
        promise {
            return! Fetch.tryPost (
                "/api/login",
                data = (m.Email, m.Password),
                decoder = decoder
            )
        }
    model, Cmd.OfPromise.either p () LoginHandler Exn

let handleLoginResult model =
    function
    | Ok usr ->
        { model with
            User = usr
            CurrentUrl = []
        }, Cmd.none
    | Error err -> { model with CurrentUrl = [ "unauthorized"; string err ] }, Cmd.none

let handleLogout (model : Model) =
    let decoder = Decode.Auto.generateDecoder<unit> ()
    let p () =
        promise {
            return! Fetch.get ("/signout", decoder = decoder)
        }
    let model' = { model with User = None }
    model', Cmd.OfPromise.attempt p () Exn

let update (msg: Msg) (model : Model) =
    match msg with
    | UrlChanged x -> { model with CurrentUrl = x }, Cmd.none
    | Login m -> handleLogin model m
    | LoginHandler usr -> handleLoginResult model usr
    | Logout -> handleLogout model
    | Init c -> handleInit model c
    | Increment -> { model with Count = model.Count + 1 }, Cmd.none
    | Decrement -> { model with Count = model.Count - 1 }, Cmd.none
    | Exn exn -> printfn "EXN: %A" exn; model, Cmd.none
    | Load -> fetchPeople model
    | LoadHandler x -> handleLoad model x
    | Sort x -> handleSort model x
    | Save -> savePerson model
    | SaveHandler p -> commitPerson model p
    | UpdateFirst s -> updateFirst model s
    | UpdateLast s -> updateLast model s
    | UpdateAlias s -> updateAlias model s
    | UpdateAge n -> updateAge model n
    | UpdateHeight n -> updateHeight model n
    | Edit p -> handleEdit model p
    | StartEdit p -> handleStartEdit model p
    | Delete p -> handleDelete model p
    | DoDelete p -> handleCommitDelete model p
    | Update -> updatePerson model
