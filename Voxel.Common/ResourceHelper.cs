using System.Reflection;
using System.Text;

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
}
