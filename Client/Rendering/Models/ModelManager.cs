using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Foxel.Common.Util;
using Foxel.Core.Assets;
using Foxel.Core.Util;
using GlmSharp;
using Greenhouse.Libs.Serialization;
using Greenhouse.Libs.Serialization.Reader;
using Greenhouse.Libs.Serialization.Structure;
using Newtonsoft.Json;

namespace Foxel.Client.Rendering.Models;

public static class ModelManager {
    public static readonly PackManager.ReloadTask ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);

    private const string Suffix = ".json";
    private const string Prefix = "models/new/";

    private static readonly Dictionary<ResourceKey, ModelRoot> RawModels = [];
    private static readonly Dictionary<ResourceKey, ModelRoot> ResolvedModels = [];

    public static ModelRoot ResolveModel(ResourceKey key)
        => ResolveModel(key, []);

    private static void Reload(PackManager manager) {
        RawModels.Clear();
        ResolvedModels.Clear();
        foreach (var resource in manager.ListResources(AssetType.Assets, Prefix, Suffix)) {
            int start = Prefix.Length;
            int end = resource.Value.Length - Suffix.Length;
            string name = resource.Value[start..end];
            var key = new ResourceKey(resource.Group, name);

            using var stream = manager.OpenStream(AssetType.Assets, resource).Last();
            using var sr = new StreamReader(stream);
            using var jr = new JsonTextReader(sr);
            var reader = new JsonDataReader(jr);
            
            var model = ModelRoot.Codec.ReadGeneric(reader);

            RawModels[key] = model;
        }
    }

    internal static ModelRoot ResolveModel(ResourceKey key, ResourceKey[] recursionCheck) {
        if (ResolvedModels.TryGetValue(key, out var cached))
            return cached;

        var model = RawModels[key];

        Dictionary<string, ResourceKey> textures = new(model.Textures);
        
        var root = model.Model.Resolve([..recursionCheck, key], textures);

        var resolved = new ModelRoot(textures, root);
        ResolvedModels[key] = resolved;

        return resolved;
    }
}

public record ModelRoot(
    Dictionary<string, ResourceKey> Textures,
    ModelPart Model
) {
    public static readonly Codec<Dictionary<string, ResourceKey>> TexturesCodec = new FoxelPrimitiveImplCodec<Dictionary<string, ResourceKey>>(
        reader => {
            var json = (StructuredObjectDataReader)reader;
            var obj = (StructuredValue.Object)json.Value;
            Dictionary<string, ResourceKey> values = [];
            foreach (var key in obj.Values.Keys) {
                var value = obj.Values[key];
                values[key] = ResourceKey.Codec.ReadGeneric(new StructuredObjectDataReader(value));
            }
            return values;
        },
        (writer, value) => {}
    );
    public static readonly Codec<ModelRoot> Codec = RecordCodec<ModelRoot>.Create(
        TexturesCodec.Field<ModelRoot>("textures", it => it.Textures),
        ModelPart.Codec.Field<ModelRoot>("model", it => it.Model),
        (tex, model) => new(tex, model)
    );
}

public abstract record ModelPart(
    string Name,
    vec3 Origin,
    vec3 Size,
    vec3 Pivot,
    quat Rotation
) {
    public static readonly Codec<Variant<string, ModelPart>> VariantCodec = new RecordVariantCodec<string, ModelPart>(
        Codecs.String,
        (it) => it switch {
            "list" => ListModelPart.Codec,
            "reference" => ReferenceModelPart.Codec,
            "cube" => CubeModelPart.Codec,
            _ => throw new ArgumentException($"Unknown model part type: {it}")
        }
    );
    public static readonly Codec<ModelPart> Codec = new FoxelPrimitiveImplCodec<ModelPart>(
        reader => VariantCodec.ReadGeneric(reader).value,
        (writer, value) => VariantCodec.WriteGeneric(writer, value switch {
            ListModelPart => new("list", value),
            ReferenceModelPart => new("reference", value),
            CubeModelPart => new("cube", value),
            _ => throw new ArgumentException($"Unkown type! Got {value.GetType()}")
        })
    );

    public abstract ModelPart Resolve(ResourceKey[] recursionCheck, Dictionary<string, ResourceKey> textures);
}

public record ListModelPart(
    ModelPart[] Parts,
    string name,
    vec3 origin,
    vec3 size,
    vec3 pivot,
    quat rotation
) : ModelPart(name, origin, size, pivot, rotation) {
    public new static readonly RecordCodec<ModelPart> Codec = RecordCodec<ModelPart>.Create(
        ModelPart.Codec.Array().Field<ModelPart>("parts", it => ((ListModelPart)it).Parts),
        Codecs.String.Field<ModelPart>("name", it => it.Name),
        FoxelCodecs.Vec3.Field<ModelPart>("origin", it => it.Origin),
        FoxelCodecs.Vec3.Field<ModelPart>("size", it => it.Size),
        FoxelCodecs.Vec3.Field<ModelPart>("pivot", it => it.Pivot),
        FoxelCodecs.Quat.Field<ModelPart>("rotation", it => it.Rotation),
        (parts, name, origin, size, pivot, rotation) => new ListModelPart(parts, name, origin, size, pivot, rotation)
    );

    public override ModelPart Resolve(ResourceKey[] recursionCheck, Dictionary<string, ResourceKey> textures) {
        ModelPart[] parts = new ModelPart[Parts.Length];
        HashSet<string> names = [];
        for (int i = 0; i < Parts.Length; i++) {
            parts[i] = Parts[i].Resolve(recursionCheck, textures);
            if (names.Contains(parts[i].Name))
                throw new Exception($"Model {recursionCheck[0]} contains duplicate name definition: {parts[i].Name}");
        }
        
        return new ListModelPart(parts, Name, Origin, Size, Pivot, Rotation);
    }

    protected override bool PrintMembers(StringBuilder builder) {
        builder.Append("[ ");
        for (int i = 0; i < Parts.Length; i++) {
            builder.Append(Parts[i]);
            if (i < Parts.Length - 1)
                builder.Append(',');
            builder.Append(' ');
        }
        builder.Append("], ");
        return base.PrintMembers(builder);
    }
}

public record ReferenceModelPart(
    ResourceKey Model,
    string name,
    vec3 origin,
    vec3 size,
    vec3 pivot,
    quat rotation
) : ModelPart(name, origin, size, pivot, rotation) {
    public new static readonly RecordCodec<ModelPart> Codec = RecordCodec<ModelPart>.Create(
        ResourceKey.Codec.Field<ModelPart>("model", it => ((ReferenceModelPart)it).Model),
        Codecs.String.Field<ModelPart>("name", it => it.Name),
        FoxelCodecs.Vec3.Field<ModelPart>("origin", it => it.Origin),
        FoxelCodecs.Vec3.Field<ModelPart>("size", it => it.Size),
        FoxelCodecs.Vec3.Field<ModelPart>("pivot", it => it.Pivot),
        FoxelCodecs.Quat.Field<ModelPart>("rotation", it => it.Rotation),
        (model, name, origin, size, pivot, rotation) => new ReferenceModelPart(model, name, origin, size, pivot, rotation)
    );

    public override ModelPart Resolve(ResourceKey[] recursionCheck, Dictionary<string, ResourceKey> textures) {
        if (recursionCheck.Contains(Model))
            throw new Exception($"Model {recursionCheck[0]} contains recursive definition for {Model}");
        
        var resolved = ModelManager.ResolveModel(Model, recursionCheck);
        foreach (var key in resolved.Textures.Keys)
            if (!textures.ContainsKey(key))
                textures.Add(key, resolved.Textures[key]);
            
        return new ListModelPart([resolved.Model], Name, Origin, Size, Pivot, Rotation);
    }
}

public record CubeModelPart(
    CubeSides Sides,
    string name,
    vec3 origin,
    vec3 size,
    vec3 pivot,
    quat rotation
) : ModelPart(name, origin, size, pivot, rotation) {
    public new static readonly RecordCodec<ModelPart> Codec = RecordCodec<ModelPart>.Create(
        CubeSides.Codec.Field<ModelPart>("sides", it => ((CubeModelPart)it).Sides),
        Codecs.String.Field<ModelPart>("name", it => it.Name),
        FoxelCodecs.Vec3.Field<ModelPart>("origin", it => it.Origin),
        FoxelCodecs.Vec3.Field<ModelPart>("size", it => it.Size),
        FoxelCodecs.Vec3.Field<ModelPart>("pivot", it => it.Pivot),
        FoxelCodecs.Quat.Field<ModelPart>("rotation", it => it.Rotation),
        (sides, name, origin, size, pivot, rotation) => new CubeModelPart(sides, name, origin, size, pivot, rotation)
    );

    public override ModelPart Resolve(ResourceKey[] recursionCheck, Dictionary<string, ResourceKey> textures)
        => this;

    protected override bool PrintMembers(StringBuilder builder) {
        builder.Append(Sides);
        builder.Append(", ");
        return base.PrintMembers(builder);
    }
}

public record CubeSides(
    CubeSide North,
    CubeSide South,
    CubeSide East,
    CubeSide West,
    CubeSide Up,
    CubeSide Down
) {
    public static readonly Codec<CubeSides> Codec = RecordCodec<CubeSides>.Create(
        CubeSide.Codec.Field<CubeSides>("north", it => it.North),
        CubeSide.Codec.Field<CubeSides>("south", it => it.South),
        CubeSide.Codec.Field<CubeSides>("east", it => it.East),
        CubeSide.Codec.Field<CubeSides>("west", it => it.West),
        CubeSide.Codec.Field<CubeSides>("up", it => it.Up),
        CubeSide.Codec.Field<CubeSides>("down", it => it.Down),
        (north, south, east, west, up, down) => new(north, south, east, west, up, down)
    );
}

public record CubeSide(
    vec4 Uv,
    string Texture,
    CullingSide CullingSide
) {
    public static readonly Codec<CubeSide> Codec = RecordCodec<CubeSide>.Create(
        FoxelCodecs.Vec4.Field<CubeSide>("uv", it => it.Uv),
        Codecs.String.Field<CubeSide>("texture", it => it.Texture),
        CullingSideExtensions.Codec.Field<CubeSide>("culling_side", it => it.CullingSide),
        (uv, tex, side) => new(uv, tex, side)
    );
}

public enum CullingSide : byte {
    West = 0,
    East = 1,
    Down = 2,
    Up = 3,
    North = 4,
    South = 5,
    None = 6,
}

public static class CullingSideExtensions {
    public static readonly Codec<CullingSide> Codec = new StringEnumCodec<CullingSide>();
}
