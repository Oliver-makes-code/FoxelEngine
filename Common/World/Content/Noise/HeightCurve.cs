using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Noise;

public sealed record HeightCurve(
    ContentReference<NoiseMap> Map,
    HeightCurve.Value[] BaseHeight,
    HeightCurve.Value[] Magnitude
) {
    public static readonly Codec<HeightCurve> Codec = RecordCodec<HeightCurve>.Create(
        ResourceKey.Codec.Field<HeightCurve>("map", it => it.Map.Key),
        Value.Codec.Array().Field<HeightCurve>("base_height", it => it.BaseHeight),
        Value.Codec.Array().Field<HeightCurve>("magnitude", it => it.Magnitude),
        (map, baseHeight, magnitude) => new(new(ContentStores.NoiseMaps, map), baseHeight, magnitude)
    );

    public readonly record struct Value(
        float Threshold,
        float Height
    ) {
        public static readonly Codec<Value> Codec = RecordCodec<Value>.Create(
            Codecs.Float.Field<Value>("threshold", it => it.Threshold),
            Codecs.Float.Field<Value>("height", it => it.Height),
            (threshold, height) => new(threshold, height)
        );
    }
}
