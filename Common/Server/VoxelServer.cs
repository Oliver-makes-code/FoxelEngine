using NLog;
using Foxel.Common.Server.Components;
using Foxel.Common.Server.Components.Networking;
using Foxel.Common.Util;
using Foxel.Common.World.Content;
using Foxel.Core.Assets;
using Foxel.Core.Util.Profiling;

namespace Foxel.Common.Server;

/// <summary>
/// A logical server for the game, used for both internal and external servers.
/// </summary>
public class VoxelServer {
    public static readonly Logger Logger = LogManager.GetLogger("Server");

    public static readonly PackManager PackManager = new(AssetType.Content, Logger);

    public static readonly ItemContentManager ItemContentManager = new();

    public static readonly BlockContentManager BlockContentManager = new();

    private static readonly Profiler.ProfilerKey TickKey = Profiler.GetProfilerKey("Tick");

    public readonly PlayerManager PlayerManager;
    public readonly WorldManager WorldManager;
    public readonly ConnectionManager ConnectionManager;
    public readonly LNLHostManager InternetHostManager;

    public readonly string ProfilerName;

    private readonly List<ServerComponent> Components = [];
    public bool isRunning { get; private set; }

    private Thread? serverThread;

    public VoxelServer(string profilerName) {
        PlayerManager = AddComponent(new PlayerManager(this));
        WorldManager = AddComponent(new WorldManager(this));
        ConnectionManager = AddComponent(new ConnectionManager(this));
        InternetHostManager = AddComponent(new LNLHostManager(this));

        ProfilerName = profilerName;
    }

    public virtual async Task Start() {
        if (isRunning)
            return;
        isRunning = true;

        await PackManager.ReloadPacks();

        serverThread = new(ServerLoop) {
            IsBackground = true
        };
        serverThread.Start();
    }

    public virtual void Stop(bool waitOnThread = false) {
        if (!isRunning)
            return;
        isRunning = false;

        if (waitOnThread)
            serverThread?.Join();
    }

    private void ServerLoop() {
        try {
            Profiler.Init(ProfilerName);

            //Tell all components the server is starting.
            foreach (var component in Components)
                component.OnServerStart();

            var lastUpdateTime = DateTime.Now;

            while (isRunning) {
                //Check if enough time has passed for a tick
                //TODO: Check if extra time should roll over? Current method may lead to inconsistent tick rates
                var now = DateTime.Now;
                double delta = (now - lastUpdateTime).TotalMilliseconds;
                if (delta < Constants.SecondsPerTick)
                    Thread.Sleep((int)(Constants.SecondsPerTick * 1000));

                lastUpdateTime = now;
                Tick();
            }
        } catch (Exception e) {
            Logger.Error(e);
        } finally {
            //Tell all components the server is stopping.
            foreach (var component in Components) {

                //Each component is given the stop command independently so that one failing won't fail them all.
                try {
                    component.OnServerStop();
                } catch (Exception e) {
                    Logger.Error(e);
                }
            }
        }

        isRunning = false;
    }

    protected virtual void Tick() {
        using (TickKey.Push()) {
            //Tick all components
            foreach (var component in Components)
                component.Tick();
        }
    }

    protected T AddComponent<T>(T toAdd) where T : ServerComponent {
        Components.Add(toAdd);
        return toAdd;
    }
}
