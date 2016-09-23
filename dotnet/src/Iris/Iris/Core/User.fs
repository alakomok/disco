namespace Iris.Core

#if JAVASCRIPT

open Fable.Core
open Fable.Import
open Iris.Core.FlatBuffers
open Iris.Web.Core.FlatBufferTypes

#else

open System
open LibGit2Sharp
open FlatBuffers
open Iris.Serialization.Raft

#endif

[<CustomEquality>]
[<CustomComparison>]
type User =
  { Id:        Id
  ; UserName:  Name
  ; FirstName: Name
  ; LastName:  Name
  ; Email:     Email
#if JAVASCRIPT
  ; Joined:    string
  ; Created:   string }
#else
  ; Joined:    DateTime
  ; Created:   DateTime }
#endif


  override me.GetHashCode() =
    let mutable hash = 42
#if JAVASCRIPT
    hash <- (hash * 7) + hashCode (string me.Id)
    hash <- (hash * 7) + hashCode me.UserName
    hash <- (hash * 7) + hashCode me.FirstName
    hash <- (hash * 7) + hashCode me.LastName
    hash <- (hash * 7) + hashCode me.Email
    hash <- (hash * 7) + hashCode (string me.Joined)
    hash <- (hash * 7) + hashCode (string me.Created)
#else
    hash <- (hash * 7) + me.Id.GetHashCode()
    hash <- (hash * 7) + me.UserName.GetHashCode()
    hash <- (hash * 7) + me.FirstName.GetHashCode()
    hash <- (hash * 7) + me.LastName.GetHashCode()
    hash <- (hash * 7) + me.Email.GetHashCode()
    hash <- (hash * 7) + (string me.Joined).GetHashCode()
    hash <- (hash * 7) + (string me.Created).GetHashCode()
#endif
    hash

  override me.Equals(o) =
    match o with
    | :? User ->
      let other = o :?> User
      me.Id               = other.Id              &&
      me.UserName         = other.UserName        &&
      me.FirstName        = other.FirstName       &&
      me.LastName         = other.LastName        &&
      me.Email            = other.Email           &&
      (string me.Joined)  = (string other.Joined) &&
      (string me.Created) = (string other.Created)
    | _ -> false

  interface System.IComparable with
    member me.CompareTo(o: obj) =
      match o with
      | :? User ->
        let other = o :?> User

#if JAVASCRIPT
        if me.UserName > other.UserName then
          1
        elif me.UserName = other.UserName then
          0
        else
          -1
#else
        let arr = [| me.UserName; other.UserName |] |> Array.sort
        if Array.findIndex ((=) me.UserName) arr = 0 then
          -1
        else
          1
#endif

      | _ -> 0


#if JAVASCRIPT
#else

  member user.Signature
    with get () =
      let name = sprintf "%s %s" user.FirstName user.LastName
      new Signature(name, user.Email, new DateTimeOffset(user.Created))

#endif

  //  ____  _
  // | __ )(_)_ __   __ _ _ __ _   _
  // |  _ \| | '_ \ / _` | '__| | | |
  // | |_) | | | | | (_| | |  | |_| |
  // |____/|_|_| |_|\__,_|_|   \__, |
  //                           |___/

  member self.ToOffset(builder: FlatBufferBuilder) =
    let id        = self.Id        |> string |> builder.CreateString
    let username  = self.UserName  |> builder.CreateString
    let firstname = self.FirstName |> builder.CreateString
    let lastname  = self.LastName  |> builder.CreateString
    let email     = self.Email     |> builder.CreateString
    let joined    = self.Joined    |> string |> builder.CreateString
    let created   = self.Created   |> string |> builder.CreateString
    UserFB.StartUserFB(builder)
    UserFB.AddId(builder, id)
    UserFB.AddUserName(builder, username)
    UserFB.AddFirstName(builder, firstname)
    UserFB.AddLastName(builder, lastname)
    UserFB.AddEmail(builder, email)
    UserFB.AddJoined(builder, joined)
    UserFB.AddCreated(builder, created)
    UserFB.EndUserFB(builder)

  member self.ToBytes() = Binary.buildBuffer self

  static member FromFB(fb: UserFB) : User option =
    try
      { Id        = Id fb.Id
      ; UserName  = fb.UserName
      ; FirstName = fb.FirstName
      ; LastName  = fb.LastName
      ; Email     = fb.Email
#if JAVASCRIPT
      ; Joined    = fb.Joined
      ; Created   = fb.Created }
#else
      ; Joined    = DateTime.Parse fb.Joined
      ; Created   = DateTime.Parse fb.Created }
#endif
      |> Some
    with
      | exn ->
        printfn "Could not de-serializae binary rep of User: %s" exn.Message
        None

  static member FromBytes (bytes: Binary.Buffer) : User option =
    UserFB.GetRootAsUserFB(Binary.createBuffer bytes)
    |> User.FromFB
