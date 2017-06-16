namespace Iris.Service

// * Imports

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open System.Collections.Generic
open Disruptor
open Disruptor.Dsl
open ZeroMQ
open Iris.Core
open Iris.Raft
open Iris.Zmq
open SharpYaml.Serialization

// * Pipeline

module Pipeline =

  // ** bufferSize

  [<Literal>]
  let private BufferSize = 2048

  // ** scheduler

  let private scheduler = TaskScheduler.Default

  // ** tag

  let private tag (str: string) = String.format "Pipeline.{0}" str

  // ** createDisruptor

  let private createDisruptor () =
    Dsl.Disruptor<PipelineEvent<IrisEvent>>(PipelineEvent<IrisEvent>, BufferSize, scheduler)

  // ** handleEventsWith

  let private handleEventsWith (handlers: IHandler<IrisEvent> [])
                               (disruptor: Disruptor<PipelineEvent<IrisEvent>>) =
    disruptor.HandleEventsWith handlers

  // ** thenDo

  let private thenDo (handlers: IHandler<IrisEvent>[]) (group: IHandlerGroup<IrisEvent>) =
    group.Then handlers

  // ** insertInto

  let private insertInto (ringBuffer: RingBuffer<PipelineEvent<IrisEvent>>) (cmd: IrisEvent) =
    let seqno = ringBuffer.Next()
    let entry = ringBuffer.[seqno]
    entry.Event <- Some cmd
    ringBuffer.Publish(seqno)

  // ** clearEvent

  let private clearEvent =
    [|  { new IHandler<IrisEvent> with
           member handler.OnEvent(ev: PipelineEvent<IrisEvent>, _, _) =
             ev.Clear() } |]

  // ** createHandler

  let createHandler (f: EventProcessor<IrisEvent>) : IHandler<IrisEvent> =
    { new IHandler<IrisEvent> with
        member handler.OnEvent(ev: PipelineEvent<IrisEvent>, seqno, eob) =
          Option.iter (f seqno eob) ev.Event }

  // ** create

  let create processors publishers postactions =
    let disruptor = createDisruptor()

    disruptor
    |> handleEventsWith processors
    |> thenDo publishers
    |> thenDo postactions
    |> thenDo clearEvent
    |> ignore

    let ringBuffer = disruptor.Start()

    { new IPipeline<IrisEvent> with
        member pipeline.Push(cmd: IrisEvent) =
          insertInto ringBuffer cmd

        member pipeline.Dispose() =
          disruptor.Shutdown() }
