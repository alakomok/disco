module Iris.Service.CommandActions

#if !IRIS_NODES

open System
open System.IO
open FSharpx.Functional
open Iris.Raft
open Iris.Core
open Iris.Core.Commands
open Iris.Core.FileSystem
open Iris.Service.Interfaces
open Iris.Service.Persistence
open System.Collections.Concurrent
open Iris.Core

type private Channel = AsyncReplyChannel<Either<IrisError,string>>

let private tag s = "Iris.Service.Commands." + s

let private serializeJson =
    let converter = Fable.JsonConverter()
    fun (o: obj) -> Newtonsoft.Json.JsonConvert.SerializeObject(o, converter)

let getServiceInfo (iris: IIrisServer): Either<IrisError,string> =
    match iris.Config with
    | Left _ -> null |> serializeJson
    | Right cfg ->
        match Config.findMember cfg cfg.Machine.MachineId with
        | Right mem ->
          { webSocket = sprintf "ws://%O:%i" mem.IpAddr mem.WsPort
            version = Build.VERSION
            buildNumber = Build.BUILD_NUMBER }
          |> serializeJson
        | Left _ -> null |> serializeJson
    |> Either.succeed

let listProjects (cfg: IrisMachine): Either<IrisError,string> =
  Directory.getDirectories cfg.WorkSpace
  |> Array.choose (fun dir ->
    match IrisProject.Load(dir, cfg) with
    | Right project -> NameAndId(unwrap project.Name, project.Id) |> Some
    | Left _ -> None)
  |> serializeJson
  |> Either.succeed

/// ## buildProject
///
/// Create a new IrisProject data structure with given parameters.
///
/// ### Signature:
/// - name: Name of the Project
/// - path: destination path of the Project
/// - raftDir: Raft data directory
/// - mem: self Member (built from Member Id env var)
///
/// Returns: IrisProject
let buildProject (machine: IrisMachine)
                  (name: string)
                  (path: FilePath)
                  (raftDir: FilePath)
                  (mem: RaftMember) =
  either {
    let! project = Project.create path name machine

    let site =
        let def = ClusterConfig.Default
        { def with Members = Map.add mem.Id mem def.Members }

    let updated =
      project
      |> Project.updateDataDir raftDir
      |> fun p -> Project.updateConfig (Config.addSiteAndSetActive site p.Config) p

    let! _ = Asset.saveWithCommit path User.Admin.Signature updated

    printfn "project: %A" project.Name
    printfn "created in: %O" project.Path

    return updated
  }

/// ## initializeRaft
///
/// Given the user (usually the admin user) and Project value, initialize the Raft intermediate
/// state in the data directory and commit the result to git.
///
/// ### Signature:
/// - user: User to commit as
/// - project: IrisProject to initialize
///
/// Returns: unit
let initializeRaft (project: IrisProject) = either {
    let! raft = createRaft project.Config
    let! _ = saveRaft project.Config raft
    return ()
  }

let createProject (machine: IrisMachine) (opts: CreateProjectOptions) = either {
    let dir = machine.WorkSpace </> filepath opts.name
    let raftDir = dir </> filepath RAFT_DIRECTORY

    // TODO: Throw error instead?
    do!
      if Directory.exists dir
      then rmDir dir
      else Either.nothing

    do! mkDir dir
    do! mkDir raftDir

    let mem =
      { Member.create(machine.MachineId) with
          IpAddr  = IpAddress.Parse opts.ipAddr
          GitPort = opts.gitPort
          WsPort  = opts.wsPort
          ApiPort = opts.apiPort
          Port    = opts.port }

    let! project = buildProject machine opts.name dir raftDir mem

    do! initializeRaft project

    return "ok"
  }

let getProjectSites machine projectName username password =
  either {
    let! path = Project.checkPath machine projectName
    let! (state: State) = Asset.loadWithMachine path machine
    // TODO: Check username and password?
    return state.Project.Config.Sites |> Array.map (fun x -> x.Name) |> serializeJson
  }

// Command to test:
// curl -H "Content-Type: application/json" \
//      -XPOST \
//      -d '{"CloneProject":["meh","git://192.168.2.106:6000/meh/.git"]}' \
//      http://localhost:7000/api/command

let cloneProject (name: string) (uri: string) =
  let machine = MachineConfig.get()
  let target = machine.WorkSpace </> filepath name
  Git.Repo.clone target uri
  |> Either.map (sprintf "Cloned project %A into %A" name target |> konst)

// Command to test:
// curl -H "Content-Type: application/json" \
//      -XPOST \
//      -d '{"PullProject":["dfb6eff5-e4b8-465d-9ad0-ee58bd508cad","meh","git://192.168.2.106:6000/meh/.git"]}' \
//      http://localhost:7000/api/command

let pullProject (id: string) (name: string) (uri: string) = either {
    let machine = MachineConfig.get()
    let target = machine.WorkSpace </> filepath name
    let! repo = Git.Repo.repository target

    let! remote =
      match Git.Config.tryFindRemote repo (string id) with
      | Some remote -> Git.Config.updateRemote repo remote uri
      | None -> Git.Config.addRemote repo (string id) uri

    let! result = Git.Repo.pull repo remote User.Admin.Signature

    match result.Status with
    | LibGit2Sharp.MergeStatus.Conflicts ->
      return!
        "Clonflict while pulling from " + uri
        |> Error.asGitError "pullProject"
        |> Either.fail
    | _ -> return "ok"
  }

let registeredServices = ConcurrentDictionary<string, IDisposable>()

let startAgent (cfg: IrisMachine) (iris: IIrisServer) =
  let fail cmd msg =
    IrisError.Other (tag cmd, msg) |> Either.fail
  MailboxProcessor<Command*Channel>.Start(fun agent ->
    let rec loop() = async {
      let! input, replyChannel = agent.Receive()
      let res =
        match input with
        | Shutdown ->
          let msg = "Disposing service..."
          // TODO: Grab a reference of the http server to dispose it too?
          Async.Start <| async {
            do! Async.Sleep 1000
            printfn "%s" msg
            dispose iris
            exit 0
          }
          Right msg
        | UnloadProject ->
          // TODO: Check if a project is actually loaded
          iris.UnloadProject()
          |> Either.map (fun () -> "Project unloaded")
        | ListProjects -> listProjects cfg
        | GetServiceInfo -> getServiceInfo iris
        | CreateProject opts -> createProject cfg opts
        | CloneProject (name, gitUri) -> cloneProject name gitUri
        | PullProject (id, name, gitUri) -> pullProject id name gitUri
        | LoadProject(projectName, username, password, site) ->
          iris.LoadProject(projectName, username, password, ?site=site)
          |> Either.map (fun _ -> "Loaded project " + projectName)
        | GetProjectSites(projectName, username, password) ->
          getProjectSites cfg projectName username password

      replyChannel.Reply res
      do! loop()
    }
    loop()
  )

let postCommand (agent: (MailboxProcessor<Command*Channel> option) ref) (cmd: Command) =
  let err msg =
    Error.asOther (tag "postCommand") msg |> Either.fail
  match !agent with
  | Some agent ->
    async {
      let! res = agent.PostAndTryAsyncReply((fun ch -> cmd, ch), Constants.COMMAND_TIMEOUT)
      match res with
      | Some res -> return res
      | None -> return err "Request has timeout"
    }
  | None -> err "Command agent hasn't been initialized yet" |> async.Return

#endif
