module Iris.Web.Types

open System
open Fable.Core
open Fable.Import
open Iris.Core

module StorageKeys =
  let [<Literal>] layout = "iris-layout"
  let [<Literal>] widgets = "iris-widgets"

module Widgets =
    let [<Literal>] Log = "LOG"
    let [<Literal>] GraphView = "Graph View"

type IWidget =
  abstract Id: Guid
  abstract Name: string
  abstract InitialLayout: Layout
  abstract Render: Guid * Elmish.Dispatch<Msg> * Model -> React.ReactElement

and WidgetRef = Guid * string

and Direction =
  | Ascending
  | Descending
  member this.Reverse =
    match this with
    | Ascending -> Descending
    | Descending -> Ascending

and Sorting =
  { column: string
    direction: Direction
  }

and Msg =
  | AddWidget of Guid * IWidget
  | RemoveWidget of Guid
  // | AddTab
  | AddLog of LogEvent
  | UpdateLogConfig of LogConfig
  | UpdateState of State option

and Model =
  { widgets: Map<Guid,IWidget>
    logs: LogEvent list
    logConfig: LogConfig
    layout: Layout[]
    state: State option
    useRightClick: bool
  }

and LogConfig =
  { filter: string option
    logLevel: LogLevel option
    setLogLevel: LogLevel
    sorting: Sorting option
    columns: Map<string, bool>
    viewLogs: LogEvent array
  }
  static member Create(logs: LogEvent list) =
    { filter = None
      logLevel = None
      // TODO: This should be read from backend
      setLogLevel = LogLevel.Debug
      sorting = None
      columns =
        Map["LogLevel", true
            "Time", true
            "Tag", true
            "Tier", true]
      viewLogs = Array.ofList logs }

and [<Pojo>] Layout =
  { i: Guid; ``static``: bool
    x: int; y: int
    w: int; h: int
    minW: int; maxW: int
    minH: int; maxH: int }

and IFactory =
  abstract CreateWidget: id: Guid option * name: string -> IWidget

let mutable private singletonFactory = None

let getFactory() =
  match singletonFactory with
  | Some x -> x
  | None -> failwith "Factory hasn't been initialized yet"

let initFactory(factory: IFactory) =
  singletonFactory <- Some factory
