using System;
using System.Linq;
using System.Threading;
using GlmSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering;
using Voxel.Client.World;
using Voxel.Common;
using Voxel.Common.Tile;
using Voxel.Common.World;

namespace Voxel.Client;

/*public class VoxelClient : Game {
    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static VoxelClient? Instance { get; private set; }
    
    public Camera? camera;

    private readonly GraphicsDeviceManager _graphics;

    SpriteBatch? batch;

    private SpriteFont? font;


    private Effect? effect;

    private IndexBuffer? indexBuffer;

    private ClientWorld? world;

    private float AspectRatio {
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

    private int Width => Window.ClientBounds.Width;
    private int Height => Window.ClientBounds.Height;

    private float[] previous = new float[40];

    private int count;

    private Timer? tickTimer;
    private Thread[] chunkBuildThreads;
    public int chunkBuildThreadCount => chunkBuildThreads.Length;
    private Thread? chunkLoadUnloadThread;
    private Texture2D _crosshair;

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

        effect.Parameters["Projection"].SetValue(camera.Projection.ToXnaMatrix4());
        effect.Parameters["World"].SetValue(camera.World.ToXnaMatrix4());
        effect.Parameters["Texture"].SetValue(Content.Load<Texture2D>("terrain"));

        _crosshair = Content.Load<Texture2D>("crosshair");
        
        Window.AllowUserResizing = true;

        Window.ClientSizeChanged += (_, _) => {
            camera!.UpdateProjection(AspectRatio);
            effect.Parameters["Projection"].SetValue(camera.Projection.ToXnaMatrix4());
        };

        GamePad.InitDatabase();

        tickTimer = new(_ => TickClient(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
        
        chunkLoadUnloadThread = new(() => {
            while (true) {
                world!.LoadChunks();
                world.UnloadChunks();
                Thread.Sleep(16);
            }
        });
        chunkBuildThreads = new Thread[ClientConfig.General.ChunkBuildThreadCount];
        ChunkMesh.SetupThreadCount(chunkBuildThreads.Length);
        Mesh.SetupThreadCount(chunkBuildThreads.Length);
        MeshBuilder.SetupThreadCount(chunkBuildThreads.Length);
        for (int i = 0; i < chunkBuildThreads.Length; i++) {
            int j = i;
            chunkBuildThreads[i] = new(() => {
                while (true) {
                    world!.BuildChunks(j);
                    Thread.Sleep(16);
                }
            });
            
            chunkBuildThreads[i].Start();
        }
        chunkLoadUnloadThread.Start();

        indexBuffer = Quad.GenerateCommonIndexBuffer(GraphicsDevice);
    }

    private void TickClient() {
        vec3 moveDir = new(0, 0, 0);
        vec2 rotDir = new(0, 0);

        Keybinds.Poll();

        if (Keybinds.pause.isPressed) {
            Exit();
        }
        if (Keybinds.strafeRight.isPressed) {
            moveDir.x -= Keybinds.strafeRight.strength;
        }
        if (Keybinds.strafeLeft.isPressed) {
            moveDir.x += Keybinds.strafeLeft.strength;
        }
        if (Keybinds.backward.isPressed) {
            moveDir.z -= Keybinds.backward.strength;
        }
        if (Keybinds.forward.isPressed) {
            moveDir.z += Keybinds.forward.strength;
        }
        if (Keybinds.jump.isPressed) {
            moveDir.y += 0.5f;
        }
        if (Keybinds.crouch.isPressed) {
            moveDir.y -= 0.5f;
        }
        if (Keybinds.lookRight.isPressed) {
            rotDir.x -= MathHelper.ToRadians(Keybinds.lookRight.strength * 4);
        }
        if (Keybinds.lookLeft.isPressed) {
            rotDir.x += MathHelper.ToRadians(Keybinds.lookLeft.strength * 4);
        }
        if (Keybinds.lookUp.isPressed) {
            rotDir.y += MathHelper.ToRadians(Keybinds.lookUp.strength * 4);
        }
        if (Keybinds.lookDown.isPressed) {
            rotDir.y -= MathHelper.ToRadians(Keybinds.lookDown.strength * 4);
        }
        if (Keybinds.attack.justPressed) {
            var pos = world!.world.Cast(camera!.Position, camera.Position + camera.Project(5), camera.GetAxis());

            if (pos.HasValue) {
                world.world.SetBlock(pos.Value.pos, Blocks.Air);
            }
        }
        
        if (Keybinds.use.justPressed) {
            var pos = world!.world.Cast(camera!.Position, camera.Position + camera.Project(5), camera.GetAxis());

            if (pos.HasValue) {
                world.world.SetBlock(pos.Value.pos - pos.Value.axis, Blocks.Stone);
            }
        }

        camera!.Move(moveDir, rotDir);

        camera.UpdateViewMatrix();

        ChunkPos chunkPos = new TilePos(camera.Position).ChunkPos();
        int dist = ClientConfig.General.RenderDistance;
        var height = (int)(dist * 1.5);
        
        for (int dx = -dist; dx <= dist; dx++) {
            for (int dz = -dist; dz <= dist; dz++) {
                for (int dy = -height; dy <= height; dy++) {
                    var pos = new ChunkPos(chunkPos.x + dx, chunkPos.y + dy, chunkPos.z + dz);
                    if (!world!.world.IsChunkLoaded(pos))
                        world.world.ChunksToLoad.Enqueue(pos);
                }
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
                chunk.z < chunkPos.z + (dist+1) &&
                chunk.y > chunkPos.y - (height+1) &&
                chunk.y < chunkPos.y + (height+1)
            ) continue;
            if (world.world.IsChunkLoaded(chunk))
                world.world.ChunksToRemove.Enqueue(chunk);
        }
    }

    protected override void OnExiting(object sender, EventArgs args) {
        foreach (var chunkBuildThread in chunkBuildThreads) {
            chunkBuildThread.Interrupt();
        }
        chunkLoadUnloadThread?.Interrupt();
        world?.world?.OnExiting();
        base.OnExiting(sender, args);
    }

    protected override void Draw(GameTime gameTime) {
        var samplerState = new SamplerState {
            Filter = TextureFilter.PointMipLinear,
            MaxAnisotropy = 16,
            MipMapLevelOfDetailBias = 16,
        };
        GraphicsDevice.SamplerStates[0] = samplerState;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        var rasterizerState = new RasterizerState {
            // FillMode = FillMode.WireFrame
        };
        GraphicsDevice.RasterizerState = rasterizerState;

        GraphicsDevice.Clear(Color.CornflowerBlue);

        effect!.Parameters["View"].SetValue(camera!.View.ToXnaMatrix4());

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
        batch.DrawString(font, $"{camera.GetCoordDirection()}", new(10, 50), Color.White);
        batch.DrawString(font, $"{camera.Rotation}", new(10, 70), Color.White);
        
        var x = Width / 2 - 9;
        var y = Height / 2 - 9;
        
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        
        batch.Draw(_crosshair, new Rectangle(x, y, 24, 24), Color.White);
        
        batch.End();

        GraphicsDevice.Viewport = originalViewport;

        base.Draw(gameTime);
    }
}
*/
