// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Types
{

using System;
using FlatBuffers;

public sealed class Snapshot : Table {
  public static Snapshot GetRootAsSnapshot(ByteBuffer _bb) { return GetRootAsSnapshot(_bb, new Snapshot()); }
  public static Snapshot GetRootAsSnapshot(ByteBuffer _bb, Snapshot obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Snapshot __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public string Checksum { get { int o = __offset(4); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetChecksumBytes() { return __vector_as_arraysegment(4); }
  public string TimeStamp { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetTimeStampBytes() { return __vector_as_arraysegment(6); }

  public static Offset<Snapshot> CreateSnapshot(FlatBufferBuilder builder,
      StringOffset ChecksumOffset = default(StringOffset),
      StringOffset TimeStampOffset = default(StringOffset)) {
    builder.StartObject(2);
    Snapshot.AddTimeStamp(builder, TimeStampOffset);
    Snapshot.AddChecksum(builder, ChecksumOffset);
    return Snapshot.EndSnapshot(builder);
  }

  public static void StartSnapshot(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddChecksum(FlatBufferBuilder builder, StringOffset ChecksumOffset) { builder.AddOffset(0, ChecksumOffset.Value, 0); }
  public static void AddTimeStamp(FlatBufferBuilder builder, StringOffset TimeStampOffset) { builder.AddOffset(1, TimeStampOffset.Value, 0); }
  public static Offset<Snapshot> EndSnapshot(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Snapshot>(o);
  }
  public static void FinishSnapshotBuffer(FlatBufferBuilder builder, Offset<Snapshot> offset) { builder.Finish(offset.Value); }
};


}