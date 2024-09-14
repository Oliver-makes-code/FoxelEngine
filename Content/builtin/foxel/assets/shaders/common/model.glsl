#ifndef MODEL_SET
#define MODEL_SET 1
#endif

layout (set = MODEL_SET, binding = 0) uniform ModelData {
    ivec3 ModelPosition;
};
