// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing

let buildDir = "./.build/"
let deployDir = "./.deploy/"
let testDir = "./.test/"

Target "Clean" (fun() ->
  trace "Cleaing your world!"
  CleanDirs [buildDir; deployDir]
)

let projects = !! "src/FAKESimple.Web/*.csproj" -- "src/**/*.Tests.csproj"
let testProjects = !! "src/**/*.Tests.csproj"

Target "Build" (fun() ->
  trace "Building again!"
  projects
  |> MSBuildRelease buildDir "ResolveReferences;_CopyWebApplication"
  |> ignore
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
)

Target "Default" (fun _ ->
    trace "Building default"
)

"Clean"
==> "Build"
==> "BuildTest"
==> "Test"
==> "Default"
==> "Web"
==> "Package"

// start build
RunTargetOrDefault "Default"
