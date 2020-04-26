module Testing.Property

open Expecto
open FsCheck
open Shared

let config = { FsCheckConfig.defaultConfig with maxTest = 1000 }

[<Tests>]
let examples =
  testList "FsCheck samples" [
    testProperty "Addition is commutative"
        (fun a b -> a + b = b + a)

    testProperty "Reverse of reverse of a list is the original list"
        (fun (xs : int list) -> List.rev (List.rev xs) = xs)

    // you can also override the FsCheck config
    testPropertyWithConfig config "Product is distributive over addition"
        (fun a b c -> a * (b + c) = a * b + a * c)
]


type PersonGen() =
   static member Person() : Arbitrary<Person> =
        let genFirsName = Gen.elements ["Don"; "Henrik"; "Reodor"; ""]
        let genLastName = Gen.elements ["Syme"; "Feldt"; "Felgen"; ""]
        let genAlias =
            Gen.choose (32, 64)
            |> Gen.sample 10 10
            |> List.map char
            |> string
            |> Gen.constant
            |> Gen.optionOf
        let genAge = Gen.choose (1,101)
        let genHeight = Gen.choose (99,200)
        let createPerson firstName lastName alias age height = {
            First = firstName
            Last = lastName
            Alias = alias
            Age = age
            Height = height
        }
        let genPerson =
           createPerson
           <!> genFirsName
           <*> genLastName
           <*> genAlias
           <*> genAge
           <*> genHeight
        genPerson |> Arb.fromGen

let config' = {
    FsCheckConfig.defaultConfig with
        arbitrary = [typeof<PersonGen>]
        maxTest = 1000
    }

[<Tests>]
let properties =
    testList "Person properties" [
        testPropertyWithConfig config' "User with generated User data"
            (fun x ->
                Expect.isNotNull x.First "First name should not be null"
                Expect.isNotNull x.Last "Last name should not be null"
                Expect.isGreaterThan x.Age 0 "Age should be larger than 0"
                Expect.isLessThan x.Age 100 "Age should not be larger than 100"
                Expect.isLessThanOrEqual x.Height 200 "Height should not be larger than 200"
            )

        testPropertyWithConfig config' "Converting to Entity.Person and back is idempotent"
            (fun x ->
                let p = Db.fromPerson x |> Db.toPerson
                Expect.equal x p
            )
  ]