module Db

open Microsoft.EntityFrameworkCore
open Shared

let inline internal runDb qry =
    try
        let ctx = new Entity.DataContext ()
        qry ctx |> Ok
    with
    | e ->
        printfn "withDb: Exception: %s" e.Message
        Error (string e)

let tryMigrate () =
    printf "Running database migrations... "
    match runDb (fun ctx -> ctx.Database.Migrate ()) with
    | Ok _ -> printfn "done."
    | Error e -> printfn "exception in Db.tryMigrate: \n%s" (string e)

let toPerson (p : Entity.Person) =
    {
        First = p.First
        Last = p.Last
        Alias = if p.Alias = null then None else Some p.Alias
        Age = p.Age
        Height = p.Height
    }

let fromPerson (p : Person) =
    let person = Entity.Person()
    person.First <- p.First
    person.Last <- p.Last
    person.Age <- p.Age
    person.Height <- p.Height
    if p.Alias.IsSome then
        person.Alias <- p.Alias.Value
    else
        ()
    person

let createPerson (person : Person) =
    let entry = fromPerson person
    let qry (ctx : Entity.DataContext) =
        ctx.Add entry |> ignore
        ctx.SaveChanges () |> ignore
        entry.PersonId
    runDb qry

let getPerson (firstName : string) (lastName : string) =
    let qry (ctx : Entity.DataContext ) =
        query {
            for i in ctx.People do
                find (i.First.Contains firstName && i.Last.Contains lastName)
        }
    runDb qry
    |> Result.map (fun p -> (p.PersonId, toPerson p))

let updatePerson (pId : int) (person : Person) =
    let qry (ctx : Entity.DataContext ) =
        query {
            for i in ctx.People do
                where (i.PersonId = pId)
        }
        |> Seq.tryHead
        |> Option.map (fun entry ->
            entry.First <- person.First
            entry.Last <- person.Last
            entry.Age <- person.Age
            entry.Height <- person.Height
            if person.Alias.IsSome then
                entry.Alias <- person.Alias.Value
            else
                entry.Alias <- ""
            ctx.Update entry |> ignore
            ctx.SaveChanges () |> ignore
        )
        |> ignore
    runDb qry

let deletePerson (pId : int) =
    let qry (ctx : Entity.DataContext ) =
        query {
            for i in ctx.People do
                where (i.PersonId = pId)
        }
        |> Seq.tryHead
        |> Option.map (fun entry ->
            ctx.Remove entry |> ignore
            ctx.SaveChanges () |> ignore
        )
        |> ignore
    runDb qry

let getPeople () =
    let qry (ctx : Entity.DataContext ) =
        query {
            for i in ctx.People do
                select i
        }
    runDb qry
    |> Result.map (Seq.toList >> List.map toPerson)

