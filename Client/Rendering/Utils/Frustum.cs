using System;
using GlmSharp;
using Foxel.Common.Collision;
using Foxel.Common.Util;
using Foxel.Core.Util;

namespace Foxel.Client.Rendering.Utils;

public struct Frustum {
    public Plane near, far;
    public Plane right, left;
    public Plane top, bottom;

    public Frustum(Camera c) {
        var quat = c.rotationVec.RotationVecToQuat();
        var camBack = quat * dvec3.UnitZ;
        var camRight = quat * dvec3.UnitX;
        var camUp = quat * dvec3.UnitY;

        float halfVSide = c.farClip * MathF.Tan(c.fovy * 0.5f);
        float halfHSide = halfVSide * c.aspect;

        var farPos = camBack * c.farClip;

        near = new(c.position - camBack * c.nearClip, -camBack);
        far = new(c.position - farPos, camBack);

        right = new(c.position, dvec3.Cross(farPos - camRight * halfHSide, -camUp));
        left = new(c.position, dvec3.Cross(-camUp, farPos + camRight * halfHSide));

        top = new(c.position, dvec3.Cross(camRight, farPos + camUp * halfVSide));
        bottom = new(c.position, dvec3.Cross(farPos - camUp * halfVSide, camRight));
    }

    public bool TestBox(Box box)
        => box.TestAgainstPlane(near) && box.TestAgainstPlane(far) && box.TestAgainstPlane(right) && box.TestAgainstPlane(left) && box.TestAgainstPlane(top) && box.TestAgainstPlane(bottom);
}
