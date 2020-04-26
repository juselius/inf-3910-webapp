module Testing.Main

open System
open Argu
open Expecto
open Testing

type Arguments =
    | Canopy of UI.CanopyMode option
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Canopy _ -> "run canopy tests"

let colorizer =
    function
    | ErrorCode.HelpText -> None
    | _ -> Some ConsoleColor.Red

let errorHandler = ProcessExiter (colorizer = colorizer )

[<EntryPoint>]
let main argv =
    let parser =
        ArgumentParser.Create<Arguments>(
            programName = "tests.exe",
            errorHandler = errorHandler
        )
    let args = parser.Parse argv
    if args.Contains Canopy then
        let canopy =
            args.GetResult (Canopy, defaultValue = Some UI.CanopyMode.Browser)
        match canopy with
        | Some mode -> UI.testUI mode
        | None -> UI.testUI UI.CanopyMode.Browser
    else
        Tests.runTestsInAssemblyWithCLIArgs [] argv

