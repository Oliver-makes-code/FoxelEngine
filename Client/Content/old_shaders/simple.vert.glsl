#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in int PackedColor;
layout(location = 2) in int PackedUV;
layout(location = 3) in float AmbientOcclusion;
layout(location = 4) in vec2 UvMin;
layout(location = 5) in vec2 UvMax;

layout(location = 0) out vec2 fsin_texCoords;
layout(location = 1) out vec4 fsin_Color;
layout(location = 2) out float fsin_Distance;
layout(location = 3) out vec2 fsin_UvMin;
layout(location = 4) out vec2 fsin_UvMax;

struct UnpackedVertex{
    vec4 color;
    vec2 uv;
};

layout (set = 0, binding = 0) uniform CameraData {
    mat4 VPMatrix;
};

layout (set = 2, binding = 0) uniform ModelData {
    mat4 ModelMatrix;
};

UnpackedVertex unpack(int packedColor, int packedUV) {
    UnpackedVertex ret;

    ret.color = vec4(
        (packedColor & 255) / 255.0f,
        ((packedColor >> 8) & 255) / 255.0f,
        ((packedColor >> 16) & 255) / 255.0f,
        ((packedColor >> 24) & 255) / 255.0f
    );

    ret.uv = vec2(
        (packedUV & 65535) / 65535.0f,
        ((packedUV >> 16) & 65535) / 65535.0f
    );

    return ret;
}

float singleInterp(float strength, float baseColor) {
    float min = 0.95 * baseColor * baseColor;
    return min + (baseColor - min) * strength;
}

vec3 getColorMultiplier(float strength, vec3 baseColor) {
    return vec3(
        singleInterp(strength, baseColor.x),
        singleInterp(strength, baseColor.y),
        singleInterp(strength, baseColor.z)
    );
}

void main() {
    mat4 mvp = ModelMatrix * VPMatrix;
    vec4 pos = vec4(Position, 1) * mvp;
    gl_Position = pos;

    UnpackedVertex up = unpack(PackedColor, PackedUV);
    fsin_texCoords = up.uv;
    
    fsin_Color = up.color * vec4(getColorMultiplier(1-AmbientOcclusion, vec3(0.9, 0.9, 1)), 1);
    fsin_Distance = pos.z;
    fsin_UvMin = UvMin;
    fsin_UvMax = UvMax;
}
