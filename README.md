![Build](https://github.com/juselius/inf-3910-webapp/workflows/Build/badge.svg)

# Example F# webapp

This repository contains a demo web application written in F#. The app is part
of the course INF-3910-5 "Applied functional programming in F#", though at
[UiT The Arctic University of Norway](https://uit.no/enhet/ifi).

This project demonstrates the follwing features:

* Fable and Elmish
* Feliz
  * Bulma
  * Components
  * Routing
* ASP.NET Core
  * Giraffe
  * Cookie Authentication
* Entity Framework Core
  *  SQLite3
* Testing
  * Expecto
  * FsCheck
  * Canopy
* DevOps
  * GitHub Actions
  * Docker

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you can also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.

## Build the application

To build and run the application:

```bash
dotnet fake build -t release
cd deploy
dotnet Server.dll
```

To build and containerize the application:

```bash
dotnet fake build -t release
docker build -t inf-3910-webapp .
docker run -p 8085:8085 inf-3910-webapp
```

## Work with the application

Before you run the project **for the first time only** you should install its local tools with this command:

```bash
dotnet tool restore
```

To concurrently run the server and the client components in watch mode use the following command:

```bash
dotnet fake build -t run
```

## Documentation

You will find more documentation about the used F# components at the following places:

* [Giraffe](https://github.com/giraffe-fsharp/Giraffe/blob/master/DOCUMENTATION.md)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
* [Feliz](https://github.com/Zaid-Ajaj/Feliz)

