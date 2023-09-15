using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Voxel.Client.Rendering;

public class Camera {
    public Vector3 Target;
    public Vector3 Position;

    public Matrix Projection;
    public Matrix View;
    public Matrix World;

    public Camera(GraphicsDevice graphicsDevice) {
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
            Position,
            Vector3.Forward,
            Vector3.Up
        );
    }

    public void Move(Vector3 dir) {
        Position.X += dir.X;
        Position.Y += dir.Y;
        Position.Z += dir.Z;

        Target.X += dir.X;
        Target.Y += dir.Y;
        Target.Z += dir.Z;
    }

    public void UpdateViewMatrix() {
        View = Matrix.CreateLookAt(
            Position,
            Target,
            Vector3.Up
        );
    }
}