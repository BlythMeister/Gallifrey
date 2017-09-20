open Fake.AppVeyor

#r    @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open System.IO

let outputDir = currentDirectory @@ "Output"
let srcDir = currentDirectory @@ "src"
let isAppVeyor = buildServer = AppVeyor
let changeLogPath = currentDirectory @@ "src" @@ "Gallifrey.UI.Modern" @@ "ChangeLog.xml"
let branchName = getBranchName currentDirectory

Target "Clean" (fun _ -> 
    CleanDir outputDir
)

Target "VersionUpdate" (fun _ ->
    let baseVersion = match isAppVeyor with
                      | true -> AppVeyorEnvironment.BuildVersion.Substring(0, AppVeyorEnvironment.BuildVersion.LastIndexOf("."))
                      | _ -> "3.6.0"

    let buildNumber = match isAppVeyor with
                      | true -> AppVeyorEnvironment.BuildNumber
                      | _ -> "6"

    let versionNumber = match branchName with
                        | "master" -> sprintf "%s.0" baseVersion                        
                        | _ -> sprintf "%s.%s" baseVersion buildNumber

    BulkReplaceAssemblyInfoVersions "src/" (fun f -> { f with AssemblyVersion = versionNumber; AssemblyInformationalVersion = versionNumber; AssemblyFileVersion = versionNumber })
    File.WriteAllText(changeLogPath, File.ReadAllText(changeLogPath).Replace("<Version Number=\"0.0.0.0\" Name=\"Pre-Release\">", (sprintf "<Version Number=\"%s\" Name=\"Pre-Release\">" versionNumber)))
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