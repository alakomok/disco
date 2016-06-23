namespace Pallet.Core

open System
open System.Net

//  _   _           _      ____  _        _
// | \ | | ___   __| | ___/ ___|| |_ __ _| |_ ___
// |  \| |/ _ \ / _` |/ _ \___ \| __/ _` | __/ _ \
// | |\  | (_) | (_| |  __/___) | || (_| | ||  __/
// |_| \_|\___/ \__,_|\___|____/ \__\__,_|\__\___|

type NodeState =
  | Joining                             // excludes node from voting
  | Running                             // normal execution state
  | Failed                              // node has failed for some reason

//  _   _           _
// | \ | | ___   __| | ___
// |  \| |/ _ \ / _` |/ _ \
// | |\  | (_) | (_| |  __/
// |_| \_|\___/ \__,_|\___|

type Node<'node> =
  { Id         : NodeId
  ; Data       : 'node
  ; Voting     : bool
  ; VotedForMe : bool
  ; State      : NodeState
  ; nextIndex  : Index
  ; matchIndex : Index
  }

  override self.ToString() =
    sprintf "Node: [id: %d] [state: %A] [voting: %b] [voted for me: %b]"
      self.Id
      self.State
      self.Voting
      self.VotedForMe

//   ____             __ _          ____ _
//  / ___|___  _ __  / _(_) __ _   / ___| |__   __ _ _ __   __ _  ___
// | |   / _ \| '_ \| |_| |/ _` | | |   | '_ \ / _` | '_ \ / _` |/ _ \
// | |__| (_) | | | |  _| | (_| | | |___| | | | (_| | | | | (_| |  __/
//  \____\___/|_| |_|_| |_|\__, |  \____|_| |_|\__,_|_| |_|\__, |\___|
//                         |___/                           |___/

type ConfigChange<'n> =
  | NodeAdded   of Node<'n>
  | NodeRemoved of Node<'n>

[<RequireQualifiedAccess>]
module Node =

  let create id data =
    { Id         = id
    ; Data       = data
    ; State      = Running
    ; Voting     = true
    ; VotedForMe = false
    ; nextIndex  = 1u
    ; matchIndex = 0u
    }

  let isVoting (node : Node<'node>) : bool =
    node.State = Running && node.Voting

  let setVoting node voting =
    { node with Voting = voting }

  let voteForMe node vote =
    { node with VotedForMe = vote }

  let hasVoteForMe node = node.VotedForMe

  let setHasSufficientLogs node =
    { node with
        State = Running
        Voting = true }

  let hasSufficientLogs node =
    node.State = Running

  let canVote peer =
    isVoting peer && hasVoteForMe peer && peer.State = Running

  let getId node = node.Id
  let getData node = node.Data
  let getState node = node.State
  let getNextIndex  node = node.nextIndex
  let getMatchIndex node = node.matchIndex

  let private added oldnodes newnodes =
    let folder changes (node: Node<_>) =
      match Array.tryFind (getId >> ((=) node.Id)) oldnodes with
        | Some _ -> changes
        | _ -> NodeAdded(node) :: changes
    Array.fold folder List.empty newnodes

  let private removed oldnodes newnodes =
    let folder changes (node: Node<_>) =
      match Array.tryFind (getId >> ((=) node.Id)) newnodes with
        | Some _ -> changes
        | _ -> NodeAdded(node) :: changes
    Array.fold folder List.empty oldnodes

  let changes (oldnodes: Node<_> array) (newnodes: Node<_> array) =
    List.empty
    |> List.append (added oldnodes newnodes)
    |> List.append (removed oldnodes newnodes)
    |> Array.ofList