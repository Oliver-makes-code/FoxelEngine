using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Noise;

public sealed record NoiseMap(
    NoiseMap.Octave[] Octaves
) {
    public static readonly Codec<NoiseMap> Codec = RecordCodec<NoiseMap>.Create(
        Octave.Codec.Array().Field<NoiseMap>("octaves", it => it.Octaves),
        (octaves) => new(octaves)
    );

    public readonly record struct Octave(
        float Multiplier,
        float Magnitude
    ) {
        public static readonly Codec<Octave> Codec = RecordCodec<Octave>.Create(
            Codecs.Float.Field<Octave>("multiplier", it => it.Multiplier),
            Codecs.Float.Field<Octave>("magnitude", it => it.Magnitude),
            (multiplier, magnitude) => new(multiplier, magnitude)
        );
    }
}
