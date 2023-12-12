layout (set = 0, binding = 0) uniform sampler TextureSampler;
layout (set = 0, binding = 1) uniform texture2D Texture;
layout (set = 1, binding = 0) uniform TextureDrawParams {
    bool flip;
};

void vert(vec3 position, out vec2 uv){
    gl_Position = vec4((position.xy * 2) - 1, 0, 1);

    //Sample from source texture.

    uv = position.xy;
    if (flip)
    uv = vec2(uv.x, 1 - uv.y);
}

void frag(vec2 uv, out vec4 o_color){
    o_color = texture(sampler2D(Texture, TextureSampler), uv);
}
