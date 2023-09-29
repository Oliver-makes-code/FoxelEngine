using System;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering;
using Voxel.Client.World;
using Voxel.Common.Tile;
using Voxel.Common.World;

namespace Voxel.Client;

public class VoxelClient : Game {
    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static VoxelClient? Instance { get; private set; }
    
    public Camera? camera;

    private readonly GraphicsDeviceManager _graphics;

    SpriteBatch? batch;

    SpriteFont? font;


    Effect? effect;

    IndexBuffer? indexBuffer;

    ClientWorld? world;

    float AspectRatio {
        get {
            if (GraphicsDevice == null)
                return 0;
            if (Window == null)
                return GraphicsDevice.DisplayMode.AspectRatio;
            float x = Width;
            float y = Height;
            return x/y;
        }
    }

    int Width => Window.ClientBounds.Width;
    int Height => Window.ClientBounds.Height;

    float[] previous = new float[40];

    int count;

    Timer? tickTimer;
    Thread? chunkBuildThread;
    Thread? chunkLoadUnloadThread;

    public VoxelClient() {
        Instance = this;

        _graphics = new(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.PreferMultiSampling = true;
        _graphics.PreparingDeviceSettings += (_, args) => {
            args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
        };

        Run();
    }

    protected override void Initialize() {
        base.Initialize();

        batch = new(GraphicsDevice);

        font = Content.Load<SpriteFont>("Arial_Font");

        world = new(new(), GraphicsDevice);

        ClientConfig.Load();
        ClientConfig.Save();

        effect = Content.Load<Effect>("Main_Eff");

        camera = new(AspectRatio);

        effect.Parameters["Projection"].SetValue(camera.Projection);
        effect.Parameters["World"].SetValue(camera.World);
        effect.Parameters["Texture"].SetValue(Content.Load<Texture2D>("terrain"));

        Window.AllowUserResizing = true;

        Window.ClientSizeChanged += (_, _) => {
            camera!.UpdateProjection(AspectRatio);
            effect.Parameters["Projection"].SetValue(camera.Projection);
        };

        GamePad.InitDatabase();

        tickTimer = new(_ => TickClient(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
        chunkBuildThread = new(() => {
            while (true) {
                world!.BuildChunks();
                Thread.Sleep(16);
            }
        });
        chunkLoadUnloadThread = new(() => {
            while (true) {
                world!.LoadChunks();
                world.UnloadChunks();
                Thread.Sleep(16);
            }
        });
        chunkBuildThread.Start();
        chunkLoadUnloadThread.Start();

        indexBuffer = Quad.GenerateCommonIndexBuffer(GraphicsDevice);
    }

    private void TickClient() {
        Vector3 moveDir = new(0, 0, 0);
        Vector2 rotDir = new(0, 0);

        Keybinds.Poll();

        if (Keybinds.pause.isPressed) {
            Exit();
        }
        if (Keybinds.strafeRight.isPressed) {
            moveDir.X -= Keybinds.strafeRight.strength;
        }
        if (Keybinds.strafeLeft.isPressed) {
            moveDir.X += Keybinds.strafeLeft.strength;
        }
        if (Keybinds.backward.isPressed) {
            moveDir.Z -= Keybinds.backward.strength;
        }
        if (Keybinds.forward.isPressed) {
            moveDir.Z += Keybinds.forward.strength;
        }
        if (Keybinds.jump.isPressed) {
            moveDir.Y += 0.5f;
        }
        if (Keybinds.crouch.isPressed) {
            moveDir.Y -= 0.5f;
        }
        if (Keybinds.lookRight.isPressed) {
            rotDir.X -= MathHelper.ToRadians(Keybinds.lookRight.strength * 4);
        }
        if (Keybinds.lookLeft.isPressed) {
            rotDir.X += MathHelper.ToRadians(Keybinds.lookLeft.strength * 4);
        }
        if (Keybinds.lookUp.isPressed) {
            rotDir.Y += MathHelper.ToRadians(Keybinds.lookUp.strength * 4);
        }
        if (Keybinds.lookDown.isPressed) {
            rotDir.Y -= MathHelper.ToRadians(Keybinds.lookDown.strength * 4);
        }
        if (Keybinds.use.justPressed) {
            var pos = world!.world.Cast(camera!.Position, camera.Position + camera.Project(5));

            if (pos.HasValue) {
                world.world.SetBlock(pos.Value, Blocks.Air);
            }
        }

        camera!.Move(moveDir, rotDir);

        camera.UpdateViewMatrix();

        ChunkPos chunkPos = new TilePos(camera.Position).ChunkPos();
        int dist = ClientConfig.General.RenderDistance;

        for (int dx = -dist; dx <= dist; dx++) {
            for (int dz = -dist; dz <= dist; dz++) {
                var posd = new ChunkPos(chunkPos.x + dx, 0, chunkPos.z + dz);
                var posu = posd.Up();

                if (!world!.world.IsChunkLoaded(posd))
                    world.world.ChunksToLoad.Enqueue(posd);
                if (!world.world.IsChunkLoaded(posu))
                    world.world.ChunksToLoad.Enqueue(posu);
            }
        }
        Monitor.Enter(world!.loadedChunks);
        var chunks = world.loadedChunks.Keys.ToArray();
        Monitor.Exit(world.loadedChunks);
        foreach (var chunk in chunks) {
            if (
                chunk.x > chunkPos.x - (dist+1) &&
                chunk.x < chunkPos.x + (dist+1) &&
                chunk.z > chunkPos.z - (dist+1) &&
                chunk.z < chunkPos.z + (dist+1)
            ) continue;
            if (world.world.IsChunkLoaded(chunk))
                world.world.ChunksToRemove.Enqueue(chunk);
        }
    }

    protected override void OnExiting(object sender, EventArgs args) {
        chunkBuildThread?.Interrupt();
        chunkLoadUnloadThread?.Interrupt();
        world?.world?.OnExiting();
        base.OnExiting(sender, args);
    }

    protected override void Draw(GameTime gameTime) {
        var samplerState = new SamplerState {
            Filter = TextureFilter.PointMipLinear
        };
        GraphicsDevice.SamplerStates[0] = samplerState;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        var rasterizerState = new RasterizerState() {
            // FillMode = FillMode.WireFrame
        };
        GraphicsDevice.RasterizerState = rasterizerState;

        GraphicsDevice.Clear(Color.CornflowerBlue);

        effect!.Parameters["View"].SetValue(camera!.View);

        GraphicsDevice.Indices = indexBuffer;

        world!.Draw(effect, camera);

        var fps = 1000f / (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        previous[count] = fps;

        count++;
        count %= previous.Length;

        fps = 0;
        foreach (var c in previous) {
            fps += c;
        }

        fps /= previous.Length;

        fps = MathF.Round(fps);

        var originalViewport = GraphicsDevice.Viewport;
        GraphicsDevice.Viewport = new Viewport(0, 0, Width, Height);

        batch!.Begin();
        batch.DrawString(font, $"{fps}", new(10, 10), Color.White);
        batch.DrawString(font, $"{camera.GetRotationDirection()}", new(10, 30), Color.White);
        batch.DrawString(font, $"{camera.Rotation}", new(10, 60), Color.White);
        batch.End();

        GraphicsDevice.Viewport = originalViewport;

        base.Draw(gameTime);
    }
}
