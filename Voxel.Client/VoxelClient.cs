using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering;
using Voxel.Client.World;
using Voxel.Common.World;

namespace Voxel.Client;

public class VoxelClient : Game {
    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static VoxelClient? Instance { get; private set; }

    private readonly GraphicsDeviceManager _graphics;

    SpriteBatch? batch;

    SpriteFont? font;

    Camera? camera;

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

    int count = 0;

    Timer? tickTimer;
    Thread? chunkBuildThread;
    Thread? chunkRebuildThread;
    Thread? chunkUnloadThread;

    public VoxelClient() {
        Instance = this;

        _graphics = new(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = true;
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
                Thread.Sleep(50);
            }
        });
        chunkRebuildThread = new(() => {
            while (true) {
                world.RebuildChunks();
                Thread.Sleep(50);
            }
        });
        chunkUnloadThread = new(() => {
            while (true) {
                world.UnloadChunks();
                Thread.Sleep(50);
            }
        });
        chunkBuildThread.Start();
        chunkRebuildThread.Start();
        chunkUnloadThread.Start();

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

        camera!.Move(moveDir, rotDir);

        camera.UpdateViewMatrix();

        ChunkPos chunkPos = new BlockPos(camera.Position).ChunkPos();
        for (int dx = -3; dx < 4; dx++) {
            for (int dz = -3; dz < 4; dz++) {
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
                chunk.x > chunkPos.x - 4 &&
                chunk.x < chunkPos.x + 4 &&
                chunk.z > chunkPos.z - 4 &&
                chunk.z < chunkPos.z + 4
            ) continue;
            if (world.world.IsChunkLoaded(chunk))
                world.world.ChunksToRemove.Enqueue(chunk);
        }
    }

    protected override void OnExiting(object sender, EventArgs args) {
        chunkBuildThread?.Interrupt();
        chunkRebuildThread?.Interrupt();
        chunkUnloadThread?.Interrupt();
        world?.world?.OnExiting();
        base.OnExiting(sender, args);
    }

    protected override void Draw(GameTime gameTime) {
        var samplerState = new SamplerState {
            Filter = TextureFilter.Point
        };
        GraphicsDevice.SamplerStates[0] = samplerState;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        var rasterizerState = new RasterizerState() {
            FillMode = FillMode.WireFrame
        };
        // GraphicsDevice.RasterizerState = rasterizerState;

        GraphicsDevice.Clear(Color.CornflowerBlue);

        effect!.Parameters["View"].SetValue(camera!.View);

        GraphicsDevice.Indices = indexBuffer;

        world!.Draw(effect, camera, out var points);

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

        foreach ((var pos, string s) in points) {
            //batch.DrawString(font, s, pos, Color.White);
        }
        batch.End();

        GraphicsDevice.Viewport = originalViewport;

        base.Draw(gameTime);
    }
}
