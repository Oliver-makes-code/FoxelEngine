using System.Reflection;

namespace Voxel.Common;

public static class ResourceHelper {
    public static string GetResourceAsString(string path) {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream("Voxel.Resources."+path)) {
            if (stream != null) {
                using (StreamReader reader = new StreamReader(stream)) {
                    string content = reader.ReadToEnd();
                    return content;
                }
            }
        }
        return "";
    }
}