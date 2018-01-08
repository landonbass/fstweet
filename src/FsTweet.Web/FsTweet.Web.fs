module MyWebApi.Program

open Suave
open Suave.DotLiquid
open Suave.Files
open Suave.Filters
open Suave.Operators
open Suave.Successful
open System.IO
open System.Reflection

let currentPath =
    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

let initDotLiquid () =
    let templatesDir = Path.Combine(currentPath, "views")
    setTemplatesDir templatesDir

let serveAssets =
    let faviconPath =
        Path.Combine(currentPath, "assets", "images", "favicon.ico")
    choose [
        pathRegex "/assets/*" >=> browseHome
        path "/favicon.ico" >=> file faviconPath
    ]

[<EntryPoint>]
let main argv =
    initDotLiquid ()
    setCSharpNamingConvention ()
    let app =
        choose [
            serveAssets
            path "/" >=> page "guest/home.liquid" ""
        ]
    startWebServer defaultConfig app
    0
