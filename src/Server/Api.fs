module Api

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open Giraffe
open Shared

let handleInit next (ctx : HttpContext) =
    task {
        match Db.getPeople () with
        | Ok people ->
            let counter = { Value = people.Length }
            return! json counter next ctx
        | Error err ->
            let counter = { Value = 42 }
            return! json counter next ctx
    }

let handleGetPeople next (ctx : HttpContext) =
    task {
        match Db.getPeople () with
        | Ok people ->
            return! json people next ctx
        | Error err ->
            return! RequestErrors.BAD_REQUEST (text err) next ctx
    }

let handleGetPerson
    ((firstName, lastName) : string * string)
    next (ctx : HttpContext) =
    task {
        try
            match Db.getPerson firstName lastName with
            | Ok person ->
                return! json person next ctx
            | Error err ->
                return! RequestErrors.BAD_REQUEST (text err) next ctx
        with exn ->
            return! RequestErrors.BAD_REQUEST (text exn.Message) next ctx
    }

let handleAddPerson next (ctx : HttpContext) =
    task {
        try
            let! data = ctx.BindJsonAsync<Person> ()
            match Db.createPerson data with
            | Ok pId ->
                return! json pId next ctx
            | Error err ->
                return! RequestErrors.BAD_REQUEST (text err) next ctx
        with exn ->
            return! RequestErrors.BAD_REQUEST (text exn.Message) next ctx
    }

let handleUpdatePerson next (ctx : HttpContext) =
    task {
        try
            let! pId, person = ctx.BindJsonAsync<int * Person> ()
            match Db.updatePerson pId person with
            | Ok _ ->
                return! json pId next ctx
            | Error err ->
                return! RequestErrors.BAD_REQUEST (text err) next ctx
        with exn ->
            return! RequestErrors.BAD_REQUEST (text exn.Message) next ctx
    }

let handleDeletePerson next (ctx : HttpContext) =
    task {
        try
            let! pId = ctx.BindJsonAsync<int> ()
            match Db.deletePerson pId with
            | Ok result ->
                return! json result next ctx
            | Error err ->
                return! RequestErrors.BAD_REQUEST (text err) next ctx
        with exn ->
            return! RequestErrors.BAD_REQUEST (text exn.Message) next ctx
    }
