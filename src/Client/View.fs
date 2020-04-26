module View

open Feliz
open Feliz.Bulma
open Feliz.Router
open Shared
open Client

let peopleView model dispatch =
    Bulma.table [
        table.isFullWidth
        table.isStriped
        prop.children [
            Html.thead [
               Html.tr [
                   prop.onClick (fun _ ->
                        dispatch (Sort (toggleSortOrder model.Sort)))
                   prop.children [
                       Html.th [
                           prop.text "First"

                       ]
                       Html.th "Last"
                       Html.th "Alias"
                       Html.th "Age"
                       Html.th "Height"
                       Html.th [
                            prop.width 5
                            prop.text "Edit"
                       ]
                       Html.th [
                            prop.width 5
                            prop.text "Remove"
                       ]
                   ]
               ]
            ]
            Html.tbody [
                for i in model.People do
                    Html.tr [
                        Html.td i.First
                        Html.td i.Last
                        Html.td (Option.defaultValue "" i.Alias)
                        Html.td i.Age
                        Html.td i.Height
                        Html.td [
                            Bulma.button.button [
                                button.isSmall
                                prop.onClick (fun _ -> dispatch (Edit i))
                                prop.children [
                                    Html.div [ prop.className "fa fa-edit"]
                                ]
                            ]
                        ]
                        Html.td [
                            Bulma.button.button [
                                button.isSmall
                                prop.onClick (fun _ -> dispatch (Delete i))
                                prop.children [
                                    Html.div [ prop.className "fa fa-trash"]
                                ]
                            ]
                        ]
                    ]
            ]
        ]
    ]

let counterView model dispatch =
    Bulma.box [
        Bulma.title.h3 [
            prop.text ("Initial people counter: " + string model.Count)
            prop.id "strike"
        ]
        Bulma.button.button [
            color.isSuccess
            prop.style [ style.marginRight 7 ]
            prop.onClick (fun _ -> dispatch Increment)
            prop.text "Increment"
        ]
        Bulma.button.button [
            color.isPrimary
            prop.onClick (fun _ -> dispatch Decrement)
            prop.text "Decrement"
        ]
    ]

let addPersonView model dispatch =
    let input' ph (msg : string -> Msg) (value : string) =
        Html.input [
            prop.placeholder ph
            prop.onChange (msg >> dispatch)
            prop.value value
            prop.id ph
        ]
    let iinput' ph (msg : int -> Msg) (value : int) =
        Html.input [
            prop.type' "number"
            prop.placeholder ph
            prop.onChange (int >> msg >> dispatch)
            prop.value value
            prop.id ph
        ]
    let (pId, p) = Option.defaultValue (0, Person.New) model.NewPerson
    Bulma.panel [
        Bulma.panelHeading [ Bulma.title.h5 "Add people" ]
        Bulma.panelBlock.div [
        Bulma.columns [
            Bulma.column [
                Bulma.label "First name"
                input' "First" UpdateFirst p.First
            ]
            Bulma.column [
                Bulma.label "Last name"
                input' "Last" UpdateLast p.Last
            ]
            Bulma.column [
                Bulma.label "Alias"
                input' "Alias" UpdateAlias (Option.defaultValue "" p.Alias)
            ]
            Bulma.column [
                Bulma.label "Age"
                iinput' "Age" UpdateAge p.Age
            ]
            Bulma.column [
                Bulma.label "Height"
                iinput' "Height" UpdateHeight p.Height
            ]
        ]
        Bulma.button.button [
            if pId > 0 then
                prop.text "Update"
                prop.style [ style.marginLeft 10 ]
                color.isDark
                prop.onClick (fun _ -> dispatch Update)
            else
                prop.text "Save"
                prop.style [ style.marginLeft 10 ]
                color.isDark
                prop.onClick (fun _ -> dispatch Save)
            if model.NewPerson.IsNone then
                prop.disabled true
            else
                prop.disabled false
            prop.id "Save"
        ]
        ]
    ]

let loadButton model dispatch =
    Bulma.field.div [
        Bulma.button.button [
            if model.People.Length > 0 then
                prop.disabled true
            else
                color.isInfo
            prop.style [ style.marginRight 7 ]
            prop.onClick (fun _ -> dispatch Load)
            prop.text "Load people"
            prop.id "Load"
        ]
    ]

let navBar model dispatch =
    Bulma.navbar [
        color.isLight
        prop.children [
            Bulma.navbarStart.div [
                Bulma.navbarItem.div [
                    Bulma.title.h4 [
                        prop.text "INF-3910-5"
                    ]
                ]
            ]
            Bulma.navbarEnd.div [
                Bulma.navbarItem.div [
                    if model.User.IsNone then
                        Bulma.button.a [
                            prop.onClick (fun _ ->
                                dispatch (UrlChanged [ "login" ]))
                            color.isBlack
                            prop.text "Login"
                            prop.id "login"
                        ]
                    else
                        Html.h5 [
                            prop.text (string model.User)
                            prop.style [ style.marginRight 10 ]
                        ]
                        Bulma.button.a [
                            prop.onClick (fun _ -> dispatch Logout)
                            color.isBlack
                            prop.text "Logout"
                            button.isOutlined
                        ]
                ]
            ]
        ]
    ]

let homePage model dispatch =
    Html.div [
        navBar model dispatch
        Bulma.container [
            Bulma.title.h3 (Interop.Hello.hello "F# JS Interop")
            Bulma.field.div [ counterView model dispatch ]
            Bulma.field.div [
                if model.User.IsSome then
                    addPersonView model dispatch
                if model.People.Length > 0 then
                    Bulma.box [
                        peopleView model dispatch
                        Charts.chartsView model
                    ]
                loadButton model dispatch
            ]
        ]
    ]

let loginPage model dispatch =
    LoginPage.loginPage (Login >> dispatch)

let render (model: Model) (dispatch: Msg -> unit) =
    let currentPage =
        match model.CurrentUrl with
        | [] -> homePage model dispatch
        | [ "login" ] -> loginPage  model dispatch
        | [ "unauthorized"; err ] -> Html.h3 ("Not authorized: " + err)
        | _ -> Html.h1 "Page not found"

    Router.router [
        Router.onUrlChanged (UrlChanged >> dispatch)
        Router.application currentPage
    ]