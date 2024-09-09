#include "common/camera.glsl"
#include "common/model.glsl"

mat4 GetMV() {
    return ModelMatrix * ViewMatrix;
}

mat4 GetMVP(){
    return ModelMatrix * GetVP();
}

vec4 ModelVertex(vec3 pos){
    return vec4(pos, 1) * GetMVP();
}

vec3 ModelNormal(vec3 pos){
    return transpose(mat3(GetMV())) * pos;
}
