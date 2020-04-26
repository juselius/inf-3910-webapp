module Interop

open Fable.Core

type IHello =
    abstract hello : string -> string

[<ImportAll("./hello.js")>]
let Hello : IHello = jsNative