// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"
#r "./packages/FAKE/tools/Fake.FluentMigrator.dll"
#r "./packages/Npgsql/lib/net451/Npgsql.dll"

open Fake
open Fake.FluentMigratorHelper

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

let connString =
  @"Server=127.0.0.1;Port=5432;Database=FsTweet;User Id=postgres;Password=test;"
let dbConnection = ConnectionString (connString, DatabaseProvider.PostgreSQL)
let migrationsAssembly =
  combinePaths buildDir "FsTweet.Db.Migrations.dll"

Target "RunMigrations" (fun _ ->
  MigrateToLatest dbConnection [migrationsAssembly] DefaultMigrationOptions
)
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
  !! "src/FsTweet.Web/*.fsproj"
  |> MSBuildDebug buildDir "Build"
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

Target "BuildMigrations" (fun _ ->
  !! "src/FsTweet.Db.Migrations/*.fsproj"
  |> MSBuildDebug buildDir "Build"
  |> Log "MigrationBuild-Output: "
)

// Build order
"Clean"
  ==> "Build"
  ==> "Views"
  ==> "Assets"
  ==> "Run"

// start build
RunTargetOrDefault "Build"
