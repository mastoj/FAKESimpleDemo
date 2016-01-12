@echo off
cls
NuGet.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
NuGet.exe "Install" "OctopusTools" "-OutputDirectory" "packages" "-ExcludeVersion"
NuGet.exe "Install" "Node.js" "-OutputDirectory" "packages" "-ExcludeVersion"
SET NodeJsPath=%~dp0packages\Node.js
SET PATH=%PATH%;%NodeJsPath%
NuGet.exe "Install" "Npm.js" "-OutputDirectory" "packages" "-ExcludeVersion"
"packages\FAKE\tools\Fake.exe" build.fsx %*
