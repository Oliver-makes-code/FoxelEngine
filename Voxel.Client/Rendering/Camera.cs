using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Voxel.Client.Rendering;

public class Camera {
    public Vector2 Rotation;
    public Vector3 Target;
    public Vector3 Position;

    public Matrix Projection;
    public Matrix View;
    public Matrix World;

    public Camera(GraphicsDevice graphicsDevice) {
        Rotation = new(0, MathHelper.ToRadians(90));

        Target = new(0f, 0f, 0f);
        Position = new(0f, 0f, -100f);

        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45),
            graphicsDevice.DisplayMode.AspectRatio,
            1f, 1000f
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

        float dist = MathF.Sqrt((dir.X*dir.X)+(dir.Z*dir.Z));

        float atan = MathF.Atan2(dir.X, dir.Z);

        Position.X += MathF.Sin(Rotation.X+atan)*dist;
        Position.Y += dir.Y;
        Position.Z += MathF.Cos(Rotation.X+atan)*dist;

        UpdateTarget();
    }

    public void UpdateTarget() {
        var sinY = MathF.Sin(Rotation.Y);

        var x = MathF.Sin(Rotation.X) * sinY;
        var z = MathF.Cos(Rotation.X) * sinY;
        var y = MathF.Cos(Rotation.Y);

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
}