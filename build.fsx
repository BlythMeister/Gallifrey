#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open Fake.AppVeyor
open System
open System.IO

let outputDir = currentDirectory @@ "Output"
let srcDir = currentDirectory @@ "src"
let isAppVeyor = buildServer = AppVeyor
let changeLogPath = currentDirectory @@ "src" @@ "Gallifrey.UI.Modern" @@ "ChangeLog.xml"
let branchName = match isAppVeyor with
                 | true-> AppVeyorEnvironment.RepoBranch
                 | _ -> getBranchName currentDirectory
                 
let isPR = match isAppVeyor with
           | true-> not(String.IsNullOrWhiteSpace AppVeyorEnvironment.PullRequestNumber)
           | _ -> false
                 
let isStable = branchName = "master"
let isBeta = (isStable || branchName = "release")
let isAlpha = (isBeta || branchName = "develop")

printfn "Running On Branch: %s" branchName
printfn "PR Number: %s" AppVeyorEnvironment.PullRequestNumber
printfn "IsPR: %b" isPR
printfn "isStable: %b" isStable
printfn "isBeta: %b" isBeta
printfn "isAlpha: %b" isAlpha

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
    printfn "Clean & Ensure Output Directory"
    CleanDir outputDir
)

Target "VersionUpdate" (fun _ ->
    printfn "Update Assembly Info Versions"
    BulkReplaceAssemblyInfoVersions "src/" (fun f -> { f with AssemblyVersion = versionNumber; AssemblyInformationalVersion = versionNumber; AssemblyFileVersion = versionNumber })
    
    printfn "Update Change Log Versions"
    File.WriteAllText(changeLogPath, File.ReadAllText(changeLogPath).Replace("<Version Number=\"0.0.0.0\" Name=\"Pre-Release\">", (sprintf "<Version Number=\"%s\" Name=\"Pre-Release\">" versionNumber)))

    printfn "Update Click-Once Settings Versions"
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

Target "Package" (fun _ ->
    let moveArtifacts releaseType = 
        let source = srcDir @@ (sprintf "Gallifrey.UI.Modern.%s" releaseType) @@ "bin" @@ "Release" @@ "app.publish"
        let destination = outputDir @@ releaseType
        Directory.Move(source, destination)
        CreateZip destination (outputDir @@ (sprintf "%s.zip" releaseType)) "" DefaultZipLevel false (Directory.GetFiles(destination, "*.*", SearchOption.AllDirectories))

    if isAlpha then
        moveArtifacts "Alpha"

    if isBeta then
        moveArtifacts "Beta"
        File.Copy(outputDir @@ "Beta" @@ "setup.exe", outputDir @@ "beta-setup.exe")

    if isStable then
        moveArtifacts "Stable"
        File.Copy(outputDir @@ "Stable" @@ "setup.exe", outputDir @@ "stable-setup.exe")
)

Target "Publish-Artifacts" (fun _ ->
    PushArtifacts (Directory.GetFiles(outputDir, "*.zip", SearchOption.TopDirectoryOnly))
    PushArtifacts (Directory.GetFiles(outputDir, "*.exe", SearchOption.TopDirectoryOnly))
)

Target "Publish-Release" (fun _ ->    
    let releasesRepo = outputDir @@ "Releases"

    //Hide process tracing so the access token doesn't show
    printfn "Downloading Releases Repo from GitHub"
    enableProcessTracing <- false
    let authKey = environVar "access_token"
    cloneSingleBranch outputDir (sprintf "https://%s:x-oauth-basic@github.com/BlythMeister/Gallifrey.Releases.git" authKey) "master" "Releases"
    enableProcessTracing <- true

    directRunGitCommandAndFail releasesRepo "config --global user.email \"publish@gallifreyapp.co.uk\""
    directRunGitCommandAndFail releasesRepo "config --global user.name \"Gallifrey Auto Publish\""

    let publishRelease (releaseType:string) =         
        let destinationRoot = releasesRepo @@ "download" @@ "modern" @@ (releaseType.ToLower())
        let destinationAppFilesRoot = destinationRoot @@ "Application Files"

        ensureDirectory destinationRoot
        match isStable, releaseType with
        | true, "Alpha" 
        | true, "Beta" -> CleanDir destinationAppFilesRoot
        | _, _ -> ensureDirectory destinationAppFilesRoot 
       
        let destinationAppFiles = destinationAppFilesRoot @@ (sprintf "Gallifrey.UI.Modern.Alpha_%s" (versionNumber.Replace(".","_")))

        if not(Directory.Exists destinationAppFiles) then
            printfn "Publish Relase Type: %s" releaseType
            let sourceRoot = outputDir @@ releaseType

            let sourceAppFiles = sourceRoot @@ "Application Files" @@ (sprintf "Gallifrey.UI.Modern.Alpha_%s" (versionNumber.Replace(".","_")))
            File.Copy(sourceRoot @@ (sprintf "Gallifrey.UI.Modern.%s.application" releaseType), destinationRoot @@ (sprintf "Gallifrey.UI.Modern.%s.application" releaseType), true)
            Directory.Move(sourceAppFiles, destinationAppFiles)
            
            StageAll releasesRepo
            Commit releasesRepo (sprintf "Publish %s - %s" releaseType versionNumber)
            if isStable then pushTag currentDirectory "origin" baseVersion
                           
    if isAlpha then publishRelease "Alpha"
    if isBeta then publishRelease "Beta"
    if isStable then publishRelease "Stable"

    pushBranch releasesRepo "origin" "master"   
)

Target "Default" DoNothing

"Clean"
    ==> "VersionUpdate"
    ==> "Build"
    ==> "Package"
    =?> ("Publish-Artifacts", isAppVeyor)
    =?> ("Publish-Release", isAppVeyor && not(isPR) && (isAlpha || isBeta || isStable))
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"
