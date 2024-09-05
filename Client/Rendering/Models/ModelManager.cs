using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Foxel.Client.Rendering.Texture;
using Foxel.Common.Util;
using Foxel.Core.Assets;
using Foxel.Core.Util;
using GlmSharp;
using Greenhouse.Libs.Serialization;
using Greenhouse.Libs.Serialization.Reader;
using Greenhouse.Libs.Serialization.Structure;

namespace Foxel.Client.Rendering.Models;

public static class ModelManager {
    public static readonly PackManager.ReloadTask ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);

    private const string Prefix = "models/";

    private static readonly Dictionary<ResourceKey, ModelRoot> RawModels = [];
    private static readonly Dictionary<ResourceKey, ModelRoot> ResolvedModels = [];
    private static readonly TransformStack Stack = new();

    private static readonly List<ResourceKey> ModelsToResolve = [];

    public static void EmitVertices(ModelRoot model, Atlas atlas, BlockModel.Builder builder) {
        Stack.Clear();
        model.Model.EmitVertices(atlas, builder, model.Textures, Stack);
    }

    public static bool TryGetModel(ResourceKey modelKey, [NotNullWhen(true)] out ModelRoot? model) {
        model = ResolveModel(modelKey, []);
        return model != null;
    }

    private static void Reload(PackManager manager) {
        RawModels.Clear();
        ResolvedModels.Clear();
        ModelsToResolve.Clear();

        // Load models first
        foreach (var (key, model) in manager.OpenJsons(AssetType.Assets, ModelRoot.Codec, Prefix)) {
            ModelsToResolve.Add(key);
            RawModels[key] = model;
        }
    }

    internal static ModelRoot? ResolveModel(ResourceKey key, ResourceKey[] recursionCheck) {
        if (ResolvedModels.TryGetValue(key, out var cached))
            return cached;
        
        if (!RawModels.ContainsKey(key))
            return null;

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
        TexturesCodec.DefaultedField<ModelRoot>("textures", it => it.Textures, () => []),
        ModelPart.Codec.Field<ModelRoot>("model", it => it.Model),
        (tex, model) => new(tex, model)
    );
}

public abstract record ModelPart(
    string Name,
    vec3 Position,
    vec3 Size,
    vec3 Pivot,
    quat Rotation
) {
    public static readonly FieldCodec<string, ModelPart> NameCodec = Codecs.String.DefaultedField<ModelPart>("name", it => it.Name, () => "part");
    public static readonly FieldCodec<vec3, ModelPart> PositionCodec = FoxelCodecs.Vec3.DefaultedField<ModelPart>("position", it => it.Position, () => vec3.Zero);
    public static readonly FieldCodec<vec3, ModelPart> SizeCodec = FoxelCodecs.Vec3.DefaultedField<ModelPart>("size", it => it.Size, () => vec3.Ones);
    public static readonly FieldCodec<vec3, ModelPart> PivotCodec = FoxelCodecs.Vec3.DefaultedField<ModelPart>("pivot", it => it.Pivot, () => new(0.5f, 0.5f, 0.5f));
    public static readonly FieldCodec<quat, ModelPart> RotationCodec = FoxelCodecs.Quat.DefaultedField<ModelPart>("rotation", it => it.Rotation, () => quat.Identity);

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

    public abstract void EmitVertices(Atlas atlas, BlockModel.Builder builder, Dictionary<string, ResourceKey> textures, TransformStack stack);

    public TransformFrame GetTransform()
        => new() {
            position = Position,
            size = Size,
            pivot = Pivot,
            rotation = Rotation,
        };
}

public record ListModelPart(
    ModelPart[] Parts,
    string name,
    vec3 position,
    vec3 size,
    vec3 pivot,
    quat rotation
) : ModelPart(name, position, size, pivot, rotation) {
    public new static readonly RecordCodec<ModelPart> Codec = RecordCodec<ModelPart>.Create(
        ModelPart.Codec.Array().Field<ModelPart>("parts", it => ((ListModelPart)it).Parts),
        NameCodec,
        PositionCodec,
        SizeCodec,
        PivotCodec,
        RotationCodec,
        (parts, name, position, size, pivot, rotation) => new ListModelPart(parts, name, position, size, pivot, rotation)
    );

    public override void EmitVertices(Atlas atlas, BlockModel.Builder builder, Dictionary<string, ResourceKey> textures, TransformStack stack) {
        stack.PushTransform(GetTransform());
        for (int i = 0; i < Parts.Length; i++)
            Parts[i].EmitVertices(atlas, builder, textures, stack);
        stack.PopTransform();
    }

    public override ModelPart Resolve(ResourceKey[] recursionCheck, Dictionary<string, ResourceKey> textures) {
        ModelPart[] parts = new ModelPart[Parts.Length];
        for (int i = 0; i < Parts.Length; i++)
            parts[i] = Parts[i].Resolve(recursionCheck, textures);
        
        return new ListModelPart(parts, Name, Position, Size, Pivot, Rotation);
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
    vec3 position,
    vec3 size,
    vec3 pivot,
    quat rotation
) : ModelPart(name, position, size, pivot, rotation) {
    public new static readonly RecordCodec<ModelPart> Codec = RecordCodec<ModelPart>.Create(
        ResourceKey.Codec.Field<ModelPart>("model", it => ((ReferenceModelPart)it).Model),
        NameCodec,
        PositionCodec,
        SizeCodec,
        PivotCodec,
        RotationCodec,
        (model, name, position, size, pivot, rotation) => new ReferenceModelPart(model, name, position, size, pivot, rotation)
    );

    public override void EmitVertices(Atlas atlas, BlockModel.Builder builder, Dictionary<string, ResourceKey> textures, TransformStack stack) {}

    public override ModelPart Resolve(ResourceKey[] recursionCheck, Dictionary<string, ResourceKey> textures) {
        if (recursionCheck.Contains(Model))
            throw new Exception($"Model {recursionCheck[0]} contains recursive definition for {Model}");
        
        var resolved = ModelManager.ResolveModel(Model, recursionCheck);
        foreach (var key in resolved!.Textures.Keys)
            if (!textures.ContainsKey(key))
                textures.Add(key, resolved.Textures[key]);
            
        return new ListModelPart([resolved.Model], Name, Position, Size, Pivot, Rotation);
    }
}

public record CubeModelPart(
    CubeSides Sides,
    string name,
    vec3 position,
    vec3 size,
    vec3 pivot,
    quat rotation
) : ModelPart(name, position, size, pivot, rotation) {
    public new static readonly RecordCodec<ModelPart> Codec = RecordCodec<ModelPart>.Create(
        CubeSides.Codec.Field<ModelPart>("sides", it => ((CubeModelPart)it).Sides),
        NameCodec,
        PositionCodec,
        SizeCodec,
        PivotCodec,
        RotationCodec,
        (sides, name, position, size, pivot, rotation) => new CubeModelPart(sides, name, position, size, pivot, rotation)
    );

    public override void EmitVertices(Atlas atlas, BlockModel.Builder builder, Dictionary<string, ResourceKey> textures, TransformStack stack) {
        stack.PushTransform(GetTransform());
        {
            stack.PushTransform(new() {
                position = new(0, 0, 0),
                size = new(1, 1, 1),
                pivot = new(0.5f, 0.5f, 0.5f),
                rotation = quat.Identity
            });

            Sides.Up.EmitVertices(atlas, builder, textures, stack);

            stack.PopTransform();
        }
        {
            stack.PushTransform(new() {
                position = new(0, 0, 0),
                size = new(1, 1, 1),
                pivot = new(0.5f, 0.5f, 0.5f),
                rotation = quat.Identity
                    .Rotated(float.Pi/2, new(0, 0, 1))
            });

            Sides.East.EmitVertices(atlas, builder, textures, stack);

            stack.PopTransform();
        }
        {
            stack.PushTransform(new() {
                position = new(0, 0, 0),
                size = new(1, 1, 1),
                pivot = new(0.5f, 0.5f, 0.5f),
                rotation = quat.Identity
                    .Rotated(float.Pi/2, new(0, 0, 1))
                    .Rotated(float.Pi/2, new(0, 1, 0))
            });

            Sides.South.EmitVertices(atlas, builder, textures, stack);

            stack.PopTransform();
        }
        {
            stack.PushTransform(new() {
                position = new(0, 0, 0),
                size = new(1, 1, 1),
                pivot = new(0.5f, 0.5f, 0.5f),
                rotation = quat.Identity
                    .Rotated(float.Pi/2, new(0, 0, 1))
                    .Rotated(float.Pi, new(0, 1, 0))
            });

            Sides.West.EmitVertices(atlas, builder, textures, stack);

            stack.PopTransform();
        }
        {
            stack.PushTransform(new() {
                position = new(0, 0, 0),
                size = new(1, 1, 1),
                pivot = new(0.5f, 0.5f, 0.5f),
                rotation = quat.Identity
                    .Rotated(float.Pi/2, new(0, 0, 1))
                    .Rotated(-float.Pi/2, new(0, 1, 0))
            });

            Sides.North.EmitVertices(atlas, builder, textures, stack);

            stack.PopTransform();
        }
        {
            stack.PushTransform(new() {
                position = new(0, 0, 0),
                size = new(1, 1, 1),
                pivot = new(0.5f, 0.5f, 0.5f),
                rotation = quat.Identity
                    .Rotated(float.Pi, new(1, 0, 0))
            });

            Sides.Down.EmitVertices(atlas, builder, textures, stack);

            stack.PopTransform();
        }
        stack.PopTransform();
    }
    
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
        FoxelCodecs.Vec4.DefaultedField<CubeSide>("uv", it => it.Uv, () => new(0, 0, 1, 1)),
        Codecs.String.Field<CubeSide>("texture", it => it.Texture),
        CullingSideExtensions.Codec.DefaultedField<CubeSide>("culling_side", it => it.CullingSide, () => CullingSide.None),
        (uv, tex, side) => new(uv, tex, side)
    );

    private static readonly vec3 LightColor = new(0.95f, 0.95f, 1f);
    private static readonly vec3 LightDir = new vec3(0.5f, 1, 0.25f).Normalized;

    private static float LightMultiplier(float dot) {
        return (dot + 1) * 0.2f + 0.6f;
    }

    public void EmitVertices(Atlas atlas, BlockModel.Builder builder, Dictionary<string, ResourceKey> textures, TransformStack stack) {
        var normal = stack.TransformNormal(new(0, 1, 0));
        var light = LightMultiplier(vec3.Dot(normal, LightDir)) * LightColor;
        if (textures.TryGetValue(Texture, out var textureKey) && atlas.TryGetSprite(textureKey, out var sprite))
            builder
                .AddVertex(CullingSide, new(stack.TransformPos(new(0, 1, 1)), light, sprite.GetTrueUV(new vec2(Uv.x, Uv.y)), new(1, 1), sprite))
                .AddVertex(CullingSide, new(stack.TransformPos(new(1, 1, 1)), light, sprite.GetTrueUV(new vec2(Uv.x, Uv.w)), new(0, 1), sprite))
                .AddVertex(CullingSide, new(stack.TransformPos(new(1, 1, 0)), light, sprite.GetTrueUV(new vec2(Uv.z, Uv.w)), new(0, 0), sprite))
                .AddVertex(CullingSide, new(stack.TransformPos(new(0, 1, 0)), light, sprite.GetTrueUV(new vec2(Uv.z, Uv.y)), new(1, 0), sprite));
    }
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
