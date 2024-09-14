#ifndef CAMERA_SET
#define CAMERA_SET 0
#endif

layout (set = CAMERA_SET, binding = 0) uniform CameraData {
    mat4 ViewMatrix;
    mat4 ProjectMatrix;
    dvec3 CameraPosition;
};

mat4 GetVP() {
    return ViewMatrix * ProjectMatrix;
}
