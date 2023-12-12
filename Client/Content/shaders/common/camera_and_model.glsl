#include "common/camera.glsl"
#include "common/model.glsl"

mat4 GetMVP(){
    return ModelMatrix * VPMatrix;
}

vec4 ModelVertex(vec3 pos){
    return vec4(pos, 1) * GetMVP();
}
