using GlmSharp;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Util;

public static class FoxelCodecs {
    public static readonly Codec<ivec2> IVec2 = new FoxelPrimitiveImplCodec<ivec2>(
        (reader) => new(Codecs.Int.FixedArray(2).ReadGeneric(reader)),
        (writer, value) => Codecs.Int.FixedArray(2).WriteGeneric(writer, value.Values)
    );
    public static readonly Codec<vec2> Vec2 = new FoxelPrimitiveImplCodec<vec2>(
        (reader) => new(Codecs.Float.FixedArray(2).ReadGeneric(reader)),
        (writer, value) => Codecs.Float.FixedArray(2).WriteGeneric(writer, value.Values)
    );
    public static readonly Codec<dvec2> DVec2 = new FoxelPrimitiveImplCodec<dvec2>(
        (reader) => new(Codecs.Double.FixedArray(2).ReadGeneric(reader)),
        (writer, value) => Codecs.Double.FixedArray(2).WriteGeneric(writer, value.Values)
    );
    public static readonly Codec<ivec3> IVec3 = new FoxelPrimitiveImplCodec<ivec3>(
        (reader) => new(Codecs.Int.FixedArray(3).ReadGeneric(reader)),
        (writer, value) => Codecs.Int.FixedArray(3).WriteGeneric(writer, value.Values)
    );
    public static readonly Codec<vec3> Vec3 = new FoxelPrimitiveImplCodec<vec3>(
        (reader) => new(Codecs.Float.FixedArray(3).ReadGeneric(reader)),
        (writer, value) => Codecs.Float.FixedArray(3).WriteGeneric(writer, value.Values)
    );
    public static readonly Codec<dvec3> DVec3 = new FoxelPrimitiveImplCodec<dvec3>(
        (reader) => new(Codecs.Double.FixedArray(3).ReadGeneric(reader)),
        (writer, value) => Codecs.Double.FixedArray(3).WriteGeneric(writer, value.Values)
    );
    public static readonly Codec<ivec4> IVec4 = new FoxelPrimitiveImplCodec<ivec4>(
        (reader) => new(Codecs.Int.FixedArray(4).ReadGeneric(reader)),
        (writer, value) => Codecs.Int.FixedArray(4).WriteGeneric(writer, value.Values)
    );
    public static readonly Codec<vec4> Vec4 = new FoxelPrimitiveImplCodec<vec4>(
        (reader) => new(Codecs.Float.FixedArray(4).ReadGeneric(reader)),
        (writer, value) => Codecs.Float.FixedArray(4).WriteGeneric(writer, value.Values)
    );
    public static readonly Codec<dvec4> DVec4 = new FoxelPrimitiveImplCodec<dvec4>(
        (reader) => new(Codecs.Double.FixedArray(4).ReadGeneric(reader)),
        (writer, value) => Codecs.Double.FixedArray(4).WriteGeneric(writer, value.Values)
    );
    public static readonly Codec<iquat> IQuat = new ProxyCodec<ivec4, iquat>(
        IVec4,
        (vec) => new(vec.x, vec.y, vec.z, vec.w),
        (quat) => (ivec4)quat
    );
    public static readonly Codec<quat> Quat = new ProxyCodec<vec4, quat>(
        Vec4,
        (vec) => new(vec.x, vec.y, vec.z, vec.w),
        (quat) => (vec4)quat
    );
    public static readonly Codec<dquat> DQuat = new ProxyCodec<dvec4, dquat>(
        DVec4,
        (vec) => new(vec.x, vec.y, vec.z, vec.w),
        (quat) => (dvec4)quat
    );
}

public record ProxyCodec<TBase, TValue>(Codec<TBase> BaseCodec, Func<TBase, TValue> IntoValue, Func<TValue, TBase> IntoBase) : Codec<TValue> {
    public override TValue ReadGeneric(DataReader reader)
        => IntoValue(BaseCodec.ReadGeneric(reader));
    public override void WriteGeneric(DataWriter writer, TValue value)
        => BaseCodec.WriteGeneric(writer, IntoBase(value));
}

public record FoxelPrimitiveImplCodec<TValue>(Func<DataReader, TValue> Reader, Action<DataWriter, TValue> Writer) : Codec<TValue> {
    public override TValue ReadGeneric(DataReader reader)
        => Reader(reader);
    public override void WriteGeneric(DataWriter writer, TValue value)
        => Writer(writer, value);
}
