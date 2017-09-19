open Fake.AppVeyor

#r    @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open System.IO

let outputDir = currentDirectory @@ "Output"
let srcDir = currentDirectory @@ "src"
let isAppVeyor = buildServer = AppVeyor

Target "Clean" (fun _ -> 
    CleanDir outputDir
)

Target "VersionUpdate" (fun _ ->
    let baseVersion = File.ReadAllText (srcDir @@ "CurrentVersion.info")
    let versionNumber = match getBranchName currentDirectory, isAppVeyor with
                        | _, false -> "0.0.0.0"
                        | "master", _ -> sprintf "%s.0" baseVersion                        
                        | _, _ -> sprintf "%s.%s" baseVersion AppVeyorEnvironment.BuildNumber

    BulkReplaceAssemblyInfoVersions "src/" (fun f -> { f with AssemblyVersion = versionNumber; AssemblyInformationalVersion = versionNumber; AssemblyFileVersion = versionNumber })
)

Target "Build" DoNothing
Target "Test" DoNothing
Target "Package" DoNothing
Target "Publish" DoNothing
Target "Default" DoNothing

"Clean"
    ==> "VersionUpdate"
    ==> "Build"
    ==> "Test"
    ==> "Package"
    ==> "Publish"
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"