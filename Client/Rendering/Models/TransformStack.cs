using System.Collections.Generic;
using GlmSharp;

namespace Foxel.Client.Rendering.Models;

public class TransformStack {
    private readonly List<TransformFrame> Stack = [];

    public void Clear() {
        Stack.Clear();
    }

    public mat4 ToMat4() {
        mat4 value = mat4.Identity;
        for (int i = Stack.Count - 1; i >= 0; i--)
            value = Stack[i].ToMat4() * value;
        return value;
    }

    public vec3 TransformPos(vec3 pos) {
        vec4 transformed = ToMat4() * new vec4(pos, 1);
        return transformed.xyz / transformed.w;
    }

    public vec3 TransformNormal(vec3 normal) {
        for (int i = Stack.Count - 1; i >= 0; i--)
            normal = Stack[i].TransformNormal(normal);
        return normal;
    }

    public void PushTransform(TransformFrame frame) {
        Stack.Add(frame);
    }

    public void PopTransform() {
        Stack.RemoveAt(Stack.Count - 1);
    }
}

public struct TransformFrame {
    public vec3 position;
    public vec3 size;
    public vec3 pivot;
    public quat rotation;

    public readonly mat4 ToMat4() {
        mat4 position = mat4.Translate(this.position - this.pivot);
        mat4 size = mat4.Scale(this.size).Transposed;
        mat4 pivot = mat4.Translate(this.pivot);
        mat4 rotation = this.rotation.ToMat4;
        return pivot * rotation * position * size;
    }

    public readonly vec3 TransformPos(vec3 pos) {
        vec4 transformed = ToMat4() * new vec4(pos, 1);
        return transformed.xyz / transformed.w;
    }

    public readonly vec3 TransformNormal(vec3 normal)
        => rotation * normal;
}
