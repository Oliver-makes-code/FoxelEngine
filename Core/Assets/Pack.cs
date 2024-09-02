using Newtonsoft.Json;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;
using Greenhouse.Libs.Serialization.Reader;

namespace Foxel.Core.Assets;

public interface Pack : IDisposable {
    public static string BuildPath(AssetType type, ResourceKey key)
        => $"{key.Group}/{type.AsString()}/{key.Value}";

    public Stream? OpenRoot(string path);

    public IEnumerable<ResourceKey> ListResources(AssetType type, string prefix = "", string suffix = "");

    public IEnumerable<string> ListGroups();

    public Stream? OpenStream(AssetType type, ResourceKey key)
        => OpenRoot(BuildPath(type, key));

    public PackMetadata? GetMetadata() {
        try {
            using var root = OpenRoot("root.json");
            if (root == null)
                return null;
            using var sr = new StreamReader(root);
            using var jr = new JsonTextReader(sr);
            var reader = new JsonDataReader(jr);
            
            return PackMetadata.Codec.ReadGeneric(reader);
        } catch {
            return null;
        }
    }
}

public record PackMetadata(
    string? Name,
    string? Description,
    string Version,
    GameMetadata? Metadata
) {
    public static readonly Codec<PackMetadata> Codec = RecordCodec<PackMetadata>.Create(
        Codecs.String.NullableField<string, PackMetadata>("name", it => it.Name),
        Codecs.String.NullableField<string, PackMetadata>("description", it => it.Description),
        Codecs.String.Field<PackMetadata>("version", it => it.Version),
        GameMetadata.Codec.NullableField<GameMetadata, PackMetadata>("metadata", it => it.Metadata),
        (name, desc, ver, meta) => new(name, desc, ver, meta) 
    );
}

public record GameMetadata(
    string[]? TargetVersions,
    bool? RequireExactVersion
) {
    public static readonly Codec<GameMetadata> Codec = RecordCodec<GameMetadata>.Create(
        Codecs.String.Array().NullableField<string[], GameMetadata>("target_versions", it => it.TargetVersions),
        Codecs.Bool.NullableField<bool, GameMetadata>("require_exact_version", it => it.RequireExactVersion),
        (targetVersions, requireExactVersion) => new(targetVersions, requireExactVersion)
    );
}

public enum AssetType {
    Assets = 0,
    Content = 1
}

public static class AssetTypeExtensions {
    public const string AssetsDir = "assets";
    public const string ContentDir = "content";

    private static readonly string[] Types = [AssetsDir, ContentDir];
    
    public static string AsString(this AssetType type) {
        return Types[(int)type];
    }
}
