module Testing.Unit

open Expecto
open Shared

[<Tests>]
let serverTests =
    testList "Db" [
      testCase "add person 1" <| fun _ ->
        let result =
            Db.createPerson {
                First = "Foo"
                Last = "Bar"
                Alias = Some "Raboof"
                Age = 31
                Height = 183
            }
        Expect.isOk result "Add Ok"

      testCase "add person 2" <| fun _ ->
        let result =
            Db.createPerson {
                First = "Reodor"
                Last = "Felgen"
                Alias = Some "Flåklypa"
                Age = 73
                Height = 171
            }
        Expect.isOk result "Add Ok"

      testCase "load people" <| fun _ ->
        let result = Db.getPeople ()
        Expect.isOk result "Load Ok"

      testCase "load person " <| fun _ ->
        let result = Db.getPerson "Reodor" "Felgen"
        Expect.isOk result "Get Ok"
        match result with
        | Ok (pId, person) ->
            Expect.isTrue (pId > 0) "pId > 0"
            Expect.equal person.Age 73 "Reodor is 73"
        | Error _ ->
            Tests.failtest "This should not be possible!"
    ] |> testSequenced

// examples
[<Tests>]
let tests =
    testList "example success" [
      testCase "universe exists" <| fun _ ->
        let subject = true
        Expect.isTrue subject "I compute, therefore I am."

      testCase "I'm skipped (should skip)" <| fun _ ->
        Tests.skiptest "Yup, waiting for a sunny day..."

      testCase "contains things" <| fun _ ->
        Expect.containsAll [| 2; 3; 4 |] [| 2; 4 |]
          "This is the case; {2,3,4} contains {2,4}"

    ]

//[<Tests>]
let failures =
    testList "example failure" [
      testCase "when true is not (should fail)" <| fun _ ->
        let subject = false
        Expect.isTrue subject "I should fail because the subject is false"

      testCase "I'm always fail (should fail)" <| fun _ ->
        Tests.failtest "This was expected..."

      testCase "contains things (should fail)" <| fun _ ->
        Expect.containsAll [| 2; 3; 4 |] [| 2; 4; 1 |]
         "Expecting we have one (1) in there"

      testCase "UTF" <| fun _ ->
        Expect.equal "abcdëf" "abcdef" "These should equal"

      test "I am (should fail)" {
        "computation expression" |> Expect.equal true false
      }
    ]
