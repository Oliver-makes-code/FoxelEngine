using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace Voxel.Common;

public static class ResourceHelper {
    public static byte[]? GetResourceAsBytes(string path) {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        using Stream? stream = assembly.GetManifestResourceStream("Voxel.Resources." + path);

        if (stream == null)
            return null;
        
        byte[] buf = new byte[stream.Length];
        stream.Read(buf, 0, buf.Length);

        return buf;
    }

    public static string? GetResourceAsString(string path) {
        byte[]? bytes = GetResourceAsBytes(path);

        if (bytes == null)
            return null;

        return Encoding.UTF8.GetString(bytes);
    }

    public static XmlDocument? GetResourceAsXml(string path) {
        string? resource = GetResourceAsString(path);
        if (resource == null)
            return null;
        
        XmlDocument doc = new();
        doc.LoadXml(resource);

        return doc;
    }

    public static JsonDocument? GetResourceAsJson(string path) {
        string? resource = GetResourceAsString(path);
        if (resource == null)
            return null;

        return JsonDocument.Parse(resource);
    }
}
