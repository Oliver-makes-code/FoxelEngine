using System.IO.Compression;
using Newtonsoft.Json;
using Voxel.Core.Util;

namespace Voxel.Core.Assets;

public sealed class ZipPack : Pack {
    private readonly ZipArchive File;

    public ZipPack(ZipArchive file) {
        File = file;
    }

    public IEnumerable<string> ListGroups() {
        HashSet<string> visited = [];
        foreach (var entry in File.Entries) {
            var entryName = entry.FullName;
            var idx = entryName.IndexOf('/');
            if (idx == -1)
                continue;
            var dirName = entryName[..idx];
            if (visited.Contains(dirName))
                continue;
            visited.Add(dirName);
            yield return dirName;
        }
    }

    public IEnumerable<ResourceKey> ListResources(AssetType type, string prefix = "", string suffix = "") {
        // I fucking hate strings oh my god
        // This shit fucking sucks
        foreach (var entry in File.Entries) {
            var entryName = entry.FullName;
            // Extract group
            var idxOfFirst = entryName.IndexOf('/');
            if (idxOfFirst == -1)
                continue;
            var group = entryName[..idxOfFirst];
            // Check type
            var idxOfSecond = entryName.IndexOf('/', idxOfFirst+1);
            var typeName = entryName[(idxOfFirst+1)..idxOfSecond];
            if (typeName != type.AsString())
                continue;
            // Get path
            var path = entryName[(idxOfSecond + 1)..];
            if (
                path.StartsWith(prefix)
                && path.EndsWith(suffix)
                && path.Length >= prefix.Length + suffix.Length
            )
                yield return new(group, path);
        }
        
        // // Keep this here. We might want to test performance later.
        // // I doubt it would be any faster, considering how much iteration is being done.
        // // Here's the original comment, preserved for transparency:

        // // This isn't good code.
        // // We're iterating through every entry to get the group,
        // // Then for each group we're iterating through each entry to check the path.
        // // This is O(n^2) at best.
        // // I know for a fact it's possible to do it with only one iteration,
        // // But I am not in the mood right now to deal with that string manipulation.
        
        // foreach (var group in ListGroups()) {
        //     var path = $"{group}/{type.AsString()}/{prefix}";
        //     foreach (var entry in File.Entries) {
        //         var name = entry.FullName;
        //         if (!name.StartsWith(path))
        //             continue;
        //         var trimmedName = name.Substring(path.Length);
        //         if (!name.EndsWith(suffix))
        //             continue;
        //         yield return new(group, prefix+trimmedName);
        //     }
        // }
    }

    public Stream? OpenRoot(string path)
        => File.GetEntry(path)?.Open();

    public void Dispose()
        => File.Dispose();
}