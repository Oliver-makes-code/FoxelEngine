using System;
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

    SpriteBatch? batch;

    SpriteFont? font;

    Camera? camera;

    Effect? effect;

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

    public VoxelClient() {
        Instance = this;

        _graphics = new(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.PreferMultiSampling = true;
        _graphics.PreparingDeviceSettings += (_, args) => {
            args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 2;
        };

        Run();
    }

    public void RedrawChunk() {
        world!.buildQueue.Add(new(0, 0, 0));
        world.buildQueue.Add(new(1, 0, 0));
    }

    protected override void Initialize() {
        base.Initialize();

        batch = new(GraphicsDevice);

        font = Content.Load<SpriteFont>("Arial_Font");

        world = new(new(), GraphicsDevice);

        ClientConfig.Load();
        ClientConfig.Save();

        for (int x = 0; x < 8; x++) {
            for (int z = 0; z < 8; z++) {
                world.world.Load(new(x, 0, z));
                world.world[new(x, 0, z)]!.FillWithRandomData();
            }
        }

        effect = Content.Load<Effect>("Main_Eff");

        camera = new(AspectRatio);

        effect.Parameters["Projection"].SetValue(camera.Projection);
        effect.Parameters["World"].SetValue(camera.World);

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

        world!.BuildOneChunk();
        world.UnloadChunks();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        GraphicsDevice.Clear(Color.CornflowerBlue);

        effect!.Parameters["View"].SetValue(camera!.View);

        world!.Draw(effect);

        var fps = 1000f / (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        var originalViewport = GraphicsDevice.Viewport;
        GraphicsDevice.Viewport = new Viewport(0, 0, Width, Height);

        batch!.Begin();
        batch.DrawString(font, $"{fps}", new(10, 10), Color.White);
        batch.End();

        GraphicsDevice.Viewport = originalViewport;

        base.Draw(gameTime);
    }
}
