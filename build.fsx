// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake

let buildDir = "./build/"

Target "Clean" (fun() ->
  trace "Cleaing your world!"
  CleanDir buildDir
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

"Clean"
  ==> "Web"
  ==> "Default"

// start build
RunTargetOrDefault "Default"
