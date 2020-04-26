open System
open System.IO

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.Extensions.DependencyInjection
open System.Security.Cryptography
open System.Security.Claims
open FSharp.Control.Tasks.V2

open Giraffe
open Api

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath =
    tryGetEnv "CONTENT_ROOT"
    |> function
    | Some root -> Path.GetFullPath root
    | None -> Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let cookieChallenge (next : HttpFunc) (ctx : HttpContext) =
    printfn "cookie challenge"
    challenge CookieAuthenticationDefaults.AuthenticationScheme next ctx

let authorize (next : HttpFunc) (ctx : HttpContext) =
    requiresAuthentication cookieChallenge next ctx

let validateUser (userName : string) (passwd : string) =
    userName.Length > 1 && passwd.Length > 1

let signIn (next : HttpFunc) (ctx : HttpContext) =
    task {
        let! user, password = ctx.BindJsonAsync<string * string> ()
        if validateUser user password then // not for production!
            let claims = [ Claim (ClaimTypes.Name, user) ]
            let identity = ClaimsIdentity (claims, CookieAuthenticationDefaults.AuthenticationScheme)
            let principal = ClaimsPrincipal identity
            do! ctx.SignInAsync (CookieAuthenticationDefaults.AuthenticationScheme, principal)
            return! json (Some user) next ctx
        else
            return! json None next ctx
    }

let signOut (next : HttpFunc) (ctx : HttpContext) =
        task {
            do! ctx.SignOutAsync()
            return! next ctx
        }

let getUser (next : HttpFunc) (ctx : HttpContext) =
    let user =
        if ctx.User.Identity.IsAuthenticated then
            Some ctx.User.Identity.Name
        else
            None
    task {
        return! json user next ctx
    }

let webApp : HttpHandler =
    choose [
        choose [
            route "/api/me" >=> getUser
            route "/api/logout" >=> signOut >=> redirectTo false "/"
            route "/api/init" >=> handleInit
            route "/api/people" >=> handleGetPeople
            routef "/api/person/%s/%s" handleGetPerson
            POST >=> route "/api/login" >=> signIn
        ]
        authorize >=> choose [
            route "/api/person"
                >=> choose [
                    POST   >=> handleAddPerson
                    PUT    >=> handleUpdatePerson
                    DELETE >=> handleDeletePerson
                ]
        ]
    ]

let jsonSerializer = Thoth.Json.Giraffe.ThothSerializer ()

let authenticationOptions (opt : AuthenticationOptions) =
    opt.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme

let cookieOptions = Action<CookieAuthenticationOptions> (fun options ->
    options.LoginPath <- PathString "/signin")

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    services.AddSingleton<Serialization.Json.IJsonSerializer>(jsonSerializer) |> ignore
    services.AddAuthentication(authenticationOptions)
        .AddCookie(cookieOptions)
        |> ignore

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles()
       .UseStaticFiles()
       .UseCookiePolicy()
       .UseAuthentication()
       .UseGiraffe webApp

Db.tryMigrate ()

WebHost
    .CreateDefaultBuilder()
    .UseWebRoot(publicPath)
    .UseContentRoot(publicPath)
    .Configure(Action<IApplicationBuilder> configureApp)
    .ConfigureServices(configureServices)
    .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
    .Build()
    .Run()
