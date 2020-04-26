namespace Shared

type Counter = { Value : int }

type Person = {
    First : string
    Last : string
    Alias : string option
    Age : int
    Height : int
} with
    static member New = {
        First = ""
        Last = ""
        Alias = None
        Age = 0
        Height = 0
    }


