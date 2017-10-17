module rec Iris.Web.Lib

// Helper methods to be used from JS

open System
open System.Collections.Generic
open Iris.Raft
open Iris.Core
open Iris.Web.Notifications
open Iris.Web.Core
open Iris.Core.Commands
open Fable.Core
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop
open Fable.Import

let inline replaceById< ^t when ^t : (member Id : IrisId)> (newItem : ^t) (ar: ^t[]) =
  Array.map (fun (x: ^t) -> if (^t : (member Id : IrisId) newItem) = (^t : (member Id : IrisId) x) then newItem else x) ar

let insertAfter (i: int) (x: 't) (xs: 't[]) =
  let len = xs.Length
  if len = 0 (* && i = 0 *) then
    [|x|]
  elif i >= len then
    failwith "Index out of array bounds"
  elif i < 0 then
    Array.append [|x|] xs
  elif i = (len - 1) then
    Array.append xs [|x|]
  else
    let xs2 = Array.zeroCreate<'t> (len + 1)
    for j = 0 to len do
      if j <= i then
        xs2.[j] <- xs.[j]
      elif j = (i + 1) then
        xs2.[j] <- x
      else
        xs2.[j] <- xs.[j - 1]
    xs2

let alert msg (_: Exception) =
  Browser.window.alert("ERROR: " + msg)

/// Works like function composition but the second operand
/// is a value, and the result of the first function is ignored
let (&>) fst v =
  fun x -> fst x; v

let private postCommandPrivate (ipAndPort: string option) (cmd: Command) =
  let url =
    match ipAndPort with
    | Some ipAndPort -> sprintf "http://%s%s" ipAndPort Constants.WEP_API_COMMAND
    | None -> Constants.WEP_API_COMMAND
  GlobalFetch.fetch(
    RequestInfo.Url url,
    requestProps
      [ RequestProperties.Method HttpMethod.POST
        requestHeaders [ContentType "application/json"]
        RequestProperties.Body (toJson cmd |> U3.Case3) ])

let postCommand onSuccess onFail (cmd: Command) =
  postCommandPrivate None cmd
  |> Promise.bind (fun res ->
    if not res.Ok
    then res.text() |> Promise.map onFail
    else res.text() |> Promise.map onSuccess)

let postCommandAndBind onSuccess onFail (cmd: Command) =
  postCommandPrivate None cmd
  |> Promise.bind (fun res ->
    if not res.Ok
    then res.text() |> Promise.bind onFail
    else res.text() |> Promise.bind onSuccess)

/// Posts a command, parses the JSON response returns a promise (can fail)
[<PassGenerics>]
let postCommandParseAndContinue<'T> (ipAndPort: string option) (cmd: Command) =
  postCommandPrivate ipAndPort cmd
  |> Promise.bind (fun res ->
    if res.Ok
    then res.text() |> Promise.map ofJson<'T>
    else res.text() |> Promise.map (failwithf "%s"))

let postCommandWithErrorNotifier defValue onSuccess cmd =
  postCommand onSuccess (fun msg -> Notifications.error msg; defValue) cmd

let postCommandAndForget cmd =
  postCommand ignore Notifications.error cmd

let listProjects() =
  ListProjects
  |> postCommandWithErrorNotifier [||] (ofJson<NameAndId[]> >> Array.map (fun x -> x.Name))

let addMember(memberIpAddr: string, memberHttpPort: uint16) =
  Promise.start (promise {
  // See workflow: https://bitbucket.org/nsynk/iris/wiki/md/workflows.md
  try
    let latestState =
      match ClientContext.Singleton.Store with
      | Some store -> store.State
      | None -> failwith "The client store is not initialized"

    let memberIpAndPort =
      sprintf "%s:%i" memberIpAddr memberHttpPort |> Some

    memberIpAndPort |> Option.iter (printfn "New member URI: %s")

    let! status =
      postCommandParseAndContinue<MachineStatus>
        memberIpAndPort
        MachineStatus

    match status with
    | Busy (_, name) -> failwithf "Host cannot be added. Busy with project %A" name
    | _ -> ()

    // Get the added machines configuration
    let! machine =
      postCommandParseAndContinue<IrisMachine>
        memberIpAndPort
        MachineConfig

    // List projects of member candidate (B)
    let! projects =
      postCommandParseAndContinue<NameAndId[]>
        memberIpAndPort
        ListProjects

    // If B has leader (A) active project,
    // then **pull** project from A into B
    // else **clone** active project from A into B

    let! commandMsg =
      let projectGitUri =
        match Project.localRemote latestState.Project with
        | Some uri -> uri
        | None -> failwith "Cannot get URI of project git repository"
      match projects |> Array.tryFind (fun p -> p.Id = latestState.Project.Id) with
      | Some p -> PullProject(p.Id, latestState.Project.Name, projectGitUri)
      | None   -> CloneProject(latestState.Project.Name, projectGitUri)
      |> postCommandParseAndContinue<string> memberIpAndPort

    Notifications.info commandMsg

    let active =
      latestState.Project.Config.ActiveSite
      |> Option.map (fun id -> { Id = id; Name = name "<unknown>" })

    // Load active project in machine B
    // Note that we don't use loadProject from below, since that function
    // restarts the ClientContextn and thus disconnects us from the service.

    // TODO: Using the admin user for now, should it be the same user as leader A?
    let! loadResult =
      LoadProject(latestState.Project.Name, name "admin", password "Nsynk", active)
      |> postCommandPrivate memberIpAndPort

    printfn "response: %A" loadResult

    // Add member B to the leader (A) cluster
    { Member.create machine.MachineId with
        HostName = machine.HostName
        IpAddr   = machine.BindAddress
        Port     = machine.RaftPort
        WsPort   = machine.WsPort
        GitPort  = machine.GitPort
        ApiPort  = machine.ApiPort }
    |> AddMember
    |> ClientContext.Singleton.Post // TODO: Check the state machine post has been successful
  with
  | exn ->
    exn.Message
    |> sprintf "Cannot add new member: %s"
    |> Notifications.error
})

let shutdown() =
  Shutdown |> postCommand
    (fun _ -> Notifications.info "The service has been shut down")
    Notifications.error

let saveProject() =
  SaveProject |> postCommand
    (fun _ -> Notifications.success "The project has been saved")
    Notifications.error

let unloadProject() =
  UnloadProject |> postCommand
    (fun _ ->
      Notifications.success "The project has been unloaded"
      Browser.location.reload())
    Notifications.error

let setLogLevel(lv) =
  LogLevel.Parse(lv) |> SetLogLevel |> ClientContext.Singleton.Post

let nullify _: 'a = null

let loadProject(project: Name, username: UserName, pass: Password, site: NameAndId option, ipAndPort: string option): JS.Promise<string option> =
  LoadProject(project, username, pass, site)
  |> postCommandPrivate ipAndPort
  |> Promise.bind (fun res ->
    if res.Ok
    then
      ClientContext.Singleton.ConnectWithWebSocket()
      |> Promise.map (fun _msg -> // TODO: Check message?
        Notifications.success "The project has been loaded successfully"
        Browser.location.reload()
        None)
    else
      res.text() |> Promise.map (fun msg ->
        if msg.Contains(ErrorMessages.PROJECT_NO_ACTIVE_CONFIG)
          || msg.Contains(ErrorMessages.PROJECT_MISSING_CLUSTER)
          || msg.Contains(ErrorMessages.PROJECT_MISSING_MEMBER)
          || msg.Contains(ErrorMessages.PROJECT_MEMBER_MISMATCH)
        then Some msg
        // We cannot deal with the error, just notify it
        else
          Notifications.error msg
          None
      )
  )

let getProjectSites(project, username, password) =
  GetProjectSites(project, username, password) |> postCommand
    ofJson<NameAndId[]>
    (fun msg -> Notifications.error msg; [||])

let createProject(projectName: string): JS.Promise<Name option> = promise {
  let! (machine: IrisMachine) = postCommandParseAndContinue None MachineConfig
  let! result =
    { name     = projectName
      ipAddr   = string machine.BindAddress
      port     = unwrap machine.RaftPort
      apiPort  = unwrap machine.ApiPort
      wsPort   = unwrap machine.WsPort
      gitPort  = unwrap machine.GitPort }
    |> CreateProject
    |> postCommand
      (fun _ ->
        Notifications.success "The project has been created successfully"
        Some (name projectName))
      (fun error ->
        Notifications.error error
        None)
  return result
}

let updatePinValue(pin: Pin, index: int, value: obj) =
  let updateArray (i: int) (v: obj) (ar: 'T[]) =
    let newArray = Array.copy ar
    newArray.[i] <- unbox v
    newArray
  let client = if Pin.isPreset pin then Some pin.ClientId else None
  match pin with
  | StringPin pin ->
    StringSlices(pin.Id, client, updateArray index value pin.Values)
  | NumberPin pin ->
    let value =
      match value with
      | :? string as v -> box(double v)
      | v -> v
    NumberSlices(pin.Id, client, updateArray index value pin.Values)
  | BoolPin pin ->
    let value =
      match value with
      | :? string as v -> box(v.ToLower() = "true")
      | v -> v
    BoolSlices(pin.Id, client, updateArray index value pin.Values)
  | BytePin   _pin -> failwith "TO BE IMPLEMENTED"
  | EnumPin   _pin -> failwith "TO BE IMPLEMENTED"
  | ColorPin  _pin -> failwith "TO BE IMPLEMENTED"
  |> UpdateSlices.ofSlices
  |> ClientContext.Singleton.Post

let findPin (pinId: PinId) (state: State) : Pin =
  let groups = state.PinGroups |> PinGroupMap.unifiedPins |> PinGroupMap.byGroup
  match Map.tryFindPin pinId groups with
  | Some pin -> pin
  | None -> failwithf "Cannot find pin with Id %O in GlobalState" pinId

let findPinGroup (pinGroupId: PinGroupId) (state: State) =
  let groups = state.PinGroups |> PinGroupMap.unifiedPins |> PinGroupMap.byGroup
  match Map.tryFind pinGroupId groups with
  | Some pinGroup -> pinGroup
  | None -> failwithf "Cannot find pin group with Id %O in GlobalState" pinGroupId

let findCue (cueId: CueId) (state: State) =
  match Map.tryFind cueId state.Cues with
  | Some cue -> cue
  | None -> failwithf "Cannot find cue with Id %O in GlobalState" cueId

let addCue (cueList:CueList) (cueGroupIndex:int) (cueIndex:int) =
  // TODO: Select the cue list from the widget
  if cueList.Groups.Length = 0 then
    failwith "A Cue Group must be added first"
  // Create new Cue and CueReference
  let newCue = { Id = IrisId.Create(); Name = name "Untitled"; Slices = [||] }
  let newCueRef = { Id = IrisId.Create(); CueId = newCue.Id; AutoFollow = -1; Duration = -1; Prewait = -1 }
  // Insert new CueRef in the selected CueGroup after the selected cue
  let cueGroup = cueList.Groups.[max cueGroupIndex 0]
  let idx = if cueIndex < 0 then cueGroup.CueRefs.Length - 1 else cueIndex
  let newCueGroup = { cueGroup with CueRefs = insertAfter idx newCueRef cueGroup.CueRefs }
  // Update the CueList
  let newCueList = { cueList with Groups = replaceById newCueGroup cueList.Groups }
  // Send messages to backend
  AddCue newCue |> ClientContext.Singleton.Post
  UpdateCueList newCueList |> ClientContext.Singleton.Post