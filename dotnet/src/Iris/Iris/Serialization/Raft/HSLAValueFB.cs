// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Serialization.Raft
{

using System;
using FlatBuffers;

public sealed class HSLAValueFB : Table {
  public static HSLAValueFB GetRootAsHSLAValueFB(ByteBuffer _bb) { return GetRootAsHSLAValueFB(_bb, new HSLAValueFB()); }
  public static HSLAValueFB GetRootAsHSLAValueFB(ByteBuffer _bb, HSLAValueFB obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public HSLAValueFB __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public byte Hue { get { int o = __offset(4); return o != 0 ? bb.Get(o + bb_pos) : (byte)0; } }
  public byte Saturation { get { int o = __offset(6); return o != 0 ? bb.Get(o + bb_pos) : (byte)0; } }
  public byte Lightness { get { int o = __offset(8); return o != 0 ? bb.Get(o + bb_pos) : (byte)0; } }
  public byte Alpha { get { int o = __offset(10); return o != 0 ? bb.Get(o + bb_pos) : (byte)0; } }

  public static Offset<HSLAValueFB> CreateHSLAValueFB(FlatBufferBuilder builder,
      byte Hue = 0,
      byte Saturation = 0,
      byte Lightness = 0,
      byte Alpha = 0) {
    builder.StartObject(4);
    HSLAValueFB.AddAlpha(builder, Alpha);
    HSLAValueFB.AddLightness(builder, Lightness);
    HSLAValueFB.AddSaturation(builder, Saturation);
    HSLAValueFB.AddHue(builder, Hue);
    return HSLAValueFB.EndHSLAValueFB(builder);
  }

  public static void StartHSLAValueFB(FlatBufferBuilder builder) { builder.StartObject(4); }
  public static void AddHue(FlatBufferBuilder builder, byte Hue) { builder.AddByte(0, Hue, 0); }
  public static void AddSaturation(FlatBufferBuilder builder, byte Saturation) { builder.AddByte(1, Saturation, 0); }
  public static void AddLightness(FlatBufferBuilder builder, byte Lightness) { builder.AddByte(2, Lightness, 0); }
  public static void AddAlpha(FlatBufferBuilder builder, byte Alpha) { builder.AddByte(3, Alpha, 0); }
  public static Offset<HSLAValueFB> EndHSLAValueFB(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<HSLAValueFB>(o);
  }
};


}