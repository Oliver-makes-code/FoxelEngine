using Newtonsoft.Json;
using Voxel.Core.Util;

namespace Voxel.Core.Assets;

public interface ContentPack : IDisposable {
    public static readonly JsonSerializer Serializer = new();

    public static string BuildPath(AssetType type, ResourceKey key)
        => $"{key.Group}/{type.AsString()}/{key.Value}";

    public Stream? OpenRoot(string path);

    public IEnumerable<ResourceKey> ListResources(AssetType type, string prefix = "", string suffix = "");

    public IEnumerable<string> ListGroups();

    public Stream? OpenStream(AssetType type, ResourceKey key)
        => OpenRoot(BuildPath(type, key));

    public PackMetadata? GetMetadata()  {
        try {
            using var root = OpenRoot("");
            if (root == null)
                return null;
            using var reader = new StreamReader(root);
            using var jsonReader = new JsonTextReader(reader);
            
            return Serializer.Deserialize<PackMetadata>(jsonReader);
        } catch {
            return null;
        }
    }
}

public class PackMetadata {
    public string? Name;
    public string? Description;
    public string? Version;
    public GameMetadata? Game;

    public class GameMetadata {
        public string[]? TargetVersion;
        public bool RequireExactVersion = false;
    }
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
