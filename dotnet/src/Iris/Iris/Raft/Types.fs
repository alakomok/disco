namespace Iris.Raft

open System
open System.Net
open Iris.Core
open Iris.Serialization.Raft
open FlatBuffers
open SharpYaml.Serialization

/// The Raft state machine
///
/// ## States
///  - `None`     - hm
///  - `Follower` - this Node is currently following a different Leader
///  - `Candiate` - this Node currently seeks to become Leader
///  - `Leader`   - this Node currently is Leader of the cluster
type RaftState =
  | Follower
  | Candidate
  | Leader

  override self.ToString() =
    sprintf "%A" self

  static member Parse str =
    match str with
    | "Follower"  -> Follower
    | "Candidate" -> Candidate
    | "Leader"    -> Leader
    | _           -> failwithf "unable to parse %A as RaftState" str

/// Response to an AppendEntry request
///
/// ## Constructor:
///  - `Id`    - the generated unique identified for the entry
///  - `Term`  - the entry's term
///  - `Index` - the entry's index in the log
type EntryResponse =
  {  Id    : Id
  ;  Term  : Term
  ;  Index : Index }

  with
    override self.ToString() =
      sprintf "Entry added with Id: %A in term: %d at log index: %d"
        (string self.Id)
        self.Term
        self.Index

[<RequireQualifiedAccess>]
module Entry =
  let inline id    (er : EntryResponse) = er.Id
  let inline term  (er : EntryResponse) = er.Term
  let inline index (er : EntryResponse) = er.Index

// __     __    _       ____                            _
// \ \   / /__ | |_ ___|  _ \ ___  __ _ _   _  ___  ___| |_
//  \ \ / / _ \| __/ _ \ |_) / _ \/ _` | | | |/ _ \/ __| __|
//   \ V / (_) | ||  __/  _ <  __/ (_| | |_| |  __/\__ \ |_
//    \_/ \___/ \__\___|_| \_\___|\__, |\__,_|\___||___/\__|
//                                   |_|

/// Request to Vote for a new Leader
///
/// ## Vote:
///  - `Term`         -  the current term, to force any other leader/candidate to step down
///  - `Candidate`    -  the unique node id of candidate for leadership
///  - `LastLogIndex` -  the index of the candidates last log entry
///  - `LastLogTerm`  -  the index of the candidates last log entry
type VoteRequest =
  { Term         : Term
  ; Candidate    : RaftNode
  ; LastLogIndex : Index
  ; LastLogTerm  : Term
  }

  with
    member self.ToOffset(builder: FlatBufferBuilder) =
      let node = self.Candidate.ToOffset(builder)
      VoteRequestFB.StartVoteRequestFB(builder)
      VoteRequestFB.AddTerm(builder, self.Term)
      VoteRequestFB.AddLastLogTerm(builder, self.LastLogTerm)
      VoteRequestFB.AddLastLogIndex(builder, self.LastLogIndex)
      VoteRequestFB.AddCandidate(builder, node)
      VoteRequestFB.EndVoteRequestFB(builder)

    static member FromFB (fb: VoteRequestFB) : Either<IrisError, VoteRequest> =
      either {
        let candidate = fb.Candidate
        if candidate.HasValue then
          let! node = RaftNode.FromFB candidate.Value
          return { Term         = fb.Term
                   Candidate    = node
                   LastLogIndex = fb.LastLogIndex
                   LastLogTerm  = fb.LastLogTerm }
        else
          return!
            "Could not parse empty NodeFB"
            |> ParseError
            |> Either.fail
      }

// __     __    _       ____
// \ \   / /__ | |_ ___|  _ \ ___  ___ _ __   ___  _ __  ___  ___
//  \ \ / / _ \| __/ _ \ |_) / _ \/ __| '_ \ / _ \| '_ \/ __|/ _ \
//   \ V / (_) | ||  __/  _ <  __/\__ \ |_) | (_) | | | \__ \  __/
//    \_/ \___/ \__\___|_| \_\___||___/ .__/ \___/|_| |_|___/\___|
//                                    |_|

/// Result of a vote
///
/// ## Result:
///  - `Term`    - current term for candidate to apply
///  - `Granted` - result of vote
type VoteResponse =
  { Term    : Term
  ; Granted : bool
  ; Reason  : IrisError option
  }

  static member FromFB (fb: VoteResponseFB) : Either<IrisError, VoteResponse> =
    either {
      let! reason =
        let reason = fb.Reason
        if reason.HasValue then
          IrisError.FromFB reason.Value
          |> Either.map Some
        else
          Right None

      return { Term    = fb.Term
               Granted = fb.Granted
               Reason  = reason }
    }

  member self.ToOffset(builder: FlatBufferBuilder) =
    let err = Option.map (fun (r: IrisError) -> r.ToOffset(builder)) self.Reason
    VoteResponseFB.StartVoteResponseFB(builder)
    VoteResponseFB.AddTerm(builder, self.Term)
    match err with
      | Some offset -> VoteResponseFB.AddReason(builder, offset)
      | _ -> ()
    VoteResponseFB.AddGranted(builder, self.Granted)
    VoteResponseFB.EndVoteResponseFB(builder)


[<RequireQualifiedAccess>]
module Vote =
  // requests
  let inline term         (vote : VoteRequest) = vote.Term
  let inline candidate    (vote : VoteRequest) = vote.Candidate
  let inline lastLogIndex (vote : VoteRequest) = vote.LastLogIndex
  let inline lastLogTerm  (vote : VoteRequest) = vote.LastLogTerm

  // responses
  let inline granted  (vote : VoteResponse) = vote.Granted
  let inline declined (vote : VoteResponse) = not vote.Granted



//     _                               _ _____       _        _
//    / \   _ __  _ __   ___ _ __   __| | ____|_ __ | |_ _ __(_) ___  ___
//   / _ \ | '_ \| '_ \ / _ \ '_ \ / _` |  _| | '_ \| __| '__| |/ _ \/ __|
//  / ___ \| |_) | |_) |  __/ | | | (_| | |___| | | | |_| |  | |  __/\__ \
// /_/   \_\ .__/| .__/ \___|_| |_|\__,_|_____|_| |_|\__|_|  |_|\___||___/
//         |_|   |_|

/// AppendEntries message.
///
/// This message is used to tell nodes if it's safe to apply entries to the
/// FSM. Can be sent without any entries as a keep alive message.  This
/// message could force a leader/candidate to become a follower.
///
/// ## Message:
///  - `Term`        - currentTerm, to force other leader/candidate to step down
///  - `PrevLogIdx`  - the index of the log just before the newest entry for the node who receive this message
///  - `PrevLogTerm` - the term of the log just before the newest entry for the node who receives this message
///  - `LeaderCommit`- the index of the entry that has been appended to the majority of the cluster. Entries up to this index will be applied to the FSM
type AppendEntries =
  { Term         : Term
  ; PrevLogIdx   : Index
  ; PrevLogTerm  : Term
  ; LeaderCommit : Index
  ; Entries      : RaftLogEntry option
  }

  with
    static member FromFB (fb: AppendEntriesFB) : Either<IrisError,AppendEntries> =
      either {
        let! entries =
          if fb.EntriesLength = 0 then
            Either.succeed None
          else
            let raw = Array.zeroCreate fb.EntriesLength
            for i in 0 .. (fb.EntriesLength - 1) do
              let entry = fb.Entries(i)
              if entry.HasValue then
                raw.[i] <- entry.Value
            RaftLogEntry.FromFB raw

        return { Term         = fb.Term
                 PrevLogIdx   = fb.PrevLogIdx
                 PrevLogTerm  = fb.PrevLogTerm
                 LeaderCommit = fb.LeaderCommit
                 Entries      = entries }
      }

    member self.ToOffset(builder: FlatBufferBuilder) =
      let entries =
        Option.map
          (fun (entries: RaftLogEntry) ->
            let offsets = entries.ToOffset(builder)
            AppendEntriesFB.CreateEntriesVector(builder, offsets))
          self.Entries

      AppendEntriesFB.StartAppendEntriesFB(builder)
      AppendEntriesFB.AddTerm(builder, self.Term)
      AppendEntriesFB.AddPrevLogTerm(builder, self.PrevLogTerm)
      AppendEntriesFB.AddPrevLogIdx(builder, self.PrevLogIdx)
      AppendEntriesFB.AddLeaderCommit(builder, self.LeaderCommit)

      Option.map (fun offset -> AppendEntriesFB.AddEntries(builder, offset)) entries
      |> ignore

      AppendEntriesFB.EndAppendEntriesFB(builder)

//     _                               _ ____
//    / \   _ __  _ __   ___ _ __   __| |  _ \ ___  ___ _ __   ___  _ __  ___  ___
//   / _ \ | '_ \| '_ \ / _ \ '_ \ / _` | |_) / _ \/ __| '_ \ / _ \| '_ \/ __|/ _ \
//  / ___ \| |_) | |_) |  __/ | | | (_| |  _ <  __/\__ \ |_) | (_) | | | \__ \  __/
// /_/   \_\ .__/| .__/ \___|_| |_|\__,_|_| \_\___||___/ .__/ \___/|_| |_|___/\___|
//         |_|   |_|                                   |_|

/// Appendentries response message.
///
/// an be sent without any entries as a keep alive message.
/// his message could force a leader/candidate to become a follower.
///
/// ## Response Message:
///  - `Term`       - currentTerm, to force other leader/candidate to step down
///  - `Success`    - true if follower contained entry matching prevLogidx and prevLogTerm
///  - `CurrentIdx` - This is the highest log IDX we've received and appended to our log
///  - `FirstIdx`   - The first idx that we received within the appendentries message
type AppendResponse =
  { Term         : Term
  ; Success      : bool
  ; CurrentIndex : Index
  ; FirstIndex   : Index
  }

  static member FromFB (fb: AppendResponseFB) : Either<IrisError,AppendResponse> =
    Right { Term         = fb.Term
            Success      = fb.Success
            CurrentIndex = fb.CurrentIndex
            FirstIndex   = fb.FirstIndex }

  member self.ToOffset(builder: FlatBufferBuilder) =
    AppendResponseFB.StartAppendResponseFB(builder)
    AppendResponseFB.AddTerm(builder, self.Term)
    AppendResponseFB.AddSuccess(builder, self.Success)
    AppendResponseFB.AddFirstIndex(builder, self.FirstIndex)
    AppendResponseFB.AddCurrentIndex(builder, self.CurrentIndex)
    AppendResponseFB.EndAppendResponseFB(builder)

[<RequireQualifiedAccess>]
module AppendRequest =
  let inline term ar = ar.Term
  let inline succeeded ar = ar.Success
  let inline failed ar = not ar.Success
  let inline firstIndex ar = ar.FirstIndex
  let inline currentIndex ar = ar.CurrentIndex

  let inline numEntries ar =
    match ar.Entries with
      | Some entries -> LogEntry.depth entries
      | _            -> 0u

  let inline prevLogIndex ae = ae.PrevLogIdx
  let inline prevLogTerm ae = ae.PrevLogTerm

//  ___           _        _ _ ____                        _           _
// |_ _|_ __  ___| |_ __ _| | / ___| _ __   __ _ _ __  ___| |__   ___ | |_
//  | || '_ \/ __| __/ _` | | \___ \| '_ \ / _` | '_ \/ __| '_ \ / _ \| __|
//  | || | | \__ \ || (_| | | |___) | | | | (_| | |_) \__ \ | | | (_) | |_
// |___|_| |_|___/\__\__,_|_|_|____/|_| |_|\__,_| .__/|___/_| |_|\___/ \__|
//                                              |_|

type InstallSnapshot =
  { Term      : Term
  ; LeaderId  : NodeId
  ; LastIndex : Index
  ; LastTerm  : Term
  ; Data      : RaftLogEntry }

  with
    member self.ToOffset (builder: FlatBufferBuilder) =
      let data = InstallSnapshotFB.CreateDataVector(builder, self.Data.ToOffset(builder))
      let leaderid = string self.LeaderId |> builder.CreateString

      InstallSnapshotFB.StartInstallSnapshotFB(builder)
      InstallSnapshotFB.AddTerm(builder, self.Term)
      InstallSnapshotFB.AddLeaderId(builder, leaderid)
      InstallSnapshotFB.AddLastTerm(builder, self.LastTerm)
      InstallSnapshotFB.AddLastIndex(builder, self.LastIndex)
      InstallSnapshotFB.AddData(builder, data)
      InstallSnapshotFB.EndInstallSnapshotFB(builder)

    static member FromFB (fb: InstallSnapshotFB) =
      either  {
        let! decoded =
          if fb.DataLength > 0 then
            let raw = Array.zeroCreate fb.DataLength
            for i in 0 .. (fb.DataLength - 1) do
              let data = fb.Data(i)
              if data.HasValue then
                raw.[i] <- data.Value
            RaftLogEntry.FromFB raw
          else
            "Invalid InstallSnapshot (no log data)"
            |> ParseError
            |> Either.fail

        match decoded with
        | Some entries ->
          return
            { Term      = fb.Term
              LeaderId  = Id fb.LeaderId
              LastIndex = fb.LastIndex
              LastTerm  = fb.LastTerm
              Data      = entries }
        | _ ->
          return!
            "Invalid InstallSnapshot (no log data)"
            |> ParseError
            |> Either.fail
      }

/////////////////////////////////////////////////
//   ____      _ _ _                _          //
//  / ___|__ _| | | |__   __ _  ___| | __      //
// | |   / _` | | | '_ \ / _` |/ __| |/ /      //
// | |__| (_| | | | |_) | (_| | (__|   <       //
//  \____\__,_|_|_|_.__/ \__,_|\___|_|\_\      //
//                                             //
//  ___       _             __                 //
// |_ _|_ __ | |_ ___ _ __ / _| __ _  ___ ___  //
//  | || '_ \| __/ _ \ '__| |_ / _` |/ __/ _ \ //
//  | || | | | ||  __/ |  |  _| (_| | (_|  __/ //
// |___|_| |_|\__\___|_|  |_|  \__,_|\___\___| //
/////////////////////////////////////////////////

type IRaftCallbacks =

  /// Request a vote from given Raft server
  abstract member SendRequestVote:     RaftNode  -> VoteRequest            -> VoteResponse option

  /// Send AppendEntries message to given server
  abstract member SendAppendEntries:   RaftNode  -> AppendEntries          -> AppendResponse option

  /// Send InstallSnapshot command to given serve
  abstract member SendInstallSnapshot: RaftNode  -> InstallSnapshot        -> AppendResponse option

  /// given the current state of Raft, prepare and return a snapshot value of
  /// current application state
  abstract member PrepareSnapshot:     RaftValue       -> RaftLog option

  /// perist the given Snapshot value to disk. For safety reasons this MUST
  /// flush all changes to disk.
  abstract member PersistSnapshot:     RaftLogEntry    -> unit

  /// attempt to load a snapshot from disk. return None if no snapshot was found
  abstract member RetrieveSnapshot:    unit            -> RaftLogEntry option

  /// apply the given command to state machine
  abstract member ApplyLog:            StateMachine    -> unit

  /// a new server was added to the configuration
  abstract member NodeAdded:           RaftNode        -> unit

  /// a new server was added to the configuration
  abstract member NodeUpdated:         RaftNode        -> unit

  /// a server was removed from the configuration
  abstract member NodeRemoved:         RaftNode        -> unit

  /// a cluster configuration transition was successfully applied
  abstract member Configured:          RaftNode array  -> unit

  /// the state of Raft itself has changed from old state to new given state
  abstract member StateChanged:        RaftState       -> RaftState              -> unit

  /// persist vote data to disk. For safety reasons this callback MUST flush
  /// the change to disk.
  abstract member PersistVote:         RaftNode option -> unit

  /// persist term data to disk. For safety reasons this callback MUST flush
  /// the change to disk>
  abstract member PersistTerm:         Term            -> unit

  /// persist an entry added to the log to disk. For safety reasons this
  /// callback MUST flush the change to disk.
  abstract member PersistLog:          RaftLogEntry        -> unit

  /// persist the removal of the passed entry from the log to disk. For safety
  /// reasons this callback MUST flush the change to disk.
  abstract member DeleteLog:           RaftLogEntry        -> unit

  /// Callback for catching debug messsages
  abstract member LogMsg:  LogLevel ->  RaftNode        -> String                 -> unit

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  ____        __ _                                                                                                                     //
// |  _ \ __ _ / _| |_                                                                                                                   //
// | |_) / _` | |_| __|                                                                                                                  //
// |  _ < (_| |  _| |_                                                                                                                   //
// |_| \_\__,_|_|  \__|                                                                                                                  //
//                                                                                                                                       //
// ## Raft Server:                                                                                                                       //
//  - `Node`                  - the server's own node information                                                                        //
//  - `RaftState`             - follower/leader/candidate indicator                                                                  //
//  - `CurrentTerm`           - the server's best guess of what the current term is starts at zero                                       //
//  - `CurrentLeader`         - what this node thinks is the node ID of the current leader, or -1 if there isn't a known current leader. //
//  - `Peers`                 - list of all known nodes                                                                                  //
//  - `NumNodes`              - number of currently known peers                                                                          //
//  - `VotedFor`              - the candidate the server voted for in its current term or None if it hasn't voted for any yet            //
//  - `Log`                   - the log which is replicated                                                                              //
//  - `CommitIdx`             - idx of highest log entry known to be committed                                                           //
//  - `LastAppliedIdx`        - idx of highest log entry applied to state machine                                                        //
//  - `TimoutElapsed`         - amount of time left till timeout                                                                         //
//  - `ElectionTimeout`       - amount of time left till we start a new election                                                         //
//  - `RequestTimeout`        - amount of time left till we consider request to be failed                                                //
//  - `Callbacks`             - all callbacks to be invoked                                                                              //
//  - `MaxLogDepth`           - maximum log depth to reach before snapshotting triggers
//  - `VotingCfgChangeLogIdx` - the log which has a voting cfg change, otherwise None                                                    //
//                                                                                                                                       //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

and RaftValueYaml() =
  [<DefaultValue>] val mutable Node            : string
  [<DefaultValue>] val mutable Term            : Term
  [<DefaultValue>] val mutable Leader          : string
  [<DefaultValue>] val mutable Peers           : RaftNodeYaml array
  [<DefaultValue>] val mutable VotedFor        : string
  [<DefaultValue>] val mutable Log             : RaftLogYaml
  [<DefaultValue>] val mutable ElectionTimeout : Long
  [<DefaultValue>] val mutable RequestTimeout  : Long
  [<DefaultValue>] val mutable MaxLogDepth     : Long

and RaftValue =
  { Node              : RaftNode
  ; State             : RaftState
  ; CurrentTerm       : Term
  ; CurrentLeader     : NodeId option
  ; Peers             : Map<NodeId,RaftNode>
  ; OldPeers          : Map<NodeId,RaftNode> option
  ; NumNodes          : Long
  ; VotedFor          : NodeId option
  ; Log               : RaftLog
  ; CommitIndex       : Index
  ; LastAppliedIdx    : Index
  ; TimeoutElapsed    : Long
  ; ElectionTimeout   : Long
  ; RequestTimeout    : Long
  ; MaxLogDepth       : Long
  ; ConfigChangeEntry : RaftLogEntry option
  }

  override self.ToString() =
    sprintf "Node              = %s
State             = %A
CurrentTerm       = %A
CurrentLeader     = %A
NumNodes          = %A
VotedFor          = %A
MaxLogDepth       = %A
CommitIndex       = %A
LastAppliedIdx    = %A
TimeoutElapsed    = %A
ElectionTimeout   = %A
RequestTimeout    = %A
ConfigChangeEntry = %s
"
      (self.Node.ToString())
      self.State
      self.CurrentTerm
      self.CurrentLeader
      self.NumNodes
      self.VotedFor
      self.MaxLogDepth
      self.CommitIndex
      self.LastAppliedIdx
      self.TimeoutElapsed
      self.ElectionTimeout
      self.RequestTimeout
      (if Option.isSome self.ConfigChangeEntry then
        Option.get self.ConfigChangeEntry |> string
       else "<empty>")

  member self.IsLeader
    with get () =
      match self.CurrentLeader with
      | Some lid -> self.Node.Id = lid
      | _ -> false

  // __   __              _
  // \ \ / /_ _ _ __ ___ | |
  //  \ V / _` | '_ ` _ \| |
  //   | | (_| | | | | | | |
  //   |_|\__,_|_| |_| |_|_|

  member self.ToYaml(serializer: Serializer) =
    self |> Yaml.toYaml |> serializer.Serialize

  member self.ToYamlObject() =
    let yaml = new RaftValueYaml()
    yaml.Node <- string self.Node.Id
    yaml.Term <- self.CurrentTerm

    Option.map
      (fun leader -> yaml.Leader <- string leader)
      self.CurrentLeader
    |> ignore

    yaml.Peers <- self.Peers
                 |> Map.toArray
                 |> Array.map (snd >> Yaml.toYaml)

    Option.map
      (fun voted -> yaml.VotedFor <- string voted)
      self.VotedFor
    |> ignore

    yaml.Log <- Yaml.toYaml self.Log
    yaml.ElectionTimeout <- self.ElectionTimeout
    yaml.RequestTimeout <- self.RequestTimeout
    yaml.MaxLogDepth <- self.MaxLogDepth
    yaml

  static member FromYamlObject (yaml: RaftValueYaml) : Either<IrisError, RaftValue> =
    either {
      let leader =
        if isNull yaml.Leader then
          None
        else
          Some (Id yaml.Leader)

      let votedfor =
        if isNull yaml.VotedFor then
          None
        else
          Some (Id yaml.VotedFor)

      let! nodes =
        Array.fold
          (fun (m: Either<IrisError, Map<Id, RaftNode>>) yml -> either {
            let! nodes = m
            let! (node : RaftNode) = Yaml.fromYaml yml
            return (Map.add node.Id node nodes)
          })
          (Right Map.empty)
          yaml.Peers

      let! log = Yaml.fromYaml yaml.Log
      let node = Map.tryFind (Id yaml.Node) nodes

      match node with
      | Some node ->
        return { Node              = node
                 State             = Follower
                 CurrentTerm       = yaml.Term
                 CurrentLeader     = leader
                 Peers             = nodes
                 OldPeers          = None
                 NumNodes          = uint32 yaml.Peers.Length
                 VotedFor          = votedfor
                 Log               = log
                 CommitIndex       = Log.index log
                 LastAppliedIdx    = Log.index log
                 TimeoutElapsed    = 0u
                 ElectionTimeout   = yaml.ElectionTimeout
                 RequestTimeout    = yaml.RequestTimeout
                 MaxLogDepth       = yaml.MaxLogDepth
                 ConfigChangeEntry = None }
      | _ ->
        return!
          "Could not current node in config"
          |> ParseError
          |> Either.fail
    }

  static member FromYaml (str: string) : Either<IrisError, RaftValue> =
    let serializer = new Serializer()
    serializer.Deserialize<RaftValueYaml>(str)
    |> Yaml.fromYaml

////////////////////////////////////////
//  __  __                       _    //
// |  \/  | ___  _ __   __ _  __| |   //
// | |\/| |/ _ \| '_ \ / _` |/ _` |   //
// | |  | | (_) | | | | (_| | (_| |   //
// |_|  |_|\___/|_| |_|\__,_|\__,_|   //
////////////////////////////////////////

[<NoComparison;NoEquality>]
type RaftMonad<'Env,'State,'T,'Error> =
  MkRM of ('Env -> 'State -> Either<'Error * 'State,'T * 'State>)

type RaftM<'t,'err> =
  RaftMonad<IRaftCallbacks, RaftValue, 't, 'err>
