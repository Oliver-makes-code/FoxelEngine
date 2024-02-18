using System;
using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.Util;
using Voxel.Core.Util;

namespace Voxel.Client.Rendering.Utils;

public struct Frustum {
    public Plane near, far;
    public Plane right, left;
    public Plane top, bottom;

    public Frustum(Camera c) {
        var quat = c.rotationVec.RotationVecToQuat();
        var camBack = quat * dvec3.UnitZ;
        var camRight = quat * dvec3.UnitX;
        var camUp = quat * dvec3.UnitY;

        var halfVSide = c.farClip * MathF.Tan(c.fovy * 0.5f);
        var halfHSide = halfVSide * c.aspect;

        var farPos = camBack * c.farClip;

        near = new Plane(c.position - camBack * c.nearClip, -camBack);
        far = new Plane(c.position - farPos, camBack);

        right = new Plane(c.position, dvec3.Cross(farPos - camRight * halfHSide, -camUp));
        left = new Plane(c.position, dvec3.Cross(-camUp, farPos + camRight * halfHSide));

        top = new Plane(c.position, dvec3.Cross(camRight, farPos + camUp * halfVSide));
        bottom = new Plane(c.position, dvec3.Cross(farPos - camUp * halfVSide, camRight));
    }

    public bool TestAABB(Box box)
        => box.TestAgainstPlane(near) && box.TestAgainstPlane(far) && box.TestAgainstPlane(right) && box.TestAgainstPlane(left) && box.TestAgainstPlane(top) && box.TestAgainstPlane(bottom);
}
