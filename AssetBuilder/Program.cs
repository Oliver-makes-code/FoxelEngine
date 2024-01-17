// See https://aka.ms/new-console-template for more information


using System.IO.Compression;
using Tomlyn;

string[] outputDirectories = {"Release", "Debug"};

var buildSettings = Toml.ToModel(File.ReadAllText("build_settings.toml"));

Console.WriteLine($"Packing files...");

if (!buildSettings.TryGetValue("input_folder", out object? value) || value is not string inPath)
    return;
if (!buildSettings.TryGetValue("output_folder", out value) || value is not string outPath)
    return;

inPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), inPath));
outPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), outPath));

Console.WriteLine($"Folders are {inPath} and {outPath}");

foreach (string directory in outputDirectories) {
    var outDir = Path.GetFullPath(Path.Combine(outPath, directory));

    if (!Directory.Exists(outDir)) {
        Console.WriteLine($"Directory {outDir} doesn't exist.");
        continue;
    }

    foreach (string s in Directory.EnumerateDirectories(outDir)) {
        var finalPath = Path.Combine(s, Path.GetFileNameWithoutExtension(inPath) + ".zip");
        finalPath = Path.GetFullPath(finalPath);

        if (File.Exists(finalPath))
            File.Delete(finalPath);

        ZipFile.CreateFromDirectory(inPath, finalPath);
        Console.WriteLine($"Packed files from {inPath} to {finalPath}");
    }
}
