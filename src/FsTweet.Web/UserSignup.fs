namespace UserSignup
module Suave =
    open Suave
    open Suave.Filters
    open Suave.Operators
    open Suave.DotLiquid

    type UserSignupViewModel = {
        Username : string
        Email : string
        Password: string
        Error : string option
    } 
    let emptyUserSignupViewModel = {
        Username = ""
        Email = ""
        Password = ""
        Error = None
    }

    let handleUserSignup ctx = async {
        printfn "%A" ctx.request.form
        return! Redirection.FOUND "/signup" ctx
    }

    let webPart () =
        path "/signup"
            >=> choose [
                GET  >=> page "user/signup.liquid" emptyUserSignupViewModel
                POST >=> handleUserSignup
            ]