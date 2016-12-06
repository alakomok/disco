// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open Fake.ZipHelper
open Fake.FuchuHelper
open Fake.Paket
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO
open System.Diagnostics

let konst x _ = x

/// Copies a directory recursively without any output.
/// If the target directory does not exist, it will be created.
/// ## Parameters
///
///  - `target` - The target directory.
///  - `source` - The source directory.
///  - `filterFile` - A file filter predicate.
let SilentCopyDir target source filterFile =
    CreateDir target
    Directory.GetFiles(source, "*.*", SearchOption.AllDirectories)
    |> Seq.filter filterFile
    |> Seq.iter (fun file ->
           let fi =
               file
               |> replaceFirst source ""
               |> trimSeparator

           let newFile = target @@ fi
          //  logVerbosefn "%s => %s" file newFile
           DirectoryName newFile |> ensureDirectory
           File.Copy(file, newFile, true))
    |> ignore

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "Iris"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "VVVV Automation Infrastructure"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "Lorem Ipsum Dolor Sit Amet"

// List of author names (for NuGet package)
let authors = [ "Karsten Gebbert <karsten@nsynk.de>" ]

// Tags for your project (for NuGet package)
let tags = "cool funky special shiny"

// File system information
let solutionFile  = "Iris.sln"

// Project code base directory
let baseDir =  __SOURCE_DIRECTORY__ @@ "src" @@ "Iris"

let useNix = Directory.Exists("/nix")

let isUnix = Environment.OSVersion.Platform = PlatformID.Unix

let maybeFail = function
  | 0    -> ()
  | code -> failwithf "Command failed with exit code %d" code

let npmPath =
  if File.Exists "/run/current-system/sw/bin/npm" then
    "/run/current-system/sw/bin/npm"
  elif File.Exists "/usr/bin/npm" then
    "/usr/bin/npm"
  elif File.Exists "/usr/local/bin/npm" then
    "/usr/local/bin/npm"
  elif Environment.GetEnvironmentVariable("APPVEYOR_CI_BUILD") = "true" then
    @"C:\Users\appveyor\AppData\Roamiug\npm\node_modules\npm\bin\npm.cmd"
  else
    "npm"

let flatcPath : string =
  if Environment.OSVersion.Platform = PlatformID.Unix then
    let info = new ProcessStartInfo("which","flatc")
    info.StandardOutputEncoding <- System.Text.Encoding.UTF8
    info.RedirectStandardOutput <- true
    info.UseShellExecute        <- false
    info.CreateNoWindow         <- true
    use proc = Process.Start info
    proc.WaitForExit()
    match proc.ExitCode with
      | 0 when not proc.StandardOutput.EndOfStream ->
        proc.StandardOutput.ReadLine()
      | _ -> failwith "flatc was not found. Please install FlatBuffers first"
  else "flatc.exe"

// Read additional information from the release notes document
let release = LoadReleaseNotes "CHANGELOG.md"

let nativeProjects =
  [ baseDir @@ "Nodes.fsproj"
    baseDir @@ "Service.fsproj"
    baseDir @@ "Tests.fsproj" ]

let jsProjects =
  [ baseDir @@ "Frontend.fsproj"
    baseDir @@ "Web.Tests.fsproj" ]

// Helper active pattern for project types
let (|Fsproj|Csproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | _                           ->
      failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)

let setParams cfg defaults =
  { defaults with
      Verbosity = Some(Quiet)
      Targets = ["Build"]
      Properties = [ "Configuration", cfg ] }

let runMono filepath workdir =
  ExecProcess (fun info ->
                  info.FileName <- "mono"
                  info.Arguments <- filepath
                  info.UseShellExecute <- true
                  info.WorkingDirectory <- workdir)
              (TimeSpan.FromMinutes 5.0)
  |> maybeFail

let runExec filepath args workdir shell =
  ExecProcess (fun info ->
                  info.FileName <- filepath
                  info.Arguments <- if String.length args > 0 then args else info.Arguments
                  info.UseShellExecute <- shell
                  info.WorkingDirectory <- workdir)
              TimeSpan.MaxValue
  |> maybeFail

let runExecAndReturn filepath args workdir =
  ExecProcessAndReturnMessages (fun info ->
    info.FileName <- filepath
    info.Arguments <- if String.length args > 0 then args else info.Arguments
    info.UseShellExecute <- false
    info.WorkingDirectory <- workdir) TimeSpan.MaxValue
  |> fun res -> res.Messages |> String.concat "\n"

let runNpm cmd workdir _ =
  let npm =
    match Environment.OSVersion.Platform with
      | PlatformID.Unix ->  "npm" // use the platform npm/node
      | _               ->        // use the nuget/paket npm/node
        __SOURCE_DIRECTORY__ @@  @"\packages\Npm.js\tools\npm.cmd"
  runExec npm cmd workdir false

let runNode cmd workdir _ =
  let node =
    match Environment.OSVersion.Platform with
      | PlatformID.Unix ->  "node" // use the platform npm/node
      | _               ->         // use the nuget/paket npm/node
        __SOURCE_DIRECTORY__ @@  @"\packages\Node.js\node.exe"
  runExec node cmd workdir false

let runFable fableconfigdir extraArgs _ =
  runNpm ("run fable -- " + fableconfigdir + " " + extraArgs) __SOURCE_DIRECTORY__ ()
  // Run Fable's dev version
  // runNode ("../../Fable/build/fable " + fableconfigdir + " " + extraArgs + " --verbose") __SOURCE_DIRECTORY__ ()

let runTestsOnWindows filepath workdir =
  let arch =
    if Environment.Is64BitOperatingSystem
    then "amd64"
    else "i386"

  printfn "Copy %s to %s" (baseDir @@ arch @@ "libzmq.dll") workdir
  CopyFile workdir (baseDir @@ arch @@ "libzmq.dll")

  printfn "Copy %s to %s" (baseDir @@ arch @@ "libzmq.dll") workdir
  CopyFile workdir (baseDir @@ arch @@ "libsodium.dll")

  ExecProcess (fun info ->
                  info.FileName <- (workdir </> filepath)
                  info.UseShellExecute <- false
                  info.WorkingDirectory <- workdir)
              TimeSpan.MaxValue
  |> maybeFail

let buildDebug fsproj _ =
  build (setParams "Debug") (baseDir @@ fsproj)

let buildRelease fsproj _ =
  build (setParams "Release") (baseDir @@ fsproj)

//  ____              _       _
// | __ )  ___   ___ | |_ ___| |_ _ __ __ _ _ __
// |  _ \ / _ \ / _ \| __/ __| __| '__/ _` | '_ \
// | |_) | (_) | (_) | |_\__ \ |_| | | (_| | |_) |
// |____/ \___/ \___/ \__|___/\__|_|  \__,_| .__/
//                                         |_|

Target "Bootstrap" (fun _ ->
  Restore(id)                              // restore Paket packages
  runNpm "install" __SOURCE_DIRECTORY__ () // restore Npm packages
  // runNpm "-g install fable-compiler mocha-phantomjs webpack" __SOURCE_DIRECTORY__ ()
)

//     _                           _     _       ___        __
//    / \   ___ ___  ___ _ __ ___ | |__ | |_   _|_ _|_ __  / _| ___
//   / _ \ / __/ __|/ _ \ '_ ` _ \| '_ \| | | | || || '_ \| |_ / _ \
//  / ___ \\__ \__ \  __/ | | | | | |_) | | |_| || || | | |  _| (_) |
// /_/   \_\___/___/\___|_| |_| |_|_.__/|_|\__, |___|_| |_|_|  \___/
//                                         |___/

Target "AssemblyInfo" (fun _ ->
    let getAssemblyInfoAttributes projectName =
        [ Attribute.Title (projectName)
          Attribute.Product project
          Attribute.Description summary
          Attribute.Version release.AssemblyVersion
          Attribute.FileVersion release.AssemblyVersion ]

    let getProjectDetails projectPath =
        let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath,
          projectName,
          System.IO.Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    let assemblyFileName (fn: string) (suffix: string) =
      fn.Substring(0,fn.Length - 7) + "Info." + suffix

    !! "src/**/*.??proj"
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, projectName, folderName, attributes) ->
        let createPath suffix =
          let filename = assemblyFileName (projFileName.Substring(folderName.Length)) suffix
          (folderName @@ "AssemblyInfo") @@ filename

        match projFileName with
        | Fsproj -> CreateFSharpAssemblyInfo (createPath "fs") attributes
        | Csproj -> CreateCSharpAssemblyInfo (createPath "cs") attributes))

//   ____
//  / ___|___  _ __  _   _
// | |   / _ \| '_ \| | | |
// | |__| (_) | |_) | |_| |
//  \____\___/| .__/ \__, |
//            |_|    |___/

Target "CopyBinaries"
  (fun _ ->
    SilentCopyDir "bin/Iris"  (baseDir @@ "bin/Release/Iris")  (konst true)
    SilentCopyDir "bin/Nodes" (baseDir @@ "bin/Release/Nodes") (konst true))

Target "CopyAssets"
  (fun _ ->
    SilentCopyDir "bin/Iris/assets" (baseDir @@ "assets/frontend") (konst true)

    !! (baseDir @@ "bin/*.js")
    |> CopyFiles "bin/Iris/assets/js"

    !! (baseDir @@ "bin/*.map")
    |> CopyFiles "bin/Iris/assets/js")


//     _             _     _
//    / \   _ __ ___| |__ (_)_   _____
//   / _ \ | '__/ __| '_ \| \ \ / / _ \
//  / ___ \| | | (__| | | | |\ V /  __/
// /_/   \_\_|  \___|_| |_|_| \_/ \___|

let comment = @"
##  ___      _
## |_ _|_ __(_)___
##  | || '__| / __|
##  | || |  | \__ \
## |___|_|  |_|___/ Automation Toolkit
## NsynK GmbH, 2016

Let us do something, while we have the chance! It is not every day that we are
needed. Not indeed that we personally are needed. Others would meet the case
equally well, if not better. To all mankind they were addressed, those cries for
help still ringing in our ears! But at this place, at this moment of time, all
mankind is us, whether we like it or not. Let us make the most of it, before it
is too late! Let us represent worthily for one the foul brood to which a cruel
fate consigned us! What do you say? It is true that when with folded arms we
weigh the pros and cons we are no less a credit to our species. The tiger bounds
to the help of his congeners without the least reflexion, or else he slinks away
into the depths of the thickets. But that is not the question. What are we doing
here, that is the question. And we are blessed in this, that we happen to know
the answer. Yes, in the immense confusion one thing alone is clear. We are
waiting for Godot to come --

"

Target "CreateArchive"
  (fun _ ->
     let nameWithVersion = "Iris-" + release.NugetVersion
     let target = "temp" @@ nameWithVersion

     if Directory.Exists target |> not then
       CreateDir target
     else
       CleanDir target

     CopyDir (target @@ "Iris")  "bin/Iris" (konst true)
     CopyDir (target @@ "Nodes") "bin/Nodes" (konst true)
     let files = !!(target @@ "**")
     CreateZip "temp" (nameWithVersion + ".zip") comment 7 false files)


//   ____ _
//  / ___| | ___  __ _ _ __
// | |   | |/ _ \/ _` | '_ \
// | |___| |  __/ (_| | | | |
//  \____|_|\___|\__,_|_| |_|

Target "Clean" (fun _ ->
    CleanDirs [
      "bin"
      "temp"
      "docs/output"
      "src/Iris/bin"
      "src/Iris/obj"
      ])

//  ____            _       _ _          _   _
// / ___|  ___ _ __(_) __ _| (_)______ _| |_(_) ___  _ __
// \___ \ / _ \ '__| |/ _` | | |_  / _` | __| |/ _ \| '_ \
//  ___) |  __/ |  | | (_| | | |/ / (_| | |_| | (_) | | | |
// |____/ \___|_|  |_|\__,_|_|_/___\__,_|\__|_|\___/|_| |_|

Target "GenerateSerialization"
  (fun _ ->
   printfn "Cleaning up previous"

   DeleteFile (baseDir @@ "Serialization.csproj")
   CleanDirs [ baseDir @@ "Iris/Serialization" ]

   let fbs =
      !! (baseDir @@ "Schema/**/*.fbs")
      |> Seq.map (fun p -> " " + p)
      |> Seq.fold ((+)) ""

   // CSHARP
   let args = "-I " + (baseDir @@ "Schema") + " --csharp " + fbs
   runExec flatcPath args baseDir false

   // JAVASCRIPT
   let args = "-I " + (baseDir @@ "Schema") + " -o " + (baseDir @@ "assets/frontend/js") + " --js " + fbs
   runExec flatcPath args baseDir false

   let files =
      !! (baseDir @@ "Iris/Serialization/**/*.cs")
      |> Seq.map (fun p -> "    <Compile Include=\"" + p + "\" />" + Environment.NewLine)
      |> Seq.fold ((+)) ""

   let top = File.ReadAllText (baseDir @@ "assets/csproj/Serialization.top.xml")
   let bot = File.ReadAllText (baseDir @@ "assets/csproj/Serialization.bottom.xml")

   File.WriteAllText((baseDir @@ "Serialization.csproj"), top + files + bot)

   buildDebug "Serialization.csproj" ()
   buildRelease "Serialization.csproj" ())

//  _____                _                 _
// |  ___| __ ___  _ __ | |_ ___ _ __   __| |
// | |_ | '__/ _ \| '_ \| __/ _ \ '_ \ / _` |
// |  _|| | | (_) | | | | ||  __/ | | | (_| |
// |_|  |_|  \___/|_| |_|\__\___|_| |_|\__,_| JS!

let frontendDir = baseDir @@ "Projects" @@ "Frontend"

Target "BuildFrontend" (fun () ->
  runNpm "install" (baseDir @@ "../..") ()
  runFable frontendDir "" ()
)

//  _____         _
// |_   _|__  ___| |_ ___
//   | |/ _ \/ __| __/ __|
//   | |  __/\__ \ |_\__ \
// JS|_|\___||___/\__|___/
let webtestsdir = baseDir @@ "Projects" @@ "Web.Tests"

Target "BuildWebTests" (fun _ ->
  runNpm "install" __SOURCE_DIRECTORY__ ()
  runFable webtestsdir "" ()
)

Target "WatchWebTests" (runFable webtestsdir "-t watch")

Target "BuildWebTestsFsProj" (buildDebug "Projects/Web.Tests/Web.Tests.fsproj")

Target "RunWebTests" (fun _ ->
  runNpm "install" (baseDir @@ "assets/frontend") ()
  runNpm "run phantomjs -- node_modules/mocha-phantomjs-core/mocha-phantomjs-core.js src/Iris/assets/frontend/tests.html tap" __SOURCE_DIRECTORY__ ()
)
//    _   _ _____ _____
//   | \ | | ____|_   _|
//   |  \| |  _|   | |
//  _| |\  | |___  | |
// (_)_| \_|_____| |_|

Target "BuildDebugCore" (buildDebug "Projects/Core/Core.fsproj")

Target "BuildReleaseCore" (buildRelease "Projects/Core/Core.fsproj")

Target "BuildDebugService" (fun () ->
  buildDebug "Projects/Service/Service.fsproj" ()
  let assetsTargetDir = (baseDir @@ "bin/Debug/Iris/assets")
  FileUtils.cp_r (baseDir @@ "assets/frontend") assetsTargetDir
  runNpm "install" assetsTargetDir ()
)

Target "BuildReleaseService" (fun () ->
  let targetDir = (baseDir @@ "bin/Release/Iris/assets")
  buildRelease "Projects/Service/Service.fsproj" ()
  FileUtils.cp_r (baseDir @@ "assets/frontend") targetDir
  runNpm "install" targetDir ()
)

Target "BuildDebugNodes" (buildDebug "Projects/Nodes/Nodes.fsproj")

Target "BuildReleaseNodes" (buildRelease "Projects/Nodes/Nodes.fsproj")

//  _____         _
// |_   _|__  ___| |_ ___
//   | |/ _ \/ __| __/ __|
//   | |  __/\__ \ |_\__ \
//   |_|\___||___/\__|___/

(*
   Working with libgit2 native libraries:

   - see ldd bin/Debug/NativeBinaries/linux/amd64/libgit...so for dependencies
   - set MONO_LOG_LEVEL=debug for more VM info
   - ln -s bin/Debug/NativeBinaries bin/Debug/libNativeBinaries
   - set LD_LIBRARY_PATH=....:/run/current-system/sw/lib/

   now it *should* work. YMMV.

   Good Fix: use a nix-shell environment that exposes LD_LIBRARY_PATH correctly.
*)

Target "BuildTests" (buildDebug "Projects/Tests/Tests.fsproj")

Target "RunTests"
  (fun _ ->
    let testsDir = baseDir @@ "bin" @@ "Debug" @@ "Tests"

    if isUnix then
      runMono "Iris.Tests.exe" testsDir
    else
      runTestsOnWindows "Iris.Tests.exe" testsDir)

//  ____
// / ___|  ___ _ ____   _____ _ __
// \___ \ / _ \ '__\ \ / / _ \ '__|
//  ___) |  __/ |   \ V /  __/ |
// |____/ \___|_|    \_/ \___|_|

Target "DevServer"
  (fun _ ->
    let info = new ProcessStartInfo("fsi", "DevServer.fsx")
    info.UseShellExecute <- false
    let proc = Process.Start(info)
    proc.WaitForExit()
    printfn "done.")

//  ____
// |  _ \  ___   ___ ___
// | | | |/ _ \ / __/ __|
// | |_| | (_) | (__\__ \
// |____/ \___/ \___|___/

Target "GenerateReferenceDocs"
  (fun _ ->
    let result =
      executeFSIWithArgs
        "docs/tools"
        "generate.fsx"
        ["--define:RELEASE"; "--define:REFERENCE"]
        []

    if not result then
      failwith "generating reference documentation failed")

let generateHelp' fail debug =
  let args =
    if debug then ["--define:HELP"]
    else ["--define:RELEASE"; "--define:HELP"]
  if executeFSIWithArgs "docs/tools" "generate.fsx" args [] then
    traceImportant "Help generated"
  else
    if fail then
      failwith "generating help documentation failed"
    else
      traceImportant "generating help documentation failed"

let generateHelp fail =
  generateHelp' fail false

Target "GenerateHelp" (fun _ ->
  DeleteFile "docs/content/release-notes.md"
  CopyFile "docs/content/" "CHANGELOG.md"
  Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"
  DeleteFile "docs/content/license.md"
  CopyFile "docs/content/" "LICENSE.txt"
  Rename "docs/content/license.md" "docs/content/LICENSE.txt"
  generateHelp true)

Target "GenerateHelpDebug" (fun _ ->
  DeleteFile "docs/content/release-notes.md"
  CopyFile "docs/content/" "CHANGELOG.md"
  Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"
  DeleteFile "docs/content/license.md"
  CopyFile "docs/content/" "LICENSE.txt"
  Rename "docs/content/license.md" "docs/content/LICENSE.txt"
  generateHelp' true true)

Target "KeepRunning" (fun _ ->
  use watcher = !! "docs/content/**/*.*" |> WatchChanges (fun changes -> generateHelp false)
  traceImportant "Waiting for help edits. Press any key to stop."
  System.Console.ReadKey() |> ignore
  watcher.Dispose())

Target "GenerateDocs" DoNothing

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "Release" DoNothing

// "Clean"
// ==> "GenerateSerialization"

// Serialization

"GenerateSerialization"
==> "BuildWebTests"

"GenerateSerialization"
==> "BuildFrontend"

"GenerateSerialization"
==> "BuildReleaseService"

"GenerateSerialization"
==> "BuildReleaseNodes"

// Tests

"GenerateSerialization"
==> "BuildTests"

"BuildTests"
==> "RunTests"

// ONWARDS!

"BuildReleaseNodes"
==> "BuildReleaseService"
==> "CopyBinaries"

// "BuildWebTests"
// ==> "CopyAssets"

"CopyBinaries"
==> "CopyAssets"
==> "CreateArchive"

"CreateArchive"
==> "Release"

"BuildDebugService"
==> "BuildWebTests"
==> "RunWebTests"

//  ____       _                 ____             _
// |  _ \  ___| |__  _   _  __ _|  _ \  ___   ___| | _____ _ __
// | | | |/ _ \ '_ \| | | |/ _` | | | |/ _ \ / __| |/ / _ \ '__|
// | |_| |  __/ |_) | |_| | (_| | |_| | (_) | (__|   <  __/ |
// |____/ \___|_.__/ \__,_|\__, |____/ \___/ \___|_|\_\___|_|
//                         |___/

let getCommitHash() =
  let log = runExecAndReturn "git" "log -n1 --oneline" baseDir
  log.Substring(0, log.IndexOf(" "))

let dockerCreateImage hash workingDir =
  let cmd = sprintf "build --label iris --tag iris:%s ." hash
  runExec "docker" cmd workingDir false

Target "DockerCreateBaseImage" (fun () ->
  runExec "docker" "build --label iris --tag iris:base ../Docker/iris_base" baseDir false
)

Target "DockerRunTests" (fun () ->
  let testsDir = "src/Iris/bin/Debug/Tests"
  FileUtils.cp_r "src/Docker/iris/" testsDir
  let hash = getCommitHash()
  dockerCreateImage hash testsDir
  let irisNodeId = Guid.NewGuid().ToString()
  let img = runExecAndReturn "docker" ("images -q iris:" + hash) baseDir
  let runCmd = sprintf "run -i --rm -e IRIS_NODE_ID=%s -e COMMAND=tests %s" irisNodeId img
  runExec "docker" runCmd  baseDir false
)

"BuildTests"
==> "DockerRunTests"

// let startDockerCmd() =
//   let project, irisNodeId, image = "foo", "foo", "foo"
//   sprintf """run -p 7000:7000 -i --rm -v %s:/project \
//     -e IRIS_NODE_ID=%s -e COMMAND=start %s""" project irisNodeId image

// Target "CreateDocker" (fun () ->
//   FileUtils.cp_r "src/Docker/iris/" "src/Iris/bin/Debug/Iris"
//   runExec "docker" (createDockerCmd()) baseDir false
// )

// Target "StartDockerImage" (fun () ->
//   runExec "docker" (startDockerCmd()) baseDir false
// )

//  ____       _                    _    _ _
// |  _ \  ___| |__  _   _  __ _   / \  | | |
// | | | |/ _ \ '_ \| | | |/ _` | / _ \ | | |
// | |_| |  __/ |_) | |_| | (_| |/ ___ \| | |
// |____/ \___|_.__/ \__,_|\__, /_/   \_\_|_|
//                         |___/

Target "DebugAll" DoNothing

"BuildFrontend"
==> "DebugAll"

"BuildDebugService"
==> "DebugAll"

"BuildDebugNodes"
==> "DebugAll"

// "CleanDocs"
//   ==> "GenerateHelp"
//   ==> "GenerateReferenceDocs"
//   ==> "GenerateDocs"
//
// "CleanDocs"
//   ==> "GenerateHelpDebug"
//
// "GenerateHelp"
//   ==> "KeepRunning"

Target "AllTests" DoNothing

"RunTests"
==> "AllTests"

"RunWebTests"
==> "AllTests"

RunTargetOrDefault "Release"
