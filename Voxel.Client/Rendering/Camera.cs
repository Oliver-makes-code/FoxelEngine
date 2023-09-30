using System;
using Microsoft.Xna.Framework;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class Camera {
    public Vector2 Rotation;
    public Vector3 Target;
    public Vector3 Position;

    public Matrix Projection;
    public Matrix View;
    public Matrix World;

    public Camera(float aspectRatio) {
        Rotation = new(MathHelper.ToRadians(0), MathHelper.ToRadians(-90));

        Target = new(0f, 0f, 0f);
        Position = new(16f,  72f, 16f);

        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(ClientConfig.General.Fov),
            aspectRatio,
            0.001f, 1000f
        );

        View = Matrix.CreateLookAt(
            Position,
            Target,
            Vector3.Up
        );

        World = Matrix.CreateWorld(
            new(0, 0, 0),
            Vector3.Forward,
            Vector3.Up
        );
    }

    public void Move(Vector3 dir, Vector2 rotation) {
        Rotation.X += rotation.X;
        Rotation.Y += rotation.Y;

        Rotation.X %= MathF.Tau;
        if (Rotation.X < 0)
            Rotation.X += MathF.Tau;

        if (Rotation.Y > MathF.PI/2 - 0.001f)
            Rotation.Y = MathF.PI/2 - 0.001f;
        if (Rotation.Y < -MathF.PI/2 + 0.001f)
            Rotation.Y = -MathF.PI/2 + 0.001f;

        int sign = dir.X == 0 && dir.Z == 0 ? 0 : 1;

        float atan = MathF.Atan2(dir.X, dir.Z);

        float angle = Rotation.X + atan;

        Position.X += MathF.Sin(angle)*sign*0.25f;
        Position.Y += dir.Y;
        Position.Z += MathF.Cos(angle)*sign*0.25f;

        UpdateTarget();
    }

    public void UpdateTarget() {
        var project = Project();

        Target.X = Position.X + project.X;
        Target.Y = Position.Y + project.Y;
        Target.Z = Position.Z + project.Z;
    }

    public Vector3 Project() {
        var cosY = MathF.Cos(Rotation.Y);

        var x = MathF.Sin(Rotation.X) * cosY;
        var z = MathF.Cos(Rotation.X) * cosY;
        var y = MathF.Sin(Rotation.Y);

        return new(x, y, z);
    }

    public Vector3 Project(float distance)
        => Project() * distance;

    public void UpdateViewMatrix() {
        View = Matrix.CreateLookAt(
            Position,
            Target,
            Vector3.Up
        );
    }

    public void UpdateProjection(float aspect) {
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(ClientConfig.General.Fov),
            aspect,
            0.001f, 1000f
        );
    }

    public bool IsPointVisible(Vector3 point) {
        Vector3 viewSpace = Vector3.Transform(point, View);
        Vector4 clipSpace = Vector4.Transform(new Vector4(viewSpace, 1), Projection);
        return Math.Abs(clipSpace.X) <= clipSpace.W &&
            Math.Abs(clipSpace.Y) <= clipSpace.W &&
            Math.Abs(clipSpace.Z) <= clipSpace.W;
    }

    public float DistanceTo(Vector3 point) => Vector3.DistanceSquared(Position, point);

    public ChunkPos GetChunkPos() => new((int)(Position.X / 32), 0, (int)(Position.Z / 32));

    public TilePos.Axis GetHorizontalAxis() {
        var raw = (int)(MathF.Round(Rotation.X / MathF.Tau * 4) % 4);
        return raw switch {
            0 => TilePos.Axis.PositiveZ,
            1 => TilePos.Axis.PositiveX,
            2 => TilePos.Axis.NegativeZ,
            3 => TilePos.Axis.NegativeX,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public TilePos.Axis GetVerticalAxis()
        => Rotation.Y < MathF.PI / 2 ? TilePos.Axis.NegativeY : TilePos.Axis.PositiveY;

    public TilePos.Axis GetAxis() 
        => Rotation.Y is < -MathF.PI / 4 or > MathF.PI / 4 ? GetVerticalAxis() : GetHorizontalAxis();

    public string GetRotationDirection()
        => ((RotationDirection)(int)(MathF.Round(Rotation.X/MathF.Tau*8) % 8)).ToString();
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
