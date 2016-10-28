namespace Iris.Core

/// * imports
#if JAVASCRIPT

open Iris.Core.FlatBuffers
open Iris.Web.Core.FlatBufferTypes

#else

/// * more
open FlatBuffers
open Iris.Serialization.Raft

#endif

/// * IrisError Definition
type IrisError =
  | OK

  // ** GIT
  | BranchNotFound         of string
  | BranchDetailsNotFound  of string
  | RepositoryNotFound     of string
  | RepositoryInitFailed   of string
  | CommitError            of string
  | GitError               of string

  // PROJECT
  | ProjectNotFound        of string
  | ProjectParseError      of string
  | ProjectPathError
  | ProjectSaveError       of string
  | ProjectInitError       of string

  | MetaDataNotFound

  | ParseError             of string

  // CLI
  | MissingStartupDir
  | CliParseError

  /// Node:
  | MissingNodeId
  | MissingNode            of string

  | AssetNotFoundError     of string
  | AssetLoadError         of string
  | AssetSaveError         of string
  | AssetDeleteError       of string

  | Other                  of string

  /// RAFT:
  | AlreadyVoted
  | AppendEntryFailed
  | CandidateUnknown
  | EntryInvalidated
  | InvalidCurrentIndex
  | InvalidLastLog
  | InvalidLastLogTerm
  | InvalidTerm
  | LogFormatError
  | LogIncomplete
  | NoError
  | NoNode
  | NotCandidate
  | NotLeader
  | NotVotingState
  | ResponseTimeout
  | SnapshotFormatError
  | StaleResponse
  | UnexpectedVotingChange
  | VoteTermMismatch

  static member FromFB (fb: ErrorFB) =
    match fb.Type with
#if JAVASCRIPT
    | x when x = ErrorTypeFB.OKFB                     -> Right OK
    | x when x = ErrorTypeFB.BranchNotFoundFB         -> Right (BranchNotFound fb.Message)
    | x when x = ErrorTypeFB.BranchDetailsNotFoundFB  -> Right (BranchDetailsNotFound fb.Message)
    | x when x = ErrorTypeFB.RepositoryNotFoundFB     -> Right (RepositoryNotFound fb.Message)
    | x when x = ErrorTypeFB.RepositoryInitFailedFB   -> Right (RepositoryInitFailed fb.Message)
    | x when x = ErrorTypeFB.CommitErrorFB            -> Right (CommitError fb.Message)
    | x when x = ErrorTypeFB.GitErrorFB               -> Right (GitError fb.Message)
    | x when x = ErrorTypeFB.ProjectNotFoundFB        -> Right (ProjectNotFound fb.Message)
    | x when x = ErrorTypeFB.ProjectParseErrorFB      -> Right (ProjectParseError fb.Message)
    | x when x = ErrorTypeFB.ProjectPathErrorFB       -> Right ProjectPathError
    | x when x = ErrorTypeFB.ProjectSaveErrorFB       -> Right (ProjectSaveError fb.Message)
    | x when x = ErrorTypeFB.ProjectInitErrorFB       -> Right (ProjectInitError fb.Message)
    | x when x = ErrorTypeFB.MetaDataNotFoundFB       -> Right MetaDataNotFound
    | x when x = ErrorTypeFB.MissingStartupDirFB      -> Right MissingStartupDir
    | x when x = ErrorTypeFB.CliParseErrorFB          -> Right CliParseError
    | x when x = ErrorTypeFB.MissingNodeIdFB          -> Right MissingNodeId
    | x when x = ErrorTypeFB.MissingNodeFB            -> Right (MissingNode fb.Message)
    | x when x = ErrorTypeFB.AssetNotFoundErrorFB     -> Right (AssetNotFoundError fb.Message)
    | x when x = ErrorTypeFB.AssetLoadErrorFB         -> Right (AssetLoadError fb.Message)
    | x when x = ErrorTypeFB.AssetSaveErrorFB         -> Right (AssetSaveError fb.Message)
    | x when x = ErrorTypeFB.AssetDeleteErrorFB       -> Right (AssetDeleteError fb.Message)
    | x when x = ErrorTypeFB.OtherFB                  -> Right (Other fb.Message)
    | x when x = ErrorTypeFB.AlreadyVotedFB           -> Right AlreadyVoted
    | x when x = ErrorTypeFB.AppendEntryFailedFB      -> Right AppendEntryFailed
    | x when x = ErrorTypeFB.CandidateUnknownFB       -> Right CandidateUnknown
    | x when x = ErrorTypeFB.EntryInvalidatedFB       -> Right EntryInvalidated
    | x when x = ErrorTypeFB.InvalidCurrentIndexFB    -> Right InvalidCurrentIndex
    | x when x = ErrorTypeFB.InvalidLastLogFB         -> Right InvalidLastLog
    | x when x = ErrorTypeFB.InvalidLastLogTermFB     -> Right InvalidLastLogTerm
    | x when x = ErrorTypeFB.InvalidTermFB            -> Right InvalidTerm
    | x when x = ErrorTypeFB.LogFormatErrorFB         -> Right LogFormatError
    | x when x = ErrorTypeFB.LogIncompleteFB          -> Right LogIncomplete
    | x when x = ErrorTypeFB.NoErrorFB                -> Right NoError
    | x when x = ErrorTypeFB.NoNodeFB                 -> Right NoNode
    | x when x = ErrorTypeFB.NotCandidateFB           -> Right NotCandidate
    | x when x = ErrorTypeFB.NotLeaderFB              -> Right NotLeader
    | x when x = ErrorTypeFB.NotVotingStateFB         -> Right NotVotingState
    | x when x = ErrorTypeFB.ResponseTimeoutFB        -> Right ResponseTimeout
    | x when x = ErrorTypeFB.SnapshotFormatErrorFB    -> Right SnapshotFormatError
    | x when x = ErrorTypeFB.StaleResponseFB          -> Right StaleResponse
    | x when x = ErrorTypeFB.UnexpectedVotingChangeFB -> Right UnexpectedVotingChange
    | x when x = ErrorTypeFB.VoteTermMismatchFB       -> Right VoteTermMismatch
    | x when x = ErrorTypeFB.ParseErrorFB             -> Right (ParseError fb.Message)
    | x ->
      sprintf "Could not parse unknown ErrotTypeFB: %A" x
      |> ParseError
      |> Either.fail
#else
    | ErrorTypeFB.OKFB                     -> Right OK
    | ErrorTypeFB.BranchNotFoundFB         -> Right (BranchNotFound fb.Message)
    | ErrorTypeFB.BranchDetailsNotFoundFB  -> Right (BranchDetailsNotFound fb.Message)
    | ErrorTypeFB.RepositoryNotFoundFB     -> Right (RepositoryNotFound fb.Message)
    | ErrorTypeFB.RepositoryInitFailedFB   -> Right (RepositoryInitFailed fb.Message)
    | ErrorTypeFB.CommitErrorFB            -> Right (CommitError fb.Message)
    | ErrorTypeFB.GitErrorFB               -> Right (GitError fb.Message)
    | ErrorTypeFB.ProjectNotFoundFB        -> Right (ProjectNotFound fb.Message)
    | ErrorTypeFB.ProjectParseErrorFB      -> Right (ProjectParseError fb.Message)
    | ErrorTypeFB.ProjectPathErrorFB       -> Right ProjectPathError
    | ErrorTypeFB.ProjectSaveErrorFB       -> Right (ProjectSaveError fb.Message)
    | ErrorTypeFB.ProjectInitErrorFB       -> Right (ProjectInitError fb.Message)
    | ErrorTypeFB.MetaDataNotFoundFB       -> Right MetaDataNotFound
    | ErrorTypeFB.MissingStartupDirFB      -> Right MissingStartupDir
    | ErrorTypeFB.CliParseErrorFB          -> Right CliParseError
    | ErrorTypeFB.MissingNodeIdFB          -> Right MissingNodeId
    | ErrorTypeFB.MissingNodeFB            -> Right (MissingNode fb.Message)
    | ErrorTypeFB.AssetNotFoundErrorFB     -> Right (AssetNotFoundError fb.Message)
    | ErrorTypeFB.AssetLoadErrorFB         -> Right (AssetLoadError fb.Message)
    | ErrorTypeFB.AssetSaveErrorFB         -> Right (AssetSaveError fb.Message)
    | ErrorTypeFB.AssetDeleteErrorFB       -> Right (AssetDeleteError fb.Message)
    | ErrorTypeFB.OtherFB                  -> Right (Other fb.Message)
    | ErrorTypeFB.AlreadyVotedFB           -> Right AlreadyVoted
    | ErrorTypeFB.AppendEntryFailedFB      -> Right AppendEntryFailed
    | ErrorTypeFB.CandidateUnknownFB       -> Right CandidateUnknown
    | ErrorTypeFB.EntryInvalidatedFB       -> Right EntryInvalidated
    | ErrorTypeFB.InvalidCurrentIndexFB    -> Right InvalidCurrentIndex
    | ErrorTypeFB.InvalidLastLogFB         -> Right InvalidLastLog
    | ErrorTypeFB.InvalidLastLogTermFB     -> Right InvalidLastLogTerm
    | ErrorTypeFB.InvalidTermFB            -> Right InvalidTerm
    | ErrorTypeFB.LogFormatErrorFB         -> Right LogFormatError
    | ErrorTypeFB.LogIncompleteFB          -> Right LogIncomplete
    | ErrorTypeFB.NoErrorFB                -> Right NoError
    | ErrorTypeFB.NoNodeFB                 -> Right NoNode
    | ErrorTypeFB.NotCandidateFB           -> Right NotCandidate
    | ErrorTypeFB.NotLeaderFB              -> Right NotLeader
    | ErrorTypeFB.NotVotingStateFB         -> Right NotVotingState
    | ErrorTypeFB.ResponseTimeoutFB        -> Right ResponseTimeout
    | ErrorTypeFB.SnapshotFormatErrorFB    -> Right SnapshotFormatError
    | ErrorTypeFB.StaleResponseFB          -> Right StaleResponse
    | ErrorTypeFB.UnexpectedVotingChangeFB -> Right UnexpectedVotingChange
    | ErrorTypeFB.VoteTermMismatchFB       -> Right VoteTermMismatch
    | ErrorTypeFB.ParseErrorFB             -> Right (ParseError fb.Message)
    | x ->
      sprintf "Could not parse unknown ErrotTypeFB: %A" x
      |> ParseError
      |> Either.fail
#endif

  member error.ToOffset (builder: FlatBufferBuilder) =
    let tipe =
      match error with
      | OK                       -> ErrorTypeFB.OKFB
      | BranchNotFound         _ -> ErrorTypeFB.BranchNotFoundFB
      | BranchDetailsNotFound  _ -> ErrorTypeFB.BranchDetailsNotFoundFB
      | RepositoryNotFound     _ -> ErrorTypeFB.RepositoryNotFoundFB
      | RepositoryInitFailed   _ -> ErrorTypeFB.RepositoryInitFailedFB
      | CommitError            _ -> ErrorTypeFB.CommitErrorFB
      | GitError               _ -> ErrorTypeFB.GitErrorFB
      | ProjectNotFound        _ -> ErrorTypeFB.ProjectNotFoundFB
      | ProjectParseError      _ -> ErrorTypeFB.ProjectParseErrorFB
      | ProjectPathError         -> ErrorTypeFB.ProjectPathErrorFB
      | ProjectSaveError       _ -> ErrorTypeFB.ProjectSaveErrorFB
      | ProjectInitError       _ -> ErrorTypeFB.ProjectInitErrorFB
      | MetaDataNotFound         -> ErrorTypeFB.MetaDataNotFoundFB
      | MissingStartupDir        -> ErrorTypeFB.MissingStartupDirFB
      | CliParseError            -> ErrorTypeFB.CliParseErrorFB
      | MissingNodeId            -> ErrorTypeFB.MissingNodeIdFB
      | MissingNode            _ -> ErrorTypeFB.MissingNodeFB
      | AssetNotFoundError     _ -> ErrorTypeFB.AssetNotFoundErrorFB
      | AssetLoadError         _ -> ErrorTypeFB.AssetLoadErrorFB
      | AssetSaveError         _ -> ErrorTypeFB.AssetSaveErrorFB
      | AssetDeleteError       _ -> ErrorTypeFB.AssetDeleteErrorFB
      | ParseError             _ -> ErrorTypeFB.ParseErrorFB
      | Other                  _ -> ErrorTypeFB.OtherFB

      | AlreadyVoted             -> ErrorTypeFB.AlreadyVotedFB
      | AppendEntryFailed        -> ErrorTypeFB.AppendEntryFailedFB
      | CandidateUnknown         -> ErrorTypeFB.CandidateUnknownFB
      | EntryInvalidated         -> ErrorTypeFB.EntryInvalidatedFB
      | InvalidCurrentIndex      -> ErrorTypeFB.InvalidCurrentIndexFB
      | InvalidLastLog           -> ErrorTypeFB.InvalidLastLogFB
      | InvalidLastLogTerm       -> ErrorTypeFB.InvalidLastLogTermFB
      | InvalidTerm              -> ErrorTypeFB.InvalidTermFB
      | LogFormatError           -> ErrorTypeFB.LogFormatErrorFB
      | LogIncomplete            -> ErrorTypeFB.LogIncompleteFB
      | NoError                  -> ErrorTypeFB.NoErrorFB
      | NoNode                   -> ErrorTypeFB.NoNodeFB
      | NotCandidate             -> ErrorTypeFB.NotCandidateFB
      | NotLeader                -> ErrorTypeFB.NotLeaderFB
      | NotVotingState           -> ErrorTypeFB.NotVotingStateFB
      | ResponseTimeout          -> ErrorTypeFB.ResponseTimeoutFB
      | SnapshotFormatError      -> ErrorTypeFB.SnapshotFormatErrorFB
      | StaleResponse            -> ErrorTypeFB.StaleResponseFB
      | UnexpectedVotingChange   -> ErrorTypeFB.UnexpectedVotingChangeFB
      | VoteTermMismatch         -> ErrorTypeFB.VoteTermMismatchFB

    let str =
      match error with
      | BranchNotFound         msg -> builder.CreateString msg |> Some
      | BranchDetailsNotFound  msg -> builder.CreateString msg |> Some
      | RepositoryNotFound     msg -> builder.CreateString msg |> Some
      | RepositoryInitFailed   msg -> builder.CreateString msg |> Some
      | CommitError            msg -> builder.CreateString msg |> Some
      | GitError               msg -> builder.CreateString msg |> Some
      | ProjectNotFound        msg -> builder.CreateString msg |> Some
      | ProjectParseError      msg -> builder.CreateString msg |> Some
      | ProjectSaveError       msg -> builder.CreateString msg |> Some
      | ProjectInitError       msg -> builder.CreateString msg |> Some
      | MissingNode            msg -> builder.CreateString msg |> Some
      | AssetNotFoundError     msg -> builder.CreateString msg |> Some
      | AssetSaveError         msg -> builder.CreateString msg |> Some
      | AssetLoadError         msg -> builder.CreateString msg |> Some
      | AssetDeleteError       msg -> builder.CreateString msg |> Some
      | ParseError             msg -> builder.CreateString msg |> Some
      | Other                  msg -> builder.CreateString msg |> Some
      | _                          -> None

    ErrorFB.StartErrorFB(builder)
    ErrorFB.AddType(builder, tipe)
    match str with
    | Some payload -> ErrorFB.AddMessage(builder, payload)
    | _            -> ()
    ErrorFB.EndErrorFB(builder)

  member self.ToBytes() = Binary.buildBuffer self

  static member FromBytes(bytes: Binary.Buffer) =
    Binary.createBuffer bytes
    |> ErrorFB.GetRootAsErrorFB
    |> IrisError.FromFB

[<RequireQualifiedAccess>]
module Error =

  let inline toMessage (error: IrisError) =
    match error with
    | BranchNotFound        e -> sprintf "Branch does not exist: %s" e
    | BranchDetailsNotFound e -> sprintf "Branch details could not be found: %s" e
    | RepositoryNotFound    e -> sprintf "Repository was not found: %s" e
    | RepositoryInitFailed  e -> sprintf "Repository could not be initialized: %s" e
    | CommitError           e -> sprintf "Could not commit changes: %s" e
    | GitError              e -> sprintf "Git error: %s" e

    | ProjectNotFound       e -> sprintf "Project could not be found: %s" e
    | ProjectPathError        ->         "Project has no path"
    | ProjectSaveError      e -> sprintf "Project could not be saved: %s" e
    | ProjectParseError     e -> sprintf "Project could not be parsed: %s" e

    | ParseError            e -> sprintf "Parse error: %s" e

    // LITEDB
    | ProjectInitError      e -> sprintf "Database could not be created: %s" e
    | MetaDataNotFound        -> sprintf "Metadata could not be loaded from db"

    // CLI
    | MissingStartupDir       ->         "Startup directory missing"
    | CliParseError           ->         "Command line parse error"

    | MissingNodeId           ->         "Node Id missing in environment"
    | MissingNode           e -> sprintf "Node with Id %s missing in Project configuration" e

    | AssetNotFoundError    e -> sprintf "Could not find asset on disk: %s" e
    | AssetSaveError        e -> sprintf "Could not save asset to disk: %s" e
    | AssetLoadError        e -> sprintf "Could not load asset to disk: %s" e
    | AssetDeleteError      e -> sprintf "Could not delete asset from disl: %s" e

    | Other                 e -> sprintf "Other error occurred: %s" (string e)

    // RAFT
    | AlreadyVoted            -> "Already voted"
    | AppendEntryFailed       -> "AppendEntry request has failed"
    | CandidateUnknown        -> "Election candidate not known to Raft"
    | EntryInvalidated        -> "Entry was invalidated"
    | InvalidCurrentIndex     -> "Invalid CurrentIndex"
    | InvalidLastLog          -> "Invalid last log"
    | InvalidLastLogTerm      -> "Invalid last log term"
    | InvalidTerm             -> "Invalid term"
    | LogFormatError          -> "Log format error"
    | LogIncomplete           -> "Log is incomplete"
    | NoError                 -> "No error"
    | NoNode                  -> "No node"
    | NotCandidate            -> "Not currently candidate"
    | NotLeader               -> "Not currently leader"
    | NotVotingState          -> "Not in voting state"
    | ResponseTimeout         -> "Response timeout"
    | SnapshotFormatError     -> "Snapshot was malformed"
    | StaleResponse           -> "Unsolicited response"
    | UnexpectedVotingChange  -> "Unexpected voting change"
    | VoteTermMismatch        -> "Vote term mismatch"

    | OK                      -> "All good."

  let inline toExitCode (error: IrisError) =
    match error with
    | OK                      -> 0
    | BranchNotFound        _ -> 1
    | BranchDetailsNotFound _ -> 2
    | RepositoryNotFound    _ -> 3
    | RepositoryInitFailed  _ -> 4
    | CommitError           _ -> 5
    | GitError              _ -> 6

    | ProjectNotFound       _ -> 7
    | ProjectPathError        -> 8
    | ProjectSaveError      _ -> 9
    | ProjectParseError     _ -> 10

    | MissingNodeId         _ -> 11
    | MissingNode           _ -> 12

    // LITEDB
    | ProjectInitError      _ -> 13
    | MetaDataNotFound        -> 14

    // CLI
    | MissingStartupDir       -> 15
    | CliParseError           -> 16

    | AssetNotFoundError    _ -> 18
    | AssetSaveError        _ -> 19
    | AssetLoadError        _ -> 19
    | AssetDeleteError      _ -> 21

    | ParseError            _ -> 21

    | Other                 _ -> 22

    // RAFT
    | AlreadyVoted            -> 23
    | AppendEntryFailed       -> 24
    | CandidateUnknown        -> 25
    | EntryInvalidated        -> 26
    | InvalidCurrentIndex     -> 27
    | InvalidLastLog          -> 28
    | InvalidLastLogTerm      -> 29
    | InvalidTerm             -> 30
    | LogFormatError          -> 31
    | LogIncomplete           -> 32
    | NoError                 -> 33
    | NoNode                  -> 34
    | NotCandidate            -> 35
    | NotLeader               -> 36
    | NotVotingState          -> 37
    | ResponseTimeout         -> 38
    | SnapshotFormatError     -> 39
    | StaleResponse           -> 40
    | UnexpectedVotingChange  -> 41
    | VoteTermMismatch        -> 42

  let inline isOk (error: IrisError) =
    match error with
    | OK -> true
    | _  -> false

  let inline exitWith (error: IrisError) =
    if not (isOk error) then
      toMessage error
      |> printfn "Fatal: %s"
    error |> toExitCode |> exit

  let throw (error: IrisError) =
    failwithf "ERROR: %A" error

  /// ## Exit with an exit code on failure
  ///
  /// Apply function `f` to inner value of `a` *if* `a` is a success,
  /// otherwise exit with an exit code derived from the error value.
  ///
  /// ### Signature:
  /// - `f`: function to apply to inner value of `a`
  /// - `a`: value to apply function
  ///
  /// Returns: ^b
  let inline orExit< ^a, ^b >
                   (f: ^a -> ^b)
                   (a: Either< IrisError, ^a>)
                   : ^b =
    match a with
    | Right value -> f value
    | Left  error -> exitWith error
