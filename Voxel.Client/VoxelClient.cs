using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering;
using Voxel.Client.World;

namespace Voxel.Client;

public class VoxelClient : Game {
    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static VoxelClient? Instance { get; private set; }

    private readonly GraphicsDeviceManager _graphics;

    Camera? camera;
    Effect? effect;

    ChunkMesh? chunkA;
    ChunkMesh? chunkB;

    readonly ClientWorld world = new(new());

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
        _graphics.PreferMultiSampling = true;
        _graphics.PreparingDeviceSettings += (a, args) => {
            args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 2;
        };

        Run();
    }

    public void RedrawChunk() {
        chunkA = new ChunkMesh(GraphicsDevice, world, new(0, 0, 0));
        chunkB = new ChunkMesh(GraphicsDevice, world, new(1, 0, 0));
    }

    protected override void Initialize() {
        base.Initialize();

        ClientConfig.Load();
        ClientConfig.Save();

        world.world.Load(new(0, 0, 0));
        world.world.Load(new(1, 0, 0));
        world.world[new(0, 0, 0)]!.FillWithRandomData();
        world.world[new(1, 0, 0)]!.FillWithRandomData();

        effect = Content.Load<Effect>("Main_Eff");

        camera = new(AspectRatio);

        effect.Parameters["Projection"].SetValue(camera.Projection);
        effect.Parameters["World"].SetValue(camera.World);

        RedrawChunk();

        Window.AllowUserResizing = true;

        Window.ClientSizeChanged += (_, _) => {
            camera!.UpdateProjection(AspectRatio);
            effect.Parameters["Projection"].SetValue(camera.Projection);
        };

        GamePad.InitDatabase();
    }

    protected override void Update(GameTime gameTime) {
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
            moveDir.Y += 0.1f;
        }
        if (Keybinds.crouch.isPressed) {
            moveDir.Y -= 0.1f;
        }
        if (Keybinds.lookRight.isPressed) {
            rotDir.X -= MathHelper.ToRadians(Keybinds.lookRight.strength);
        }
        if (Keybinds.lookLeft.isPressed) {
            rotDir.X += MathHelper.ToRadians(Keybinds.lookLeft.strength);
        }
        if (Keybinds.lookUp.isPressed) {
            rotDir.Y += Keybinds.lookUp.strength * 0.01f;
        }
        if (Keybinds.lookDown.isPressed) {
            rotDir.Y -= Keybinds.lookDown.strength * 0.01f;
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

        effect!.Parameters["View"].SetValue(camera!.View);

        chunkA!.Draw(GraphicsDevice, effect);
        chunkB!.Draw(GraphicsDevice, effect);

        base.Draw(gameTime);
    }
}
