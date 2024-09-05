using System.Collections.Generic;
using GlmSharp;

namespace Foxel.Client.Rendering.Models;

public class TransformStack {
    private readonly List<TransformFrame> Stack = [];

    public void Clear() {
        Stack.Clear();
    }

    public vec3 TransformPos(vec3 pos) {
        for (int i = Stack.Count - 1; i >= 0; i--)
            pos = Stack[i].TransformPos(pos);
        return pos;
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

    public readonly vec3 TransformPos(vec3 pos) {
        // Scale
        pos *= size;

        // Move to position
        pos += position;

        // Rotate
        pos -= pivot;
        pos *= rotation;
        pos += pivot;

        return pos;
    }

    public readonly vec3 TransformNormal(vec3 normal)
        => normal * rotation;
}
