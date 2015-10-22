// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing
open Fake.AppVeyor
open Fake.NuGetHelper
open Fake.OctoTools
open System.IO

module Npm =
  open System

  type NpmCommand =
    | Install of string

  type NpmParams = {
    Src: string
    ToolPath: string
    WorkingDirectory: string
    Command: NpmCommand
    Timeout: TimeSpan
  }

  let npmParams = {
    Src = "";
    ToolPath = "";
    Command = (Install "");
    WorkingDirectory = ".";
    Timeout = TimeSpan.MaxValue
  }

  let parse command =
    match command with
    | Install str -> sprintf "install %s" str

  let run npmParams =
    let npmPath = npmParams.ToolPath @@ "npm.cmd"
    let arguments = npmParams.Command |> parse
    let result = ExecProcess (
                  fun info ->
                    info.FileName <- npmPath
                    info.WorkingDirectory <- npmParams.WorkingDirectory
                    info.Arguments <- arguments
                  ) npmParams.Timeout
    if result <> 0 then failwith (sprintf "'npm %s' failed" arguments)

  let Npm f =
    npmParams |> f |> run

open Npm

let buildDir = "./.build/"
let packagingDir = buildDir + "FAKESimple.Web/_PublishedWebsites/FAKESimple.Web"
let deployDir = "./.deploy/"
let testDir = "./.test/"
let projects = !! "src/**/*.csproj" -- "src/**/*.Tests.csproj"
let testProjects = !! "src/**/*.Tests.csproj"
let packages = !! "./**/packages.config"

let getOutputDir proj =
  let folderName = Directory.GetParent(proj).Name
  sprintf "%s%s/" buildDir folderName

let build proj =
  let outputDir = proj |> getOutputDir
  MSBuildRelease outputDir "ResolveReferences;Build" [proj] |> ignore

Target "Clean" (fun() ->
  trace "Cleaing your world!"
  CleanDirs [buildDir; deployDir; testDir]
)

Target "RestorePackages" (fun _ ->
  packages
  |> Seq.iter (RestorePackage (fun p -> {p with OutputPath = "./src/packages"}))
)

Target "Build" (fun() ->
  trace "Building again!"

  projects
  |> Seq.iter build
)

Target "BuildTest" (fun() ->
  trace "Building the tests again!"
  testProjects
  |> MSBuildDebug testDir "Build"
  |> ignore
)

Target "Test" (fun() ->
  trace "Testing your stuff!"
  !! (testDir + "/*.Tests.dll")
      |> xUnit2 (fun p ->
          {p with
              ShadowCopy = false;
              HtmlOutputPath = Some (testDir @@ "xunit.html");
              XmlOutputPath = Some (testDir @@ "xunit.xml");
          })
)

// Default target
Target "Web" (fun _ ->
  Npm (fun p ->
    { p with
        Command = (Install "")
        ToolPath = "C:/Program Files/nodejs/"
        WorkingDirectory = "./src/FAKESimple.Web/"
    })

//  let result =
//          ExecProcess (fun info ->
//              info.FileName <- "npm.cmd"
//              info.Arguments <- "install ./src/FAKESimple.Web/"
//              info.WorkingDirectory <- "."
//          ) (System.TimeSpan.FromMinutes 1.0)
//  if result <> 0 then failwith "Operation failed or timed out"
  trace "Hello World from FAKE"
)

let getVersion() =
  let buildCandidate = (environVar "APPVEYOR_BUILD_NUMBER")
  if buildCandidate = "" || buildCandidate = null then "1.0.0" else (sprintf "1.0.0.%s" buildCandidate)

Target "Package" (fun _ ->
  trace "Packing the web"
  let version = getVersion()
  NuGet (fun p ->
        {p with
            Authors = ["Tomas Jansson"]
            Project = "FAKESimple.Web"
            Description = "Demoing FAKE"
            OutputPath = deployDir
            Summary = "Does this work"
            WorkingDir = packagingDir
            Version = version
            Publish = false })
            (packagingDir + "/FAKESimple.Web.nuspec")
)

let execOnAppveyor arguments =
  let result =
    ExecProcess (fun info ->
      info.FileName <- "appveyor"
      info.Arguments <- arguments
      ) (System.TimeSpan.FromMinutes 2.0)
  if result <> 0 then failwith (sprintf "Failed to execute appveyor command: %s" arguments)
  trace "Published packages"

let publishOnAppveyor folder =
  !! (folder + "*.nupkg")
  |> Seq.iter (fun artifact -> execOnAppveyor (sprintf "PushArtifact %s" artifact))

Target "Publish" (fun _ ->
  match buildServer with
  | BuildServer.AppVeyor ->
      publishOnAppveyor deployDir
  | _ -> ()
)


let executeOcto command =
  let serverName = environVar "OCTO_SERVER"
  let apiKey = environVar "OCTO_KEY"
  let server = { Server = serverName; ApiKey = apiKey }
  Octo (fun octoParams ->
      { octoParams with
          ToolPath = "./packages/octopustools"
          Server   = server
          Command  = command }
  )

Target "Create release" (fun _ ->
  let version = getVersion()
  let release = CreateRelease({ releaseOptions with Project = "FAKESimple.Web"; Version = version }, None)
  executeOcto release
)

Target "Deploy" (fun _ ->
  let version = getVersion()
  let deploy = DeployRelease({deployOptions with Project = "FAKESimple.Web"; Version = version; DeployTo = "Prod"})
  executeOcto deploy
)

Target "Default" (fun _ ->
  trace "Done"
  ()
)

"Web"
==> "Clean"
==> "RestorePackages"
==> "Build"
==> "BuildTest"
==> "Test"
==> "Package"
==> "Publish"
==> "Create release"
==> "Deploy"
==> "Default"

Target "TryOcto" (fun _ ->
  let deploy = DeployRelease({deployOptions with Project = "FAKESimple.Web"; Version = "1.0.0.23"})
  executeOcto deploy
)

// start build
RunTargetOrDefault "Build"
//New-AzurePublicIpAddress -Name connecttointernet -ResourceGroupName FakeAppveyorDemo -DomainNameLabel "fakeocto" -Location "North Europe" -AllocationMethod Dynamic
