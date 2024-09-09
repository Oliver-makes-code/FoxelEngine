layout (set = 0, binding = 0) uniform CameraData {
    mat4 ViewMatrix;
    mat4 ProjectMatrix;
};

mat4 GetVP() {
    return ViewMatrix * ProjectMatrix;
}
