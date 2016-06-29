// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Serialization.Raft
{

using System;
using FlatBuffers;

public sealed class InstallSnapshotFB : Table {
  public static InstallSnapshotFB GetRootAsInstallSnapshotFB(ByteBuffer _bb) { return GetRootAsInstallSnapshotFB(_bb, new InstallSnapshotFB()); }
  public static InstallSnapshotFB GetRootAsInstallSnapshotFB(ByteBuffer _bb, InstallSnapshotFB obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public InstallSnapshotFB __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public ulong Term { get { int o = __offset(4); return o != 0 ? bb.GetUlong(o + bb_pos) : (ulong)0; } }
  public ulong LeaderId { get { int o = __offset(6); return o != 0 ? bb.GetUlong(o + bb_pos) : (ulong)0; } }
  public ulong LastIndex { get { int o = __offset(8); return o != 0 ? bb.GetUlong(o + bb_pos) : (ulong)0; } }
  public ulong LastTerm { get { int o = __offset(10); return o != 0 ? bb.GetUlong(o + bb_pos) : (ulong)0; } }
  public LogFB GetData(int j) { return GetData(new LogFB(), j); }
  public LogFB GetData(LogFB obj, int j) { int o = __offset(12); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int DataLength { get { int o = __offset(12); return o != 0 ? __vector_len(o) : 0; } }

  public static Offset<InstallSnapshotFB> CreateInstallSnapshotFB(FlatBufferBuilder builder,
      ulong Term = 0,
      ulong LeaderId = 0,
      ulong LastIndex = 0,
      ulong LastTerm = 0,
      VectorOffset DataOffset = default(VectorOffset)) {
    builder.StartObject(5);
    InstallSnapshotFB.AddLastTerm(builder, LastTerm);
    InstallSnapshotFB.AddLastIndex(builder, LastIndex);
    InstallSnapshotFB.AddLeaderId(builder, LeaderId);
    InstallSnapshotFB.AddTerm(builder, Term);
    InstallSnapshotFB.AddData(builder, DataOffset);
    return InstallSnapshotFB.EndInstallSnapshotFB(builder);
  }

  public static void StartInstallSnapshotFB(FlatBufferBuilder builder) { builder.StartObject(5); }
  public static void AddTerm(FlatBufferBuilder builder, ulong Term) { builder.AddUlong(0, Term, 0); }
  public static void AddLeaderId(FlatBufferBuilder builder, ulong LeaderId) { builder.AddUlong(1, LeaderId, 0); }
  public static void AddLastIndex(FlatBufferBuilder builder, ulong LastIndex) { builder.AddUlong(2, LastIndex, 0); }
  public static void AddLastTerm(FlatBufferBuilder builder, ulong LastTerm) { builder.AddUlong(3, LastTerm, 0); }
  public static void AddData(FlatBufferBuilder builder, VectorOffset DataOffset) { builder.AddOffset(4, DataOffset.Value, 0); }
  public static VectorOffset CreateDataVector(FlatBufferBuilder builder, Offset<LogFB>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartDataVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<InstallSnapshotFB> EndInstallSnapshotFB(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<InstallSnapshotFB>(o);
  }
};


}
