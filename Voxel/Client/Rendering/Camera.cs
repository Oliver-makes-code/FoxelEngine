using System;
using GlmSharp;
using Microsoft.Xna.Framework;
using Voxel.Common;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class Camera {
    public vec2 Rotation;
    public vec3 Target;
    public vec3 Position;

    public mat4 Projection;
    public mat4 View;
    public mat4 World;

    public Camera(float aspectRatio) {
        Rotation = new(MathHelper.ToRadians(0), MathHelper.ToRadians(-90));
        
        Position = new(16f,  72f, 16f);
        
        UpdateTarget();

        UpdateProjection(aspectRatio);
        
        UpdateViewMatrix();

        World = Matrix.CreateWorld(
            new(0, 0, 0),
            Vector3.Forward,
            Vector3.Up
        ).ToGlmMatrix4();
    }

    public void Move(vec3 dir, vec2 rotation) {
        Rotation.x += rotation.x;
        Rotation.y += rotation.y;

        Rotation.x %= MathF.Tau;
        if (Rotation.x < 0)
            Rotation.x += MathF.Tau;

        if (Rotation.y > MathF.PI/2 - 0.001f)
            Rotation.y = MathF.PI/2 - 0.001f;
        if (Rotation.y < -MathF.PI/2 + 0.001f)
            Rotation.y = -MathF.PI/2 + 0.001f;

        int sign = dir.x == 0 && dir.z == 0 ? 0 : 1;

        float atan = MathF.Atan2(dir.x, dir.z);

        float angle = Rotation.x + atan;

        Position.x += MathF.Sin(angle)*sign*0.25f;
        Position.y += dir.y;
        Position.z += MathF.Cos(angle)*sign*0.25f;

        UpdateTarget();
    }

    public void UpdateTarget() {
        var project = Project();

        Target.x = Position.x + project.x;
        Target.y = Position.y + project.y;
        Target.z = Position.z + project.z;
    }

    public vec3 Project() {
        var cosY = MathF.Cos(Rotation.y);

        var x = MathF.Sin(Rotation.x) * cosY;
        var z = MathF.Cos(Rotation.x) * cosY;
        var y = MathF.Sin(Rotation.y);

        return new(x, y, z);
    }

    public vec3 Project(float distance)
        => Project() * distance;

    public void UpdateViewMatrix() {
        View = mat4.LookAt(
            Position,
            Target,
            new(0, 1, 0)
        );
    }

    public void UpdateProjection(float aspect) {
        Projection = mat4.Perspective(
            MathHelper.ToRadians(ClientConfig.General.Fov),
            aspect,
            0.001f, 1000f
        );
    }

    public bool IsPointVisible(vec3 point) {
        Vector3 viewSpace = Vector3.Transform(point.ToXnaVector3(), View.ToXnaMatrix4());
        Vector4 clipSpace = Vector4.Transform(new Vector4(viewSpace, 1), Projection.ToXnaMatrix4());
        return Math.Abs(clipSpace.X) <= clipSpace.W &&
            Math.Abs(clipSpace.Y) <= clipSpace.W &&
            Math.Abs(clipSpace.Z) <= clipSpace.W;
    }

    public float DistanceTo(vec3 point) => vec3.DistanceSqr(Position, point);

    public TilePos.Axis GetHorizontalAxis() {
        var raw = (int)(MathF.Round(Rotation.x / MathF.Tau * 4) % 4);
        return raw switch {
            0 => TilePos.Axis.PositiveZ,
            1 => TilePos.Axis.PositiveX,
            2 => TilePos.Axis.NegativeZ,
            3 => TilePos.Axis.NegativeX,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public TilePos.Axis GetVerticalAxis()
        => Rotation.y < MathF.PI / 2 ? TilePos.Axis.NegativeY : TilePos.Axis.PositiveY;

    public TilePos.Axis GetAxis() 
        => Rotation.y is < -MathF.PI / 4 or > MathF.PI / 4 ? GetVerticalAxis() : GetHorizontalAxis();

    public string GetRotationDirection()
        => ((RotationDirection)(int)(MathF.Round(Rotation.x/MathF.Tau*8) % 8)).ToString();
    public string GetCoordDirection()
        => GetVerticalAxis().ToString();
    public enum RotationDirection {
        South,
        SouthEast,
        East,
        NorthEast,
        North,
        NorthWest,
        West,
        SouthWest
    }

    public enum CoordDirection {
        PositiveZ,
        PositiveX,
        NegativeZ,
        NegativeX
    }
}
