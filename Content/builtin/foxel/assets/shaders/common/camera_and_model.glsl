#include "common/camera.glsl"
#include "common/model.glsl"

vec3 OffsetToModel(vec3 pos) {
    return vec3(pos - CameraPosition + ModelPosition);
}

vec4 ModelVertex(vec3 pos){
    return vec4(OffsetToModel(pos), 1) * GetVP();
}

vec3 ModelNormal(vec3 pos){
    return transpose(mat3(ViewMatrix)) * pos;
}
