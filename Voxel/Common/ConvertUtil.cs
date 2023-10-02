using GlmSharp;
using Microsoft.Xna.Framework;

namespace Voxel.Common; 

/// <summary>
/// Just for the transition of monogame to gmlsharp
/// </summary>
public static class ConvertUtil {
    public static Vector2 ToXnaVector2(this vec2 vec)
        => new(vec.x, vec.y);
    
    public static Vector3 ToXnaVector3(this vec3 vec)
        => new(vec.x, vec.y, vec.z);
    
    public static Vector4 ToXnaVector4(this vec4 vec)
        => new(vec.x, vec.y, vec.z, vec.w);
    
    public static vec2 ToGlmVector2(this Vector2 vec)
        => new(vec.X, vec.Y);
    
    public static vec3 ToGlmVector3(this Vector3 vec)
        => new(vec.X, vec.Y, vec.Z);
    
    public static vec4 ToGlmVector4(this Vector4 vec)
        => new(vec.X, vec.Y, vec.Z, vec.W);

    public static Matrix ToXnaMatrix4(this mat4 m)
        => new(m.Column0.ToXnaVector4(), m.Column1.ToXnaVector4(), m.Column2.ToXnaVector4(), m.Column3.ToXnaVector4());

    public static mat4 ToGlmMatrix4(this Matrix m)
        => new(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        );
}
