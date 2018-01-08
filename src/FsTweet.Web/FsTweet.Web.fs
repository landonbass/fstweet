module MyWebApi.Program

open Suave
open Suave.Successful
open Suave.DotLiquid
open Suave.Operators
open Suave.Filters
open System.IO
open System.Reflection

let currentPath =
    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

let initDotLiquid () =
    let templatesDir = Path.Combine(currentPath, "views")
    setTemplatesDir templatesDir

[<EntryPoint>]
let main argv =
    initDotLiquid ()
    setCSharpNamingConvention ()
    let app =
        path "/" >=> page "guest/home.liquid" ""
    startWebServer defaultConfig (OK "Hello World!")
    0
