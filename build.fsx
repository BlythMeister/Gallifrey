open Fake.AppVeyor

#r    @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open System.IO

let outputDir = currentDirectory @@ "Output"
let srcDir = currentDirectory @@ "src"
let isAppVeyor = buildServer = AppVeyor
let changeLogPath = currentDirectory @@ "src" @@ "Gallifrey.UI.Modern" @@ "ChangeLog.xml"
let branchName = match isAppVeyor with
                 | true-> AppVeyorEnvironment.RepoBranch
                 | _ -> getBranchName currentDirectory

let isStable = branchName = "master"
let isBeta = isStable || branchName = "develop"

printfn "Running On Branch: %s" branchName

let baseVersion = match isAppVeyor with
                  | true -> AppVeyorEnvironment.BuildVersion.Substring(0, AppVeyorEnvironment.BuildVersion.LastIndexOf("."))
                  | _ -> "0.0.0"

let buildNumber = match isAppVeyor with
                  | true -> AppVeyorEnvironment.BuildNumber
                  | _ -> "0"

let versionNumber = match isStable with
                    | true -> sprintf "%s.0" baseVersion                        
                    | _ -> sprintf "%s.%s" baseVersion buildNumber

Target "Clean" (fun _ -> 
    CleanDir outputDir
)

Target "VersionUpdate" (fun _ ->


    BulkReplaceAssemblyInfoVersions "src/" (fun f -> { f with AssemblyVersion = versionNumber; AssemblyInformationalVersion = versionNumber; AssemblyFileVersion = versionNumber })

    
    if isAppVeyor then
        File.WriteAllText(changeLogPath, File.ReadAllText(changeLogPath).Replace("<Version Number=\"0.0.0.0\" Name=\"Pre-Release\">", (sprintf "<Version Number=\"%s\" Name=\"Pre-Release\">" versionNumber)))

        Directory.GetFiles(srcDir, "*proj", SearchOption.AllDirectories)
        |> Seq.iter(fun x -> File.WriteAllText(x, File.ReadAllText(x).Replace("<MinimumRequiredVersion>0.0.0.0</MinimumRequiredVersion>", (sprintf "<MinimumRequiredVersion>%s</MinimumRequiredVersion>" versionNumber))
                                                                     .Replace("<ApplicationVersion>0.0.0.0</ApplicationVersion>", (sprintf "<ApplicationVersion>%s</ApplicationVersion>" versionNumber))))
)

Target "Build" (fun _ ->
    let buildMode = getBuildParamOrDefault "buildMode" "Release"
    let setParams defaults = { defaults with Verbosity = Some(Quiet)
                                             Targets = ["Clean,Rebuild,publish"]
                                             Properties = [
                                                            "Optimize", "True"
                                                            "DebugSymbols", "True"
                                                            "Configuration", buildMode
                                                          ]
                             }

    build setParams (srcDir @@ "Gallifrey.sln")
)

Target "Test" (fun _ ->
    printfn "There are no tests to run yet...."
)

Target "Package" (fun _ ->
    let moveArtifacts releaseType = 
        let source = srcDir @@ (sprintf "Gallifrey.UI.Modern.%s" releaseType) @@ "bin" @@ "Release" @@ "app.publish"
        let destination = outputDir @@ releaseType
        Directory.Move(source, destination)
        CreateZip destination (outputDir @@ (sprintf "%s.zip" releaseType)) "" DefaultZipLevel false (Directory.GetFiles(destination, "*.*", SearchOption.AllDirectories))

    moveArtifacts "Alpha"

    if isBeta then 
        moveArtifacts "Beta"
        File.Copy(outputDir @@ "Beta" @@ "setup.exe", outputDir @@ "beta-setup.exe")

    if isStable then 
        moveArtifacts "Stable"
        File.Copy(outputDir @@ "Stable" @@ "setup.exe", outputDir @@ "stable-setup.exe")
)

Target "Publish" (fun _ ->
    let publishRelease (releaseType:string) = 
        let sourceRoot = outputDir @@ releaseType
        let destinationRoot = currentDirectory @@ "Releases" @@ "download" @@ "modern-temp" @@ (releaseType.ToLower())
        ensureDirectory destinationRoot
        File.Copy(sourceRoot @@ (sprintf "Gallifrey.UI.Modern.%s.application" releaseType), destinationRoot @@ (sprintf "Gallifrey.UI.Modern.%s.application" releaseType), true)

        let destinationFiles = destinationRoot @@ "Application Files"
        ensureDirectory destinationFiles
        Directory.GetDirectories(sourceRoot @@ "Application Files")
        |> Seq.map(fun x -> new DirectoryInfo(x))
        |> Seq.iter(fun x -> Directory.Move(x.FullName, destinationFiles @@ x.Name))
    
    
    PushArtifacts (Directory.GetFiles(outputDir, "*.zip", SearchOption.TopDirectoryOnly))
    PushArtifacts (Directory.GetFiles(outputDir, "*.exe", SearchOption.TopDirectoryOnly))
    
    let releasesRepo = outputDir @@ "Releases"
    DeleteDir releasesRepo |> ignore
    cloneSingleBranch outputDir "https://github.com/BlythMeister/Gallifrey.Releases.git" "master" "Releases"

    publishRelease "Alpha"
    if isBeta then publishRelease "Beta"
    if isStable then publishRelease "Stable"

    StageAll releasesRepo
    Commit releasesRepo (sprintf "Publish - %s" versionNumber)
    push releasesRepo    
)

Target "Default" DoNothing

"Clean"
    ==> "VersionUpdate"
    ==> "Build"
    ==> "Test"
    ==> "Package"
    =?> ("Publish", isAppVeyor)
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"