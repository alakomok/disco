// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Serialization.Raft
{

using System;
using FlatBuffers;

public sealed class AddCueListFB : Table {
  public static AddCueListFB GetRootAsAddCueListFB(ByteBuffer _bb) { return GetRootAsAddCueListFB(_bb, new AddCueListFB()); }
  public static AddCueListFB GetRootAsAddCueListFB(ByteBuffer _bb, AddCueListFB obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public AddCueListFB __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public CueListFB CueList { get { return GetCueList(new CueListFB()); } }
  public CueListFB GetCueList(CueListFB obj) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }

  public static Offset<AddCueListFB> CreateAddCueListFB(FlatBufferBuilder builder,
      Offset<CueListFB> CueListOffset = default(Offset<CueListFB>)) {
    builder.StartObject(1);
    AddCueListFB.AddCueList(builder, CueListOffset);
    return AddCueListFB.EndAddCueListFB(builder);
  }

  public static void StartAddCueListFB(FlatBufferBuilder builder) { builder.StartObject(1); }
  public static void AddCueList(FlatBufferBuilder builder, Offset<CueListFB> CueListOffset) { builder.AddOffset(0, CueListOffset.Value, 0); }
  public static Offset<AddCueListFB> EndAddCueListFB(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<AddCueListFB>(o);
  }
};


}