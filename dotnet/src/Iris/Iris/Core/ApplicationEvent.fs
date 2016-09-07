namespace Iris.Core

open Iris.Raft

#if JAVASCRIPT
#else

open Iris.Serialization.Raft
open FlatBuffers

#endif

(*
        _                _____                 _
       / \   _ __  _ __ | ____|_   _____ _ __ | |_ ™
      / _ \ | '_ \| '_ \|  _| \ \ / / _ \ '_ \| __|
     / ___ \| |_) | |_) | |___ \ V /  __/ | | | |_
    /_/   \_\ .__/| .__/|_____| \_/ \___|_| |_|\__|
            |_|   |_|

    The AppEventT type models all possible state-changes the app can legally
    undergo. Using this design, we have a clean understanding of how data flows
    through the system, and have the compiler assist us in handling all possible
    states with total functions.

*)

[<RequireQualifiedAccess>]
type AppCommand =
  | Undo
  | Redo
  | Reset

  // PROJECT
  | SaveProject
  // | OpenProject
  // | CreateProject
  // | CloseProject
  // | DeleteProject

#if JAVASCRIPT
#else
  with
    static member FromFB (fb: AppCommandFB) =
      match fb.Command with
      | AppCommandTypeFB.UndoFB        -> Some Undo
      | AppCommandTypeFB.RedoFB        -> Some Redo
      | AppCommandTypeFB.ResetFB       -> Some Reset
      | AppCommandTypeFB.SaveProjectFB -> Some SaveProject
      | _                              -> None

    member self.ToOffset(builder: FlatBufferBuilder) : Offset<AppCommandFB> =
      let tipe =
        match self with
        | Undo        -> AppCommandTypeFB.UndoFB
        | Redo        -> AppCommandTypeFB.RedoFB
        | Reset       -> AppCommandTypeFB.ResetFB
        | SaveProject -> AppCommandTypeFB.SaveProjectFB

      AppCommandFB.StartAppCommandFB(builder)
      AppCommandFB.AddCommand(builder, tipe)
      AppCommandFB.EndAppCommandFB(builder)
#endif

type ApplicationEvent =

  // CLIENT
  // | AddClient    of string
  // | UpdateClient of string
  // | RemoveClient of string

  // NODE
  | AddNode      of RaftNode
  | UpdateNode   of RaftNode
  | RemoveNode   of RaftNode

  // PATCH
  | AddPatch    of Patch
  | UpdatePatch of Patch
  | RemovePatch of Patch

  // IOBOX
  | AddIOBox    of IOBox
  | UpdateIOBox of IOBox
  | RemoveIOBox of IOBox

  // CUE
  | AddCue      of Cue
  | UpdateCue   of Cue
  | RemoveCue   of Cue

  | Command     of AppCommand

  | LogMsg      of LogLevel * string

  with

    override self.ToString() : string =
      match self with
      // PROJECT
      // | OpenProject   -> "OpenProject"
      // | SaveProject   -> "SaveProject"
      // | CreateProject -> "CreateProject"
      // | CloseProject  -> "CloseProject"
      // | DeleteProject -> "DeleteProject"

      // CLIENT
      // | AddClient    s -> sprintf "AddClient %s"    s
      // | UpdateClient s -> sprintf "UpdateClient %s" s
      // | RemoveClient s -> sprintf "RemoveClient %s" s

      // NODE
      | AddNode    node -> sprintf "AddNode %s"    (string node)
      | UpdateNode node -> sprintf "UpdateNode %s" (string node)
      | RemoveNode node -> sprintf "RemoveNode %s" (string node)

      // PATCH
      | AddPatch    patch -> sprintf "AddPatch %s"    (string patch)
      | UpdatePatch patch -> sprintf "UpdatePatch %s" (string patch)
      | RemovePatch patch -> sprintf "RemovePatch %s" (string patch)

      // IOBOX
      | AddIOBox    iobox -> sprintf "AddIOBox %s"    (string iobox)
      | UpdateIOBox iobox -> sprintf "UpdateIOBox %s" (string iobox)
      | RemoveIOBox iobox -> sprintf "RemoveIOBox %s" (string iobox)

      // CUE
      | AddCue    cue      -> sprintf "AddCue %s"    (string cue)
      | UpdateCue cue      -> sprintf "UpdateCue %s" (string cue)
      | RemoveCue cue      -> sprintf "RemoveCue %s" (string cue)
      | Command    ev      -> sprintf "Command: %s"  (string ev)
      | LogMsg(level, msg) -> sprintf "LogMsg: [%A] %s" level msg

#if JAVASCRIPT
#else
    static member FromFB (fb: ApplicationEventFB) =
      match fb.AppEventType with

      //   ____
      //  / ___|   _  ___
      // | |  | | | |/ _ \
      // | |__| |_| |  __/
      //  \____\__,_|\___|

      | ApplicationEventTypeFB.AddCueFB ->
        let ev = fb.GetAppEvent(new AddCueFB())
        ev.GetCue(new CueFB())
        |> Cue.FromFB
        |> Option.map AddCue

      | ApplicationEventTypeFB.UpdateCueFB  ->
        let ev = fb.GetAppEvent(new UpdateCueFB())
        ev.GetCue(new CueFB())
        |> Cue.FromFB
        |> Option.map UpdateCue

      | ApplicationEventTypeFB.RemoveCueFB  ->
        let ev = fb.GetAppEvent(new RemoveCueFB())
        ev.GetCue(new CueFB())
        |> Cue.FromFB
        |> Option.map RemoveCue

      //  ____       _       _
      // |  _ \ __ _| |_ ___| |__
      // | |_) / _` | __/ __| '_ \
      // |  __/ (_| | || (__| | | |
      // |_|   \__,_|\__\___|_| |_|

      | ApplicationEventTypeFB.AddPatchFB ->
        let ev = fb.GetAppEvent(new AddPatchFB())
        ev.GetPatch(new PatchFB())
        |> Patch.FromFB
        |> Option.map AddPatch

      | ApplicationEventTypeFB.UpdatePatchFB  ->
        let ev = fb.GetAppEvent(new UpdatePatchFB())
        ev.GetPatch(new PatchFB())
        |> Patch.FromFB
        |> Option.map UpdatePatch

      | ApplicationEventTypeFB.RemovePatchFB  ->
        let ev = fb.GetAppEvent(new RemovePatchFB())
        ev.GetPatch(new PatchFB())
        |> Patch.FromFB
        |> Option.map RemovePatch

      //  ___ ___  ____
      // |_ _/ _ \| __ )  _____  __
      //  | | | | |  _ \ / _ \ \/ /
      //  | | |_| | |_) | (_) >  <
      // |___\___/|____/ \___/_/\_\

      | ApplicationEventTypeFB.AddIOBoxFB ->
        let ev = fb.GetAppEvent(new AddIOBoxFB())
        ev.GetIOBox(new IOBoxFB())
        |> IOBox.FromFB
        |> Option.map AddIOBox

      | ApplicationEventTypeFB.UpdateIOBoxFB  ->
        let ev = fb.GetAppEvent(new UpdateIOBoxFB())
        ev.GetIOBox(new IOBoxFB())
        |> IOBox.FromFB
        |> Option.map UpdateIOBox

      | ApplicationEventTypeFB.RemoveIOBoxFB  ->
        let ev = fb.GetAppEvent(new RemoveIOBoxFB())
        ev.GetIOBox(new IOBoxFB())
        |> IOBox.FromFB
        |> Option.map RemoveIOBox

      //  _   _           _
      // | \ | | ___   __| | ___
      // |  \| |/ _ \ / _` |/ _ \
      // | |\  | (_) | (_| |  __/
      // |_| \_|\___/ \__,_|\___|

      | ApplicationEventTypeFB.AddNodeFB ->
        let ev = fb.GetAppEvent(new AddNodeFB())
        ev.GetNode(new NodeFB())
        |> RaftNode.FromFB
        |> Option.map AddNode

      | ApplicationEventTypeFB.UpdateNodeFB  ->
        let ev = fb.GetAppEvent(new UpdateNodeFB())
        ev.GetNode(new NodeFB())
        |> RaftNode.FromFB
        |> Option.map UpdateNode

      | ApplicationEventTypeFB.RemoveNodeFB  ->
        let ev = fb.GetAppEvent(new RemoveNodeFB())
        ev.GetNode(new NodeFB())
        |> RaftNode.FromFB
        |> Option.map RemoveNode

      //  __  __ _
      // |  \/  (_)___  ___
      // | |\/| | / __|/ __|
      // | |  | | \__ \ (__
      // |_|  |_|_|___/\___|

      | ApplicationEventTypeFB.LogMsgFB     ->
        let ev = fb.GetAppEvent(new LogMsgFB())
        let level = LogLevel.Parse ev.LogLevel
        LogMsg(level, ev.Msg) |> Some

      | ApplicationEventTypeFB.AppCommandFB ->
        let ev = fb.GetAppEvent(new AppCommandFB())
        AppCommand.FromFB ev
        |> Option.map Command

      | _ -> None

    member self.ToOffset(builder: FlatBufferBuilder) : Offset<ApplicationEventFB> =
      let mkOffset tipe value =
        ApplicationEventFB.StartApplicationEventFB(builder)
        ApplicationEventFB.AddAppEventType(builder, tipe)
        ApplicationEventFB.AddAppEvent(builder, value)
        ApplicationEventFB.EndApplicationEventFB(builder)

      match self with
      //   ____
      //  / ___|   _  ___
      // | |  | | | |/ _ \
      // | |__| |_| |  __/
      //  \____\__,_|\___|

      | AddCue cue ->
        let cuefb = cue.ToOffset(builder)
        AddCueFB.StartAddCueFB(builder)
        AddCueFB.AddCue(builder, cuefb)
        let addfb = AddCueFB.EndAddCueFB(builder)
        mkOffset ApplicationEventTypeFB.AddCueFB addfb.Value

      | UpdateCue cue ->
        let cuefb = cue.ToOffset(builder)
        UpdateCueFB.StartUpdateCueFB(builder)
        UpdateCueFB.AddCue(builder, cuefb)
        let updatefb = UpdateCueFB.EndUpdateCueFB(builder)
        mkOffset ApplicationEventTypeFB.UpdateCueFB updatefb.Value

      | RemoveCue cue ->
        let cuefb = cue.ToOffset(builder)
        RemoveCueFB.StartRemoveCueFB(builder)
        RemoveCueFB.AddCue(builder, cuefb)
        let removefb = RemoveCueFB.EndRemoveCueFB(builder)
        mkOffset ApplicationEventTypeFB.RemoveCueFB removefb.Value

      //  ____       _       _
      // |  _ \ __ _| |_ ___| |__
      // | |_) / _` | __/ __| '_ \
      // |  __/ (_| | || (__| | | |
      // |_|   \__,_|\__\___|_| |_|

      | AddPatch patch ->
        let patchfb = patch.ToOffset(builder)
        AddPatchFB.StartAddPatchFB(builder)
        AddPatchFB.AddPatch(builder, patchfb)
        let addfb = AddPatchFB.EndAddPatchFB(builder)
        mkOffset ApplicationEventTypeFB.AddPatchFB addfb.Value

      | UpdatePatch patch ->
        let patchfb = patch.ToOffset(builder)
        UpdatePatchFB.StartUpdatePatchFB(builder)
        UpdatePatchFB.AddPatch(builder, patchfb)
        let updatefb = UpdatePatchFB.EndUpdatePatchFB(builder)
        mkOffset ApplicationEventTypeFB.UpdatePatchFB updatefb.Value

      | RemovePatch patch ->
        let patchfb = patch.ToOffset(builder)
        RemovePatchFB.StartRemovePatchFB(builder)
        RemovePatchFB.AddPatch(builder, patchfb)
        let removefb = RemovePatchFB.EndRemovePatchFB(builder)
        mkOffset ApplicationEventTypeFB.RemovePatchFB removefb.Value

      //  ___ ___  ____
      // |_ _/ _ \| __ )  _____  __
      //  | | | | |  _ \ / _ \ \/ /
      //  | | |_| | |_) | (_) >  <
      // |___\___/|____/ \___/_/\_\

      | AddIOBox iobox ->
        let ioboxfb = iobox.ToOffset(builder)
        AddIOBoxFB.StartAddIOBoxFB(builder)
        AddIOBoxFB.AddIOBox(builder, ioboxfb)
        let addfb = AddIOBoxFB.EndAddIOBoxFB(builder)
        mkOffset ApplicationEventTypeFB.AddIOBoxFB addfb.Value

      | UpdateIOBox iobox ->
        let ioboxfb = iobox.ToOffset(builder)
        UpdateIOBoxFB.StartUpdateIOBoxFB(builder)
        UpdateIOBoxFB.AddIOBox(builder, ioboxfb)
        let updatefb = UpdateIOBoxFB.EndUpdateIOBoxFB(builder)
        mkOffset ApplicationEventTypeFB.UpdateIOBoxFB updatefb.Value

      | RemoveIOBox iobox ->
        let ioboxfb = iobox.ToOffset(builder)
        RemoveIOBoxFB.StartRemoveIOBoxFB(builder)
        RemoveIOBoxFB.AddIOBox(builder, ioboxfb)
        let removefb = RemoveIOBoxFB.EndRemoveIOBoxFB(builder)
        mkOffset ApplicationEventTypeFB.RemoveIOBoxFB removefb.Value

      //  ____        __ _   _   _           _
      // |  _ \ __ _ / _| |_| \ | | ___   __| | ___
      // | |_) / _` | |_| __|  \| |/ _ \ / _` |/ _ \
      // |  _ < (_| |  _| |_| |\  | (_) | (_| |  __/
      // |_| \_\__,_|_|  \__|_| \_|\___/ \__,_|\___|

      | AddNode node ->
        let nodefb = node.ToOffset(builder)
        AddNodeFB.StartAddNodeFB(builder)
        AddNodeFB.AddNode(builder, nodefb)
        let addfb = AddNodeFB.EndAddNodeFB(builder)
        mkOffset ApplicationEventTypeFB.AddNodeFB addfb.Value

      | UpdateNode node ->
        let nodefb = node.ToOffset(builder)
        UpdateNodeFB.StartUpdateNodeFB(builder)
        UpdateNodeFB.AddNode(builder, nodefb)
        let updatefb = UpdateNodeFB.EndUpdateNodeFB(builder)
        mkOffset ApplicationEventTypeFB.UpdateNodeFB updatefb.Value

      | RemoveNode node ->
        let nodefb = node.ToOffset(builder)
        RemoveNodeFB.StartRemoveNodeFB(builder)
        RemoveNodeFB.AddNode(builder, nodefb)
        let removefb = RemoveNodeFB.EndRemoveNodeFB(builder)
        mkOffset ApplicationEventTypeFB.RemoveNodeFB removefb.Value

      //  __  __ _
      // |  \/  (_)___  ___
      // | |\/| | / __|/ __|
      // | |  | | \__ \ (__
      // |_|  |_|_|___/\___|

      | Command ev ->
        let cmdfb = ev.ToOffset(builder)
        mkOffset ApplicationEventTypeFB.AppCommandFB cmdfb.Value

      | LogMsg(level, msg) ->
        let level = string level |> builder.CreateString
        let msg = msg |> builder.CreateString
        let log = LogMsgFB.CreateLogMsgFB(builder, level, msg)
        mkOffset ApplicationEventTypeFB.LogMsgFB log.Value

    member self.ToBytes () = buildBuffer self

    static member FromBytes (bytes: byte array) : ApplicationEvent option =
      let msg = ApplicationEventFB.GetRootAsApplicationEventFB(new ByteBuffer(bytes))
      ApplicationEvent.FromFB(msg)

#endif