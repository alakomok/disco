﻿namespace Iris.Tests

open System.IO
open Fuchu
open Iris.Core.Types

module Project =
  let loadSaveTest =
    testCase "Save/Load Project should render equal project values" <|
      fun _ ->
        let name = "test"
        let path = "./tmp"

        Directory.CreateDirectory path |> ignore

        let project = createProject name
        project.Path <- Some(path)
        saveProject project

        let project' =
          loadProject (path + "/test.iris")
          |> Option.get

          // [<DefaultValue>] val mutable  Name      : string
          // [<DefaultValue>] val mutable  Path      : FilePath option
          // [<DefaultValue>] val mutable  LastSaved : DateTime option
          // [<DefaultValue>] val mutable  Copyright : string   option
          // [<DefaultValue>] val mutable  Author    : string   option
          // [<DefaultValue>] val mutable  Year      : int
          // [<DefaultValue>] val mutable  Audio     : AudioConfig
          // [<DefaultValue>] val mutable  Vvvv      : VvvvConfig
          // [<DefaultValue>] val mutable  Engine    : VsyncConfig
          // [<DefaultValue>] val mutable  Timing    : TimingConfig
          // [<DefaultValue>] val mutable  Port      : PortConfig
          // [<DefaultValue>] val mutable  ViewPorts : ViewPort list
          // [<DefaultValue>] val mutable  Displays  : Display list
          // [<DefaultValue>] val mutable  Tasks     : Task list
          // [<DefaultValue>] val mutable  Cluster   : Cluster

        Assert.Equal("Projects Name settings should be equal", true,
                     (project.Name = project'.Name))
        Assert.Equal("Projects Path settings should be equal", true,
                     (project.Path = project'.Path))
        Assert.Equal("Projects Audio settings should be equal", true,
                     (project.Audio = project'.Audio))
        Assert.Equal("Projects Vvvv settings should be equal", true,
                     (project.Vvvv = project'.Vvvv))
        Assert.Equal("Projects Engine settings should be equal", true,
                     (project.Engine = project'.Engine))
        Assert.Equal("Projects Timing settings should be equal", true,
                     (project.Timing = project'.Timing))
        Assert.Equal("Projects Port settings should be equal", true,
                     (project.Port = project'.Port))
        Assert.Equal("Projects ViewPorts settings should be equal", true,
                     (project.ViewPorts = project'.ViewPorts))
        Assert.Equal("Projects Cluster settings should be equal", true,
                     (project.Cluster = project'.Cluster))
  [<Tests>]
  let configTests =
    testList "Config tests" [
        loadSaveTest
      ]