// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake

// Directories
let buildDir  = "./build/"
let deployDir = "./deploy/"


// Filesets
let appReferences  =
    !! "/**/*.csproj"
    ++ "/**/*.fsproj"

// version info
let version = "0.1"  // or retrieve from CI server
let noFilter = fun _ -> true
// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

let copyToBuildDir srcDir targetDirName =
  let targetDir = combinePaths buildDir targetDirName
  CopyDir targetDir srcDir noFilter
Target "Assets" (fun _ ->
  copyToBuildDir "./src/FsTweet.Web/assets" "assets"
)

Target "Build" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
    |> Log "AppBuild-Output: "
)

Target "Views" (fun _ ->
  let srcDir = "./src/FsTweet.Web/views"
  let targetDir = combinePaths buildDir "views"
  CopyDir targetDir srcDir noFilter
)

Target "Run" (fun _ ->
  ExecProcess
    (fun info -> info.FileName <- "./build/FsTweet.Web.exe")
    (System.TimeSpan.FromDays 1.)
  |> ignore
)

// Build order
"Clean"
  ==> "Build"
  ==> "Views"
  ==> "Assets"
  ==> "Run"

// start build
RunTargetOrDefault "Build"
