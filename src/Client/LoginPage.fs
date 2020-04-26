// Login page with an interal MVU loop using Feliz.Components
module LoginPage

open Feliz
open Feliz.Bulma

type Model = {
    Email : string
    Password : string
    Remember : bool
}

type Props = {
    OnLogin : Model -> unit
}

type private Msg =
    | UpdateEmail of string
    | UpdatePassword of string
    | UpdateRemember of bool

let private emailField model dispatch =
    Bulma.field.div [
        Bulma.label "Email"
        Bulma.control.div [
            control.hasIconsLeft
            prop.children [
                Bulma.input.email [
                    prop.placeholder "bob@acme.com"
                    prop.required true
                    prop.onChange (UpdateEmail >> dispatch)
                    prop.id "loginpage-email"
                ]
                Bulma.icon [
                    icon.isSmall
                    icon.isLeft
                    prop.children [
                        Html.i [ prop.className "fa fa-envelope" ]
                    ]
               ]
            ]
        ]
    ]

let private passwordField model dispatch =
    Bulma.field.div [
        Bulma.label "Password"
        Bulma.control.div [
            control.hasIconsLeft
            prop.children [
                Bulma.input.password [
                    prop.placeholder ""
                    prop.required true
                    prop.onChange (UpdatePassword >> dispatch)
                    prop.id "loginpage-password"
                ]
                Bulma.icon [
                    icon.isSmall
                    icon.isLeft
                    prop.children [
                        Html.i [ prop.className "fa fa-lock" ]
                    ]
                ]
            ]
        ]
    ]

let private rememberField model dispatch =
    Bulma.field.div [
        Bulma.label [
            Bulma.input.checkbox [
                prop.value model.Remember
                prop.onSelect (fun _ ->
                    dispatch (UpdateRemember (not model.Remember)))
                prop.style [ style.marginRight 6]
            ]
            Html.text "Remember me"
        ]
    ]

let private loginField model onLogin =
    let active = model.Email.Length > 0 && model.Password.Length > 0
    Bulma.field.div [
        Bulma.button.a [
            prop.disabled (not active)
            prop.text "Login"
            prop.onClick (fun _ -> if active then onLogin model)
            color.isPrimary
            prop.id "loginpage-login"
        ]
    ]

let private loginForm onLogin model dispatch =
    Bulma.box [
        emailField model dispatch
        passwordField model dispatch
        rememberField model dispatch
        loginField model onLogin
    ]

let private update (model : Model) =
    function
    | UpdateEmail x -> { model with Email = x }
    | UpdatePassword x -> { model with Password = x }
    | UpdateRemember x -> { model with Remember = x}

let private initialModel =
    {
        Email = ""
        Password = ""
        Remember = false
    }

let private loginPage' onLogin model dispatch =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.children [
            Bulma.heroBody [
                Bulma.container [
                    Bulma.columns [
                        columns.isCentered
                        prop.children [
                            Bulma.column [
                                column.isOneThird
                                prop.children [
                                    loginForm onLogin model dispatch
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let private loginComponent =
    React.functionComponent ("LoginPage", fun (props : Props) ->
        let model, dispatch = React.useReducer (update, initialModel)
        let onLogin = React.useCallback props.OnLogin // memoize the callback
        loginPage' onLogin model dispatch
    )

let loginPage (onLogin : Model -> unit) = loginComponent { OnLogin = onLogin }