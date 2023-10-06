// See https://aka.ms/new-console-template for more information


using System.IO.Compression;
using Tomlyn;

var outputDirectories = new[] {
    "Release",
    "Debug"
};

var buildSettings = Toml.ToModel(File.ReadAllText("build_settings.toml"));

if (!buildSettings.TryGetValue("input_folder", out object? value) || value is not string inPath)
    return;
if (!buildSettings.TryGetValue("output_folder", out value) || value is not string outPath)
    return;

foreach (string directory in outputDirectories) {

    var outDir = Path.Combine(outPath, directory);

    if(!Directory.Exists(outDir))
        continue;
    
    foreach (string s in Directory.EnumerateDirectories(outDir)) {
        var finalPath = Path.Combine(s, Path.GetFileNameWithoutExtension(inPath) + ".zip");
        
        if (File.Exists(finalPath))
            File.Delete(finalPath);
        
        ZipFile.CreateFromDirectory(inPath, finalPath);
    }
}
