using System.Reflection;
using System.Runtime.InteropServices;

namespace Voxel.Common.World.Generation;

public static class GenerationUtils {


    public static void LoadNativeLibraries() {
        NativeLibrary.Load(Path.Combine("FastNoise"), Assembly.GetExecutingAssembly(), DllImportSearchPath.ApplicationDirectory);
    }
}
