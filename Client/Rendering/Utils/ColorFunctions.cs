using System.Runtime.CompilerServices;
using GlmSharp;
using Voxel.Common.Util;

namespace Voxel.Rendering.Utils; 

public static class ColorFunctions {
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static vec3 GetColorMultiplier(float strength, vec3 color) {
        var output = new vec3();

        for (int i = 0; i < 3; i++) {
            float currentColor = color[i];
            
            float squaredColor = 0.95f * currentColor * currentColor;
            output[i] = MathHelper.LerpF(squaredColor, currentColor, strength);
        }
        
        return output;
    }
}
