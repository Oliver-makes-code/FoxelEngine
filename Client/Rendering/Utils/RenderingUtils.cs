using GlmSharp;

namespace Voxel.Rendering.Utils;

public static class RenderingUtils {


    public static float BiliniearInterpolation(vec4 values, vec2 position) {
        position = vec2.Clamp(position, vec2.Zero, vec2.Ones);
        var top = values.x + (values.y - values.x) * position.x;
        var bottom = values.w + (values.z - values.w) * position.x;

        return top + (bottom - top) * position.y;
    }

}
