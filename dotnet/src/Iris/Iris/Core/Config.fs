namespace Iris.Core

open System
open System.IO
open Iris.Raft

//     _    _ _
//    / \  | (_) __ _ ___  ___  ___
//   / _ \ | | |/ _` / __|/ _ \/ __|
//  / ___ \| | | (_| \__ \  __/\__ \
// /_/   \_\_|_|\__,_|___/\___||___/

type DisplayYaml    = ConfigFile.Project_Type.Displays_Item_Type
type ViewPortYaml   = ConfigFile.Project_Type.ViewPorts_Item_Type
type TaskYaml       = ConfigFile.Project_Type.Tasks_Item_Type
type ArgumentYaml   = TaskYaml.Arguments_Item_Type
type ClusterYaml    = ConfigFile.Project_Type.Cluster_Type
type NodeYaml       = ConfigFile.Project_Type.Cluster_Type.Nodes_Item_Type
type GroupYaml      = ConfigFile.Project_Type.Cluster_Type.Groups_Item_Type
type AudioYaml      = ConfigFile.Project_Type.Audio_Type
type EngineYaml     = ConfigFile.Project_Type.Engine_Type
type MetadatYaml    = ConfigFile.Project_Type.Metadata_Type
type PortsYaml      = ConfigFile.Project_Type.Ports_Type
type TimingYaml     = ConfigFile.Project_Type.Timing_Type
type VvvvYaml       = ConfigFile.Project_Type.VVVV_Type
type ExeYaml        = ConfigFile.Project_Type.VVVV_Type.Executables_Item_Type
type PluginYaml     = ConfigFile.Project_Type.VVVV_Type.Plugins_Item_Type
type SignalYaml     = DisplayYaml.Signals_Item_Type
type RegionMapYaml  = DisplayYaml.RegionMap_Type
type RegionYaml     = RegionMapYaml.Regions_Item_Type
//  ____        __ _    ____             __ _
// |  _ \ __ _ / _| |_ / ___|___  _ __  / _(_) __ _
// | |_) / _` | |_| __| |   / _ \| '_ \| |_| |/ _` |
// |  _ < (_| |  _| |_| |__| (_) | | | |  _| | (_| |
// |_| \_\__,_|_|  \__|\____\___/|_| |_|_| |_|\__, |
//                                            |___/

/// ## RaftConfig
///
/// Configuration for Raft-specific, user-facing values.
///
type RaftConfig =
  { RequestTimeout:   Long
    ElectionTimeout:  Long
    MaxLogDepth:      Long
    LogLevel:         LogLevel
    DataDir:          FilePath
    MaxRetries:       uint8
    PeriodicInterval: uint8 }

  static member Default =
    let guid = Guid.NewGuid()
    let path = Path.GetTempPath() </> guid.ToString() </> RAFT_DIRECTORY
    { RequestTimeout   = 500u
      ElectionTimeout  = 6000u
      MaxLogDepth      = 20u
      MaxRetries       = 10uy
      PeriodicInterval = 50uy
      LogLevel         = Err
      DataDir          = path }

// __     __                     ____             __ _
// \ \   / /_   ____   ____   __/ ___|___  _ __  / _(_) __ _
//  \ \ / /\ \ / /\ \ / /\ \ / / |   / _ \| '_ \| |_| |/ _` |
//   \ V /  \ V /  \ V /  \ V /| |__| (_) | | | |  _| | (_| |
//    \_/    \_/    \_/    \_/  \____\___/|_| |_|_| |_|\__, |
//                                                     |___/

type VvvvConfig =
  { Executables : VvvvExe list
    Plugins     : VvvvPlugin list }

  static member Default =
    { Executables = List.empty
      Plugins     = List.empty }

//  ____            _    ____             __ _
// |  _ \ ___  _ __| |_ / ___|___  _ __  / _(_) __ _
// | |_) / _ \| '__| __| |   / _ \| '_ \| |_| |/ _` |
// |  __/ (_) | |  | |_| |__| (_) | | | |  _| | (_| |
// |_|   \___/|_|   \__|\____\___/|_| |_|_| |_|\__, |
//                                             |___/

type PortConfig =
  { UDPCue : uint32 }

  static member Default =
    { UDPCue = 8075u }

//  _____ _           _              ____             __ _
// |_   _(_)_ __ ___ (_)_ __   __ _ / ___|___  _ __  / _(_) __ _
//   | | | | '_ ` _ \| | '_ \ / _` | |   / _ \| '_ \| |_| |/ _` |
//   | | | | | | | | | | | | | (_| | |__| (_) | | | |  _| | (_| |
//   |_| |_|_| |_| |_|_|_| |_|\__, |\____\___/|_| |_|_| |_|\__, |
//                            |___/                        |___/

type TimingConfig =
  { Framebase : uint32
    Input     : string
    Servers   : IpAddress list
    UDPPort   : uint32
    TCPPort   : uint32 }

  static member Default =
    { Framebase = 50u
      Input     = "Iris Freerun"
      Servers   = List.empty
      UDPPort   = 8071u
      TCPPort   = 8072u }

//     _             _ _        ____             __ _
//    / \  _   _  __| (_) ___  / ___|___  _ __  / _(_) __ _
//   / _ \| | | |/ _` | |/ _ \| |   / _ \| '_ \| |_| |/ _` |
//  / ___ \ |_| | (_| | | (_) | |__| (_) | | | |  _| | (_| |
// /_/   \_\__,_|\__,_|_|\___/ \____\___/|_| |_|_| |_|\__, |
//                                                    |___/

type AudioConfig =
  { SampleRate : uint32 }

  static member Default =
    { SampleRate = 48000u }

//  _   _           _    ____
// | | | | ___  ___| |_ / ___|_ __ ___  _   _ _ __
// | |_| |/ _ \/ __| __| |  _| '__/ _ \| | | | '_ \
// |  _  | (_) \__ \ |_| |_| | | | (_) | |_| | |_) |
// |_| |_|\___/|___/\__|\____|_|  \___/ \__,_| .__/
//                                           |_|

type HostGroup =
  { Name    : Name
    Members : Id list }

  override self.ToString() =
    sprintf "HostGroup:
              Name: %A
              Members: %A"
            self.Name
            (List.fold (fun m s -> m + " " + string s) "" self.Members)

//   ____ _           _
//  / ___| |_   _ ___| |_ ___ _ __
// | |   | | | | / __| __/ _ \ '__|
// | |___| | |_| \__ \ ||  __/ |
//  \____|_|\__,_|___/\__\___|_|

type Cluster =
  { Name   : Name
    Nodes  : RaftNode  list
    Groups : HostGroup list }

  override self.ToString() =
    sprintf "Cluster:
              Name: %A
              Nodes: %A
              Groups: %A"
            self.Name
            self.Nodes
            self.Groups

//   ____             __ _
//  / ___|___  _ __  / _(_) __ _
// | |   / _ \| '_ \| |_| |/ _` |
// | |__| (_) | | | |  _| | (_| |
//  \____\___/|_| |_|_| |_|\__, |
//                         |___/

type IrisConfig =
  { AudioConfig    : AudioConfig
    VvvvConfig     : VvvvConfig
    RaftConfig     : RaftConfig
    TimingConfig   : TimingConfig
    PortConfig     : PortConfig
    ClusterConfig  : Cluster
    ViewPorts      : ViewPort list
    Displays       : Display  list
    Tasks          : Task     list }

//  _   _      _
// | | | | ___| |_ __   ___ _ __ ___
// | |_| |/ _ \ | '_ \ / _ \ '__/ __|
// |  _  |  __/ | |_) |  __/ |  \__ \
// |_| |_|\___|_| .__/ \___|_|  |___/
//              |_|

[<RequireQualifiedAccess>]
module Config =

  let private parseTuple (input: string) : Either<IrisError,int * int> =
    input.Split [| '('; ','; ' '; ')' |]       // split the string according to the specified chars
    |> Array.filter (String.length >> ((<) 0)) // filter out elements that have zero length
    |> fun parsed ->
      try
        match parsed with
        | [| x; y |] -> Right (int x, int y)
        | _ ->
          sprintf "Cannot parse %A as (int * int) tuple" input
          |> ParseError
          |> Either.fail
      with
        | exn ->
          sprintf "Cannot parse %A as (int * int) tuple: %s" input exn.Message
          |> ParseError
          |> Either.fail

  let private parseRect (str : string) : Either<IrisError,Rect> =
    parseTuple str
    |> Either.map Rect

  let private parseCoordinate (str : string) : Either<IrisError,Coordinate> =
    parseTuple str
    |> Either.map Coordinate

  let parseStringProp (str : string) : string option =
    if str.Length > 0 then Some(str) else None

  //      _             _ _
  //     / \  _   _  __| (_) ___
  //    / _ \| | | |/ _` | |/ _ \
  //   / ___ \ |_| | (_| | | (_) |
  //  /_/   \_\__,_|\__,_|_|\___/

  /// ### Parse the Audio configuration section
  ///
  /// Parses the Audio configuration section of the passed-in configuration file.
  ///
  /// # Returns: AudioConfig
  let private parseAudio (config: ConfigFile) : Either<IrisError, AudioConfig> =
    Either.tryWith ParseError "AudioConfig" <| fun _ ->
      { SampleRate = uint32 config.Project.Audio.SampleRate }

  /// ### Save the AudioConfig value
  ///
  /// Transfer the configuration from `AudioConfig` values to a given config file.
  ///
  /// # Returns: ConfigFile
  let private saveAudio (file: ConfigFile, config: IrisConfig) =
    file.Project.Audio.SampleRate <- int (config.AudioConfig.SampleRate)
    (file, config)

  //  __     __
  //  \ \   / /_   ____   ____   __
  //   \ \ / /\ \ / /\ \ / /\ \ / /
  //    \ V /  \ V /  \ V /  \ V /
  //     \_/    \_/    \_/    \_/
  //

  let parseExe (exe: ExeYaml) : Either<IrisError, VvvvExe> =
    Right { Executable = exe.Path
            Version    = exe.Version
            Required   = exe.Required }

  let parseExes exes : Either<IrisError, VvvvExe list> =
    either {
      let! exes =
        Seq.fold
          (fun (m: Either<IrisError,VvvvExe list>) exe -> either {
            let! exes = m
            let! exe = parseExe exe
            return exe :: exes
          })
          (Right [])
          exes
      return List.reverse exes
    }

  let parsePlugin (plugin: PluginYaml) : Either<IrisError, VvvvPlugin> =
    Right { Name = plugin.Name
            Path = plugin.Path }

  let parsePlugins plugins : Either<IrisError, VvvvPlugin list> =
    either {
      let! plugins =
        Seq.fold
          (fun (m: Either<IrisError,VvvvPlugin list>) plugin -> either {
            let! plugins = m
            let! plugin = parsePlugin plugin
            return plugin :: plugins
          })
          (Right [])
          plugins
      return List.reverse plugins
    }

  /// ### Parses the VVVV configuration
  ///
  /// Constructs the VVVV configuration values from the handed config file value.
  ///
  /// # Returns: VvvvConfig
  let private parseVvvv (config: ConfigFile) : Either<IrisError, VvvvConfig> =
    either {
      let vvvv = config.Project.VVVV
      let! exes = parseExes vvvv.Executables
      let! plugins = parsePlugins vvvv.Plugins
      return { Executables = exes
               Plugins     = plugins }
    }

  /// ### Save the VVVV configuration
  ///
  /// Translate the values from Config into the passed in configuration file.
  ///
  /// # Returns: ConfigFile
  let private saveVvvv (file: ConfigFile, config: IrisConfig) =
    file.Project.VVVV.Executables.Clear()
    for exe in config.VvvvConfig.Executables do
      let entry = new ConfigFile.Project_Type.VVVV_Type.Executables_Item_Type()
      entry.Path <- exe.Executable;
      entry.Version <- exe.Version;
      entry.Required <- exe.Required
      file.Project.VVVV.Executables.Add(entry)

    file.Project.VVVV.Plugins.Clear()
    for plug in config.VvvvConfig.Plugins do
      let entry = new ConfigFile.Project_Type.VVVV_Type.Plugins_Item_Type ()
      entry.Name <- plug.Name
      entry.Path <- plug.Path
      file.Project.VVVV.Plugins.Add(entry)

    (file, config)

  //  ____        __ _
  // |  _ \ __ _ / _| |_
  // | |_) / _` | |_| __|
  // |  _ < (_| |  _| |_
  // |_| \_\__,_|_|  \__|

  /// ## Parses Raft-related values in passed configuration
  ///
  /// Parses the passed-in configuration file contents and returns a `RaftConfig` value.
  ///
  /// Returns: RaftConfig
  let private parseRaft (config: ConfigFile) : Either<IrisError, RaftConfig> =
    either {
      let engine = config.Project.Engine

      let! loglevel = LogLevel.TryParse engine.LogLevel

      try
        return
          { RequestTimeout   = uint32 engine.RequestTimeout
            ElectionTimeout  = uint32 engine.ElectionTimeout
            MaxLogDepth      = uint32 engine.MaxLogDepth
            LogLevel         = loglevel
            DataDir          = engine.DataDir
            MaxRetries       = uint8 engine.MaxRetries
            PeriodicInterval = uint8 engine.PeriodicInterval }
      with
        | exn ->
          return!
            sprintf "Could not parse Engine config: %s" exn.Message
            |> ParseError
            |> Either.fail
    }

  /// ### Save the passed RaftConfig to the configuration file
  ///
  /// Save Raft algorithm specific configuration options to the configuration file object.
  ///
  /// # Returns: ConfigFile
  let private saveRaft (file: ConfigFile, config: IrisConfig) =
    file.Project.Engine.RequestTimeout   <- int config.RaftConfig.RequestTimeout
    file.Project.Engine.ElectionTimeout  <- int config.RaftConfig.ElectionTimeout
    file.Project.Engine.MaxLogDepth      <- int config.RaftConfig.MaxLogDepth
    file.Project.Engine.LogLevel         <- string config.RaftConfig.LogLevel
    file.Project.Engine.DataDir          <- config.RaftConfig.DataDir
    file.Project.Engine.MaxRetries       <- int config.RaftConfig.MaxRetries
    file.Project.Engine.PeriodicInterval <- int config.RaftConfig.PeriodicInterval
    (file, config)

  //   _____ _           _
  //  |_   _(_)_ __ ___ (_)_ __   __ _
  //    | | | | '_ ` _ \| | '_ \ / _` |
  //    | | | | | | | | | | | | | (_| |
  //    |_| |_|_| |_| |_|_|_| |_|\__, |
  //                             |___/

  /// ### Parse the timing related configuration options
  ///
  /// Parse TimingConfig related values into a TimingConfig value and return it.
  ///
  /// # Returns: TimingConfig
  let private parseTiming (config: ConfigFile) : Either<IrisError,TimingConfig> =
    either {
      let timing = config.Project.Timing
      let! servers =
        Seq.fold
          (fun (m: Either<IrisError, IpAddress list>) thing -> either {
            let! lst = m
            let! server = IpAddress.TryParse thing
            return (server :: lst)
          })
          (Right [])
          timing.Servers

      try
        return
          { Framebase = uint32 timing.Framebase
            Input     = timing.Input
            Servers   = List.reverse servers
            UDPPort   = uint32 timing.UDPPort
            TCPPort   = uint32 timing.TCPPort }
      with
        | exn ->
          return!
            sprintf "Could not parse Timing config: %s" exn.Message
            |> ParseError
            |> Either.fail
    }


  /// ### Transfer the TimingConfig options to the passed configuration file
  ///
  ///
  ///
  /// # Returns: ConfigFile
  let private saveTiming (file: ConfigFile, config: IrisConfig) =
    file.Project.Timing.Framebase <- int (config.TimingConfig.Framebase)
    file.Project.Timing.Input     <- config.TimingConfig.Input

    file.Project.Timing.Servers.Clear()
    for srv in config.TimingConfig.Servers do
      file.Project.Timing.Servers.Add(string srv)

    file.Project.Timing.TCPPort <- int (config.TimingConfig.TCPPort)
    file.Project.Timing.UDPPort <- int (config.TimingConfig.UDPPort)

    (file, config)

  //   ____            _
  //  |  _ \ ___  _ __| |_
  //  | |_) / _ \| '__| __|
  //  |  __/ (_) | |  | |_
  //  |_|   \___/|_|   \__|

  /// ### Parse the Port configuration
  ///
  /// Parse the port configuration in a given config file into a `PortConfig` value.
  ///
  /// # Returns: PortConfig
  let private parsePort (config: ConfigFile) : Either<IrisError, PortConfig> =
    Either.tryWith ParseError "PortConfig" <| fun _ ->
      { UDPCue = uint32 config.Project.Ports.UDPCues }

  /// ### Transfer the PortConfig configuration
  ///
  /// Save all values in the PortConfig to the passed configuration file instance.
  ///
  /// # Returns: ConfigFile
  let private savePort (file: ConfigFile, config: IrisConfig) =
    file.Project.Ports.UDPCues <- int (config.PortConfig.UDPCue)
    (file, config)

  //  __     ___               ____            _
  //  \ \   / (_) _____      _|  _ \ ___  _ __| |_
  //   \ \ / /| |/ _ \ \ /\ / / |_) / _ \| '__| __|
  //    \ V / | |  __/\ V  V /|  __/ (_) | |  | |_
  //     \_/  |_|\___| \_/\_/ |_|   \___/|_|   \__|

  let parseViewPort (viewport: ViewPortYaml) =
    either {
      let! pos     = parseCoordinate viewport.Position
      let! size    = parseRect       viewport.Size
      let! outpos  = parseCoordinate viewport.OutputPosition
      let! outsize = parseRect       viewport.OutputSize
      let! overlap = parseRect       viewport.Overlap

      return { Id             = Id viewport.Id
               Name           = viewport.Name
               Position       = pos
               Size           = size
               OutputPosition = outpos
               OutputSize     = outsize
               Overlap        = overlap
               Description    = viewport.Description }
    }

  /// ### Parse all Viewport configs listed in a config file
  ///
  /// Parses the ViewPort config section and returns a list of `ViewPort` values.
  ///
  /// # Returns: ViewPort list
  let private parseViewPorts (config: ConfigFile) : Either<IrisError,ViewPort list> =
    either {
      let! viewports =
        Seq.fold
          (fun (m: Either<IrisError, ViewPort list>) vp -> either {
            let! viewports = m
            let! viewport = parseViewPort vp
            return viewport :: viewports
          })
          (Right [])
          config.Project.ViewPorts

      return List.reverse viewports
    }

  /// ### Transfers the passed list of ViewPort values
  ///
  /// Adds a config section for each ViewPort value in the passed in Config to the configuration
  /// file.
  ///
  /// # Returns: ConfigFile
  let private saveViewPorts (file: ConfigFile, config: IrisConfig) =
    file.Project.ViewPorts.Clear()
    for vp in config.ViewPorts do
      let item = new ConfigFile.Project_Type.ViewPorts_Item_Type()
      item.Id             <- string vp.Id
      item.Name           <- vp.Name
      item.Size           <- string vp.Size
      item.Position       <- string vp.Position
      item.Overlap        <- string vp.Overlap
      item.OutputPosition <- string vp.OutputPosition
      item.OutputSize     <- string vp.OutputSize
      item.Description    <- vp.Description
      file.Project.ViewPorts.Add(item)
    (file, config)

  //  ____  _                   _
  // / ___|(_) __ _ _ __   __ _| |___
  // \___ \| |/ _` | '_ \ / _` | / __|
  //  ___) | | (_| | | | | (_| | \__ \
  // |____/|_|\__, |_| |_|\__,_|_|___/
  //          |___/

  /// ## Parse a Signal definition
  ///
  /// Parse a signal definition. Returns a ParseError on failure.
  ///
  /// ### Signature:
  /// - signal: SignalYaml
  ///
  /// Returns: Either<IrisError, Signal>
  let parseSignal (signal: SignalYaml) : Either<IrisError, Signal> =
    either {
      let! size = parseRect       signal.Size
      let! pos = parseCoordinate signal.Position

      return { Size     = size
               Position = pos }
    }

  /// ## Parse a list of signals
  ///
  /// Parse a list of signals stored in the ConfigFile. Returns a ParseError on failure.
  ///
  /// ### Signature:
  /// - signals: SignalYaml collection
  ///
  /// Returns: Either<IrisError, Signal list>
  let parseSignals signals =
    either {
      let! parsed =
        Seq.fold
          (fun (m: Either<IrisError,Signal list>) signal -> either {
            let! signals = m
            let! signal = parseSignal signal
            return signal :: signals
          })
          (Right [])
          signals
      return List.reverse parsed
    }

  //  ____            _
  // |  _ \ ___  __ _(_) ___  _ __  ___
  // | |_) / _ \/ _` | |/ _ \| '_ \/ __|
  // |  _ <  __/ (_| | | (_) | | | \__ \
  // |_| \_\___|\__, |_|\___/|_| |_|___/
  //            |___/

  /// ## Parse a Region definition
  ///
  /// Parse a single Region definition. Returns a ParseError on failure.
  ///
  /// ### Signature:
  /// - region: Region
  ///
  /// Returns: Either<IrisError,Region>
  let parseRegion (region: RegionYaml) : Either<IrisError, Region> =
    either {
      let! srcpos  = parseCoordinate region.SrcPosition
      let! srcsize = parseRect       region.SrcSize
      let! outpos  = parseCoordinate region.OutputPosition
      let! outsize = parseRect       region.OutputSize

      return
        { Id             = Id region.Id
          Name           = region.Name
          SrcPosition    = srcpos
          SrcSize        = srcsize
          OutputPosition = outpos
          OutputSize     = outsize }
    }

  /// ## Parse a list of Region definitions
  ///
  /// Parse a list of Region definitions. Returns a ParseError on failure.
  ///
  /// ### Signature:
  /// - regions: RegionYaml collection
  ///
  /// Returns: Either<IrisError,Region list>
  let parseRegions regions : Either<IrisError, Region list> =
    either {
      let! parsed =
        Seq.fold
          (fun (m: Either<IrisError, Region list>) region -> either {
            let! regions = m
            let! region = parseRegion region
            return region :: regions
          })
          (Right [])
          regions
      return List.reverse parsed
    }

  //   ____  _           _
  //  |  _ \(_)___ _ __ | | __ _ _   _ ___
  //  | | | | / __| '_ \| |/ _` | | | / __|
  //  | |_| | \__ \ |_) | | (_| | |_| \__ \
  //  |____/|_|___/ .__/|_|\__,_|\__, |___/
  //              |_|            |___/

  /// ## Parse a Display definition
  ///
  /// Parse a Display definition. Returns a ParseError on failure.
  ///
  /// ### Signature:
  /// - display: DisplayYaml
  ///
  /// Returns: Either<IrisError,Display>
  let parseDisplay (display: DisplayYaml) : Either<IrisError, Display> =
    either {
      let! size = parseRect display.Size
      let! signals = parseSignals display.Signals
      let! regions = parseRegions display.RegionMap.Regions

      let regionmap =
        { SrcViewportId = Id display.RegionMap.SrcViewportId
          Regions       = regions }

      return { Id        = Id display.Id
               Name      = display.Name
               Size      = size
               Signals   = signals
               RegionMap = regionmap }
    }

  /// ## Parse a list of Display definitionsg
  ///
  /// Parses a list of Display definitions. Returns a ParseError on failure.
  ///
  /// ### Signature:
  /// - displays: DisplayYaml collection
  ///
  /// Returns: Either<IrisError,Display list>
  let private parseDisplays (config: ConfigFile) : Either<IrisError, Display list> =
    either {
      let! displays =
        Seq.fold
          (fun (m: Either<IrisError, Display list>) display -> either {
            let! displays = m
            let! display = parseDisplay display
            return (display :: displays)
          })
          (Right [])
          config.Project.Displays

      return List.reverse displays
    }

  /// ### Transfer the Display config to a configuration file
  ///
  /// Save all `Display` values in `Config` to the passed configuration file.
  ///
  /// # Returns: ConfigFile
  let private saveDisplays (file: ConfigFile, config: IrisConfig) =
    file.Project.Displays.Clear()
    for dp in config.Displays do
      let item = new ConfigFile.Project_Type.Displays_Item_Type()
      item.Id <- string dp.Id
      item.Name <- dp.Name
      item.Size <- dp.Size.ToString()

      item.RegionMap.SrcViewportId <- string dp.RegionMap.SrcViewportId
      item.RegionMap.Regions.Clear()

      for region in dp.RegionMap.Regions do
        let r = new ConfigFile.Project_Type.Displays_Item_Type.RegionMap_Type.Regions_Item_Type()
        r.Id <- string region.Id
        r.Name <- region.Name
        r.OutputPosition <- region.OutputPosition.ToString()
        r.OutputSize <- region.OutputSize.ToString()
        r.SrcPosition <- region.SrcPosition.ToString()
        r.SrcSize <- region.SrcSize.ToString()
        item.RegionMap.Regions.Add(r)

      item.Signals.Clear()

      for signal in dp.Signals do
        let s = new ConfigFile.Project_Type.Displays_Item_Type.Signals_Item_Type()
        s.Position <- signal.Position.ToString()
        s.Size <- signal.Size.ToString()
        item.Signals.Add(s)

      file.Project.Displays.Add(item)
    (file, config)

  //     _                                         _
  //    / \   _ __ __ _ _   _ _ __ ___   ___ _ __ | |_
  //   / _ \ | '__/ _` | | | | '_ ` _ \ / _ \ '_ \| __|
  //  / ___ \| | | (_| | |_| | | | | | |  __/ | | | |_
  // /_/   \_\_|  \__, |\__,_|_| |_| |_|\___|_| |_|\__|
  //              |___/


  /// ## Parse a single Argument key/value pair
  ///
  /// Parse a single Argument key/value pair
  ///
  /// ### Signature:
  /// - argument: ArgumentYaml
  ///
  /// Returns: Either<IrisError, string * string>
  let parseArgument (argument: ArgumentYaml) =
    either {
      if (argument.Key.Length > 0) && (argument.Value.Length > 0) then
        return (argument.Key, argument.Value)
      else
        return!
          sprintf "Could not parse Argument: %A" argument
          |> ParseError
          |> Either.fail
    }

  /// ## Parse a list of ArgumentYamls
  ///
  /// Parse a list of ArgumentYamls
  ///
  /// ### Signature:
  /// - arguments: ArgumentYaml collection
  ///
  /// Returns: Either<IrisError, (string * string) list>
  let parseArguments arguments =
    either {
      let! arguments =
        Seq.fold
          (fun (m: Either<IrisError, Argument list>) thing -> either {
            let! arguments = m
            let! argument = parseArgument thing
            return argument :: arguments
          })
          (Right [])
          arguments

      return List.reverse arguments
    }

  //   _____         _
  //  |_   _|_ _ ___| | _____
  //    | |/ _` / __| |/ / __|
  //    | | (_| \__ \   <\__ \
  //    |_|\__,_|___/_|\_\___/
  //


  /// ## Parse a Task definition
  ///
  /// Parse a single Task definition. Returns a ParseError on failure.
  ///
  /// ### Signature:
  /// - task: TaskYaml
  ///
  /// Returns: Either<IrisError, Task>
  let parseTask (task: TaskYaml) : Either<IrisError, Task> =
    either {
      let! arguments = parseArguments task.Arguments
      return { Id          = Id task.Id
               Description = task.Description
               DisplayId   = Id task.DisplayId
               AudioStream = task.AudioStream
               Arguments   = arguments }
    }

  /// ### Parse Task configuration section
  ///
  /// Create `Task` values for each entry in the Task config section.
  ///
  /// # Returns: Task list
  let private parseTasks (config: ConfigFile) : Either<IrisError,Task list> =
    either {
      let! tasks =
        Seq.fold
          (fun (m: Either<IrisError, Task list>) task -> either {
            let! tasks = m
            let! task = parseTask task
            return task :: tasks
          })
          (Right [])
          config.Project.Tasks
      return List.reverse tasks
    }

  /// ### Save the Tasks to a config file
  ///
  /// Transfers all `Task` values into the configuration file.
  ///
  /// # Returns: ConfigFile
  let private saveTasks (file: ConfigFile, config: IrisConfig) =
    file.Project.Tasks.Clear()
    for task in config.Tasks do
      let t = new ConfigFile.Project_Type.Tasks_Item_Type()
      t.Id <- string task.Id
      t.AudioStream <- task.AudioStream
      t.Description <- task.Description
      t.DisplayId   <- string task.DisplayId

      for arg in task.Arguments do
        let a = new ConfigFile.Project_Type.Tasks_Item_Type.Arguments_Item_Type()
        a.Key <- fst arg
        a.Value <- snd arg
        t.Arguments.Add(a)

      file.Project.Tasks.Add(t)
    (file, config)

  //    ____ _           _
  //   / ___| |_   _ ___| |_ ___ _ __
  //  | |   | | | | / __| __/ _ \ '__|
  //  | |___| | |_| \__ \ ||  __/ |
  //   \____|_|\__,_|___/\__\___|_|
  //

  /// ## Parse a single Node definition
  ///
  /// Parse a single Node definition. Returns a ParseError on failiure.
  ///
  /// ### Signature:
  /// - node: NodeYaml
  ///
  /// Returns: Either<IrisError, RaftNode>
  let parseNode (node: NodeYaml) : Either<IrisError, RaftNode> =
    either {
      let! ip = IpAddress.TryParse node.Ip
      let! state = RaftNodeState.TryParse node.State

      try
        return { Id         = Id node.Id
                 HostName   = node.HostName
                 IpAddr     = ip
                 Port       = uint16 node.Port
                 WebPort    = uint16 node.WebPort
                 WsPort     = uint16 node.WsPort
                 GitPort    = uint16 node.GitPort
                 State      = state
                 Voting     = true
                 VotedForMe = false
                 NextIndex  = 1u
                 MatchIndex = 0u }
      with
        | exn ->
          return!
            sprintf "Could not parse Node definition: %s" exn.Message
            |> ParseError
            |> Either.fail
    }

  /// ## Parse a collectio of Node definitions
  ///
  /// Parse a list of Node definitions. Returns a ParseError on failure.
  ///
  /// ### Signature:
  /// - nodes: NodeYaml collection
  ///
  /// Returns: Either<IrisError, RaftNode list>
  let parseNodes nodes : Either<IrisError, RaftNode list> =
    either {
      let! nodes =
        Seq.fold
          (fun (m: Either<IrisError, RaftNode list>) node -> either {
            let! nodes = m
            let! node = parseNode node
            return node :: nodes
          })
          (Right [])
          nodes
      return List.reverse nodes
    }

  let parseGroup (group: GroupYaml) : Either<IrisError, HostGroup> =
    either {
      if group.Name.Length > 0 then
        let ids = Seq.map Id group.Members |> Seq.toList

        return { Name    = group.Name
                 Members = ids }
      else
        return!
          "Invalid HostGroup setting (Name must be given)"
          |> ParseError
          |> Either.fail
    }

  let parseGroups groups : Either<IrisError, HostGroup list> =
    either {
      let! groups =
        Seq.fold
          (fun (m: Either<IrisError, HostGroup list>) group -> either {
            let! groups = m
            let! group = parseGroup group
            return group :: groups
          })
          (Right [])
          groups

      return List.reverse groups
    }

  /// ### Parse the Cluster configuration section
  ///
  /// Parse the cluster configuration section of a given configuration file into a `Cluster` value.
  ///
  /// # Returns: Cluster

  let private parseCluster (config: ConfigFile) : Either<IrisError, Cluster> =
    either {
      let cluster = config.Project.Cluster

      let! groups = parseGroups cluster.Groups
      let! nodes = parseNodes cluster.Nodes

      return { Name   = cluster.Name
               Nodes  = nodes
               Groups = groups }
    }

  /// ### Save a Cluster value to a configuration file
  ///
  /// Saves the passed `Cluster` value to the passed config file.
  ///
  /// # Returns: ConfigFile
  let saveCluster (file: ConfigFile, config: IrisConfig) =
    file.Project.Cluster.Nodes.Clear()
    file.Project.Cluster.Groups.Clear()
    file.Project.Cluster.Name <- config.ClusterConfig.Name

    for node in config.ClusterConfig.Nodes do
      let n = new ConfigFile.Project_Type.Cluster_Type.Nodes_Item_Type()
      n.Id       <- string node.Id
      n.Ip       <- string node.IpAddr
      n.HostName <- node.HostName
      n.Port     <- int node.Port
      n.WebPort  <- int node.WebPort
      n.WsPort   <- int node.WsPort
      n.GitPort  <- int node.GitPort
      n.State    <- string node.State
      file.Project.Cluster.Nodes.Add(n)

    for group in config.ClusterConfig.Groups do
      let g = new ConfigFile.Project_Type.Cluster_Type.Groups_Item_Type()
      g.Name <- group.Name

      for mem in group.Members do
        g.Members.Add(string mem)

      file.Project.Cluster.Groups.Add(g)
    (file, config)

  let fromFile (file: ConfigFile) : Either<IrisError, IrisConfig> =
    either {
      let! raftcfg   = parseRaft      file
      let! timing    = parseTiming    file
      let! vvvv      = parseVvvv      file
      let! audio     = parseAudio     file
      let! ports     = parsePort      file
      let! viewports = parseViewPorts file
      let! displays  = parseDisplays  file
      let! tasks     = parseTasks     file
      let! cluster   = parseCluster   file

      return { VvvvConfig    = vvvv
               AudioConfig   = audio
               RaftConfig    = raftcfg
               TimingConfig  = timing
               PortConfig    = ports
               ViewPorts     = viewports
               Displays      = displays
               Tasks         = tasks
               ClusterConfig = cluster }
    }

  let toFile (config: IrisConfig) (file: ConfigFile) =
    (file, config)
    |> saveVvvv
    |> saveAudio
    |> saveRaft
    |> saveTiming
    |> savePort
    |> saveViewPorts
    |> saveDisplays
    |> saveTasks
    |> saveCluster
    |> fst

  let create (name: string) =
    { VvvvConfig     = VvvvConfig.Default
    ; AudioConfig    = AudioConfig.Default
    ; RaftConfig     = RaftConfig.Default
    ; TimingConfig   = TimingConfig.Default
    ; PortConfig     = PortConfig.Default
    ; ViewPorts      = []
    ; Displays       = []
    ; Tasks          = []
    ; ClusterConfig  = { Name   = name + " cluster"
                       ; Nodes  = []
                       ; Groups = [] } }

  let updateVvvv (vvvv: VvvvConfig) (config: IrisConfig) =
    { config with VvvvConfig = vvvv }

  let updateAudio (audio: AudioConfig) (config: IrisConfig) =
    { config with AudioConfig = audio }

  let updateEngine (engine: RaftConfig) (config: IrisConfig) =
    { config with RaftConfig = engine }

  let updateTiming (timing: TimingConfig) (config: IrisConfig) =
    { config with TimingConfig = timing }

  let updatePorts (ports: PortConfig) (config: IrisConfig)=
    { config with PortConfig = ports }

  let updateViewPorts (viewports: ViewPort list) (config: IrisConfig) =
    { config with ViewPorts = viewports }

  let updateDisplays (displays: Display list) (config: IrisConfig) =
    { config with Displays = displays }

  let updateTasks (tasks: Task list) (config: IrisConfig) =
    { config with Tasks = tasks }

  let updateCluster (cluster: Cluster) (config: IrisConfig) =
    { config with ClusterConfig = cluster }

  let findNode (config: IrisConfig) (id: Id) =
    let result =
      List.tryFind
        (fun (node: RaftNode) -> node.Id = id)
        config.ClusterConfig.Nodes

    match result with
    | Some node -> Either.succeed node
    | _         -> MissingNode (string id) |> Either.fail

  let getNodes (config: IrisConfig) : Either<IrisError,RaftNode array> =
    config.ClusterConfig.Nodes
    |> Array.ofList
    |> Either.succeed

  let getNodeId () =
    let id = Environment.GetEnvironmentVariable IRIS_NODE_ID
    if isNull id then
      MissingNodeId |> Either.fail
    else
      Id id |> Either.succeed

  let selfNode (options: IrisConfig) =
    getNodeId ()
    |> Either.bind (findNode options)

  let addNode (node: RaftNode) (config: IrisConfig) =
    { config with
        ClusterConfig =
          { config.ClusterConfig with
              Nodes = node :: config.ClusterConfig.Nodes } }

  let removeNode (id: Id) (config: IrisConfig) =
    { config with
        ClusterConfig =
          { config.ClusterConfig with
              Nodes = List.filter
                        (fun (node: RaftNode) -> node.Id = id)
                        config.ClusterConfig.Nodes } }

  let metadataPath (config: IrisConfig) =
    config.RaftConfig.DataDir </> RAFT_METADATA_FILENAME + ASSET_EXTENSION

  let logDataPath (config: IrisConfig) =
    config.RaftConfig.DataDir </> RAFT_LOGDATA_PATH
