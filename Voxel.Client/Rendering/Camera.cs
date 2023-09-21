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
        Rotation = new(MathHelper.ToRadians(45), MathHelper.ToRadians(-15));

        Target = new(0f, 0f, 0f);
        Position = new(-10f, 48f, -10f);

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
        var cosY = MathF.Cos(Rotation.Y);

        var x = MathF.Sin(Rotation.X) * cosY;
        var z = MathF.Cos(Rotation.X) * cosY;
        var y = MathF.Sin(Rotation.Y);

        Target.X = Position.X + x;
        Target.Y = Position.Y + y;
        Target.Z = Position.Z + z;
    }

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
}
