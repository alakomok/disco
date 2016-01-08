namespace Iris.Service.Types

open System.Collections.Generic
open Nessos.FsPickler
open Vsync

[<AutoOpen>]
module PinGroup =

  type Id = string

  (* ---------- Pin ---------- *)
  type Pin =
    { Id      : Id
    ; Name    : string
    ; IOBoxes : string array
    }

    static member FromBytes(data : byte[]) : Pin =
      let s = FsPickler.CreateBinarySerializer()
      s.UnPickle<Pin> data
    
    member self.ToBytes() =
      let s = FsPickler.CreateBinarySerializer()
      s.Pickle self

  type PinDict = Dictionary<Id,Pin>

  (* ---------- Host ---------- *)

  type Host =
    { Id   : string
    ; Pins : Dictionary<Id,Pin>
    }
    static member Create(id : Id) =
      { Id = id; Pins = new Dictionary<Id, Pin>() }

  (* ---------- PinAction ---------- *)

  type PinAction =
    | Add    = 1
    | Update = 2
    | Delete = 3

  (* ---------- PinGroup ---------- *)

  type PinGroup(grpname) as self = 
    [<DefaultValue>] val mutable group : IrisGroup

    let mutable pins : PinDict = new Dictionary<Id,Pin>()

    let toI (pa : PinAction) : int = int pa

    let bToP (f : Pin -> unit) (bytes : byte[]) =
      f <| Pin.FromBytes(bytes)
      
    let pToB (p : Pin) : byte[] =
      p.ToBytes()

    let AllHandlers =
      [ (PinAction.Add,    self.PinAdded)
      ; (PinAction.Update, self.PinUpdated)
      ; (PinAction.Delete, self.PinDeleted)
      ]

    do
      self.group <- new IrisGroup(grpname)
      self.group.AddViewHandler(self.ViewChanged)
      self.group.AddCheckpointMaker(self.MakeCheckpoint)
      self.group.AddCheckpointLoader(self.LoadCheckpoint)
      List.iter (fun (a,cb) -> self.group.AddHandler(toI a, mkHandler(bToP cb))) AllHandlers

    member self.Join() = self.group.Join()

    member self.Dump() =
      for pin in pins do
        printfn "pin id: %s" pin.Key

    member self.Send(action : PinAction, p : Pin) =
      self.group.Send(toI action, p.ToBytes())

    member self.Add(p : Pin) =
      pins.Add(p.Id, p)

    member self.MakeCheckpoint(view : View) =
      printfn "makeing a snapshot. %d pins in it" pins.Count
      let s = FsPickler.CreateBinarySerializer()
      self.group.SendChkpt(s.Pickle pins)
      self.group.EndOfChkpt()

    member self.LoadCheckpoint(bytes : byte[]) =
      let s = FsPickler.CreateBinarySerializer()
      pins <- s.UnPickle<PinDict> bytes
      printfn "loaded a snapshot. %d pins in it" pins.Count

    member self.ViewChanged(view : View) : unit =
      printfn "viewid: %d" <| view.GetViewid() 

    member self.PinAdded(pin : Pin) : unit =
      if not <| pins.ContainsKey(pin.Id)
      then
        self.Add(pin)
        printfn "pin added cb: "
        self.Dump()
      else
        printfn "pin already present"

    member self.PinUpdated(pin : Pin) : unit =
      printfn "%s updated" pin.Name

    member self.PinDeleted(pin : Pin) : unit =
      printfn "%s removed" pin.Name