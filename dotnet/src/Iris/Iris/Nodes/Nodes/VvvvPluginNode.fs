namespace VVVV.Nodes

open System
open System.ComponentModel.Composition
open VVVV.PluginInterfaces.V1
open VVVV.PluginInterfaces.V2
open VVVV.Utils.VColor
open VVVV.Utils.VMath
open VVVV.Core.Logging
open Iris.Raft
open Iris.Core
open Iris.Nodes

//  ____  _             _
// |  _ \| |_   _  __ _(_)_ __
// | |_) | | | | |/ _` | | '_ \
// |  __/| | |_| | (_| | | | | |
// |_|   |_|\__,_|\__, |_|_| |_|
//                |___/

[<PluginInfo(Name="VvvvPlugin", Category="Iris", AutoEvaluate=true)>]
type VvvvPluginNode() =

  [<Import();DefaultValue>]
  val mutable Logger: ILogger

  [<DefaultValue>]
  [<Input("VvvvPlugin")>]
  val mutable InPlugin: ISpread<VvvvPlugin>

  [<DefaultValue>]
  [<Input("Update", IsSingle = true, IsBang = true)>]
  val mutable InUpdate: IDiffSpread<bool>

  [<DefaultValue>]
  [<Output("Name")>]
  val mutable OutName: ISpread<string>

  [<DefaultValue>]
  [<Output("Path")>]
  val mutable OutPath: ISpread<string>

  [<DefaultValue>]
  [<Output("Update", IsSingle = true, IsBang = true)>]
  val mutable OutUpdate: ISpread<bool>

  interface IPluginEvaluate with
    member self.Evaluate (spreadMax: int) : unit =
      if self.InUpdate.[0] then

        self.OutName.SliceCount <- self.InPlugin.SliceCount
        self.OutPath.SliceCount <- self.InPlugin.SliceCount

        for n in 0 .. (spreadMax - 1) do
          if not (Util.isNullReference self.InPlugin.[0])  then
            let config = self.InPlugin.[n]
            self.OutName.[n] <- config.Name
            self.OutPath.[n] <- config.Path

      if self.InUpdate.IsChanged then
        self.OutUpdate.[0] <- self.InUpdate.[0]