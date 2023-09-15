using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using Voxel.Client.Rendering;

namespace Voxel.Client;

public class VoxelClient : Game {
    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static VoxelClient? Instance { get; private set; }

    private readonly GraphicsDeviceManager _graphics;

    Camera? camera;

    BasicEffect? basicEffect;

    VertexPositionColor[]? tri;

    VertexBuffer? vertexBuffer;

    float AspectRatio {
        get {
            if (GraphicsDevice == null)
                return 0;
            if (Window == null)
                return GraphicsDevice.DisplayMode.AspectRatio;
            float x = Window.ClientBounds.Width;
            float y = Window.ClientBounds.Height;
            return x/y;
        }
    }

    public VoxelClient() {
        Instance = this;

        _graphics = new(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;

        Run();
    }

    protected override void Initialize() {
        base.Initialize();

        basicEffect = new(GraphicsDevice) {
            Alpha = 1.0f,
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        tri = new VertexPositionColor[3];
        
        tri[0] = new(new(0, 20, 0), Color.Red);
        tri[1] = new(new(-20, -20, 0), Color.Green);
        tri[2] = new(new(20, -20, 0), Color.Blue);

        vertexBuffer = new(GraphicsDevice, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);
        vertexBuffer.SetData(tri);

        camera = new(AspectRatio);

        Window.AllowUserResizing = true;

        Window.ClientSizeChanged += (_, _) => camera?.UpdateProjection(AspectRatio);
    }

    protected override void Update(GameTime gameTime) {
        Vector3 moveDir = new(0, 0, 0);
        Vector2 rotDir = new(0, 0);
        if (Keyboard.GetState().IsKeyDown(Keys.D)) {
            moveDir.X -= 1;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.A)) {
            moveDir.X += 1;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.S)) {
            moveDir.Z -= 1;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.W)) {
            moveDir.Z += 1;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Space)) {
            moveDir.Y += 1;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.LeftShift)) {
            moveDir.Y -= 1;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Right)) {
            rotDir.X -= (float)Math.PI/180;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Left)) {
            rotDir.X += (float)Math.PI/180;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Up)) {
            rotDir.Y += (float)Math.PI/180;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Down)) {
            rotDir.Y -= (float)Math.PI/180;
        }

        camera!.Move(moveDir, rotDir);

        camera.UpdateViewMatrix();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {

        GraphicsDevice.Clear(Color.CornflowerBlue);

        basicEffect!.Projection = camera!.Projection;
        basicEffect.View = camera.View;
        basicEffect.World = camera.World;

        GraphicsDevice.SetVertexBuffer(vertexBuffer);

        // Turn off backface culling
        RasterizerState state = new() {
            CullMode = CullMode.None
        };
        GraphicsDevice.RasterizerState = state;

        foreach (var pass in basicEffect.CurrentTechnique.Passes) {
            pass.Apply();
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 3);
        }

        base.Draw(gameTime);
    }
}
