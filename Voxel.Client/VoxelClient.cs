using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using Voxel.Client.Rendering;
using Voxel.Common.World;

namespace Voxel.Client;

public class VoxelClient : Game {
    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static VoxelClient? Instance { get; private set; }

    private readonly GraphicsDeviceManager _graphics;

    Camera? camera;

    BasicEffect? basicEffect;

    ChunkMesh? chunkA;
    ChunkMesh? chunkB;

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
        _graphics.SynchronizeWithVerticalRetrace = true;

        Run();
    }

    public void RedrawChunk() {
        chunkA = new ChunkMesh(GraphicsDevice, new(), new(0, 0, 0));
        chunkB = new ChunkMesh(GraphicsDevice, new(), new(32, 0, 0));
    }

    protected override void Initialize() {
        base.Initialize();

        basicEffect = new(GraphicsDevice) {
            Alpha = 1.0f,
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        camera = new(AspectRatio);

        basicEffect.Projection = camera.Projection;
        basicEffect.World = camera.World;

        RedrawChunk();

        Window.AllowUserResizing = true;

        Window.ClientSizeChanged += (_, _) => {
            camera!.UpdateProjection(AspectRatio);
            basicEffect.Projection = camera.Projection;
        };
    }

    protected override void Update(GameTime gameTime) {
        Vector3 moveDir = new(0, 0, 0);
        Vector2 rotDir = new(0, 0);
        if (Keyboard.GetState().IsKeyDown(Keys.D)) {
            moveDir.X -= 1f;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.A)) {
            moveDir.X += 1f;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.S)) {
            moveDir.Z -= 1f;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.W)) {
            moveDir.Z += 1f;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Space)) {
            moveDir.Y += 0.1f;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.LeftShift)) {
            moveDir.Y -= 0.1f;
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
        if (Keyboard.GetState().IsKeyDown(Keys.R)) {
            RedrawChunk();
        }

        camera!.Move(moveDir, rotDir);

        camera.UpdateViewMatrix();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        basicEffect!.View = camera!.View;

        chunkA!.Draw(GraphicsDevice, basicEffect);
        chunkB!.Draw(GraphicsDevice, basicEffect);

        base.Draw(gameTime);
    }
}
