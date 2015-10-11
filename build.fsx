// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing
open Fake.NuGetHelper
open System.IO

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
  let result =
          ExecProcess (fun info ->
              info.FileName <- "node.exe"
              info.Arguments <- "build.js"
          ) (System.TimeSpan.FromMinutes 1.0)
  if result <> 0 then failwith "Operation failed or timed out"
  trace "Hello World from FAKE"
)

Target "Package" (fun _ ->
  trace "Packing the web"
  let versionCandidate = (environVar "version")
  let buildCandidate = (environVar "APPVEYOR_BUILD_NUMBER")
  let version = if buildCandidate = "" || buildCandidate = null then "1.0.0" else (sprintf "1.0.0.%s" buildCandidate)
  NuGet (fun p ->
        {p with
            Authors = ["Tomas Jansson"]
            Project = "FAKESimple.Demo"
            Description = "Demoing FAKE"
            OutputPath = deployDir
            Summary = "Does this work"
            WorkingDir = packagingDir
            Version = version
            Publish = false })
            (packagingDir + "/FAKESimple.Web.nuspec")
)

Target "Default" (fun _ ->
    trace "Building default"
)

"Clean"
==> "RestorePackages"
==> "Build"
==> "BuildTest"
==> "Test"
==> "Default"
==> "Package"
==> "Web"
// start build
RunTargetOrDefault "Build"
