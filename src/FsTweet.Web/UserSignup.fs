namespace UserSignup

module Domain =
    open Chessie 
    open Chessie.ErrorHandling

    type Username = private Username of string with
        member this.Value =
            let (Username username) = this
            username
        static member TryCreate (username: string) =
            match username with
            | null | ""            -> fail "username should not be empty"
            | x when x.Length > 12 -> fail "username must be 12 chars or less"
            | x                    -> Username x |> ok
    type EmailAddress = private EmailAddress of string with
        member this.Value =
            let (EmailAddress emailAddress) = this
            emailAddress
        static member TryCreate (emailAddress : string) =
            try
                new System.Net.Mail.MailAddress(emailAddress) |> ignore
                EmailAddress emailAddress |> ok
            with
                | _ -> fail "Invalid Email Address"
    type Password = private Password of string with
        member this.Value =
            let (Password password) = this
            password
        static member TryCreate (password : string) =
            match password with
            | null | ""                           -> fail "Password should not be empty"
            | x when x.Length < 4 || x.Length > 8 -> fail "Password should contain only 4-8 characters"
            | x                                   -> Password x |> ok

    type SignupUserRequest = {
        Username     : Username
        Password     : Password
        EmailAddress : EmailAddress
    } with
        static member TryCreate (username, password, email) =
            trial {
                let! username     = Username.TryCreate username
                let! password     = Password.TryCreate password
                let! emailAddress = EmailAddress.TryCreate email
                return {
                    Username     = username
                    Password     = password
                    EmailAddress = emailAddress
                }
            }

module Suave =
    open Chessie
    open Chessie.ErrorHandling
    open Domain
    open Suave
    open Suave.DotLiquid
    open Suave.Filters
    open Suave.Form
    open Suave.Operators

    let signupTemplatePath = "user/signup.liquid"

    type UserSignupViewModel = {
        Username : string
        Email    : string
        Password : string
        Error    : string option
    } 
    let emptyUserSignupViewModel = {
        Username = ""
        Email    = ""
        Password = ""
        Error    = None
    }

    let handleUserSignup ctx = async {
        match bindEmptyForm ctx.request with
        | Choice1Of2 (vm : UserSignupViewModel) ->
            let result =
                SignupUserRequest.TryCreate (vm.Username, vm.Password, vm.Email)
            let onSuccess (signupUserRequest, _) =
                printfn "%A" signupUserRequest
                Redirection.FOUND "/signup" ctx
            let onFailure msgs =
                let viewModel = {vm with Error = Some (List.head msgs)}
                page signupTemplatePath viewModel ctx
            return! either onSuccess onFailure result
        | Choice2Of2 err ->
            let viewModel = {emptyUserSignupViewModel with Error = Some err}
            return! page signupTemplatePath viewModel ctx
    }

    let webPart () =
        path "/signup"
            >=> choose [
                GET  >=> page signupTemplatePath emptyUserSignupViewModel
                POST >=> handleUserSignup
            ]