using NLog;
using Voxel.Common.Content;
using Voxel.Common.Server.Components;
using Voxel.Common.Server.Components.Networking;
using Voxel.Common.Util;
using Voxel.Common.Util.Profiling;

namespace Voxel.Common.Server;

/// <summary>
/// A logical server for the game, used for both internal and external servers.
/// </summary>
public class VoxelServer {
    public static readonly Logger Logger = LogManager.GetLogger("Server");


    private static Profiler.ProfilerKey TickKey = Profiler.GetProfilerKey("Tick");

    private readonly List<ServerComponent> Components = new();

    public readonly PlayerManager PlayerManager;
    public readonly WorldManager WorldManager;
    public readonly ConnectionManager ConnectionManager;
    public readonly LNLHostManager InternetHostManager;

    public readonly string ProfilerName;

    private Thread? serverThread;
    public bool isRunning { get; private set; }

    public VoxelServer(string profilerName) {
        PlayerManager = AddComponent(new PlayerManager(this));
        WorldManager = AddComponent(new WorldManager(this));
        ConnectionManager = AddComponent(new ConnectionManager(this));
        InternetHostManager = AddComponent(new LNLHostManager(this));

        ProfilerName = profilerName;
    }

    public virtual void Start() {
        if (isRunning)
            return;
        isRunning = true;

        serverThread = new Thread(ServerLoop);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    protected virtual void Tick() {
        using (TickKey.Push()) {
            //Tick all components
            foreach (var component in Components)
                component.Tick();
        }
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
                //TODO - Check if extra time should roll over? Current method may lead to inconsistent tick rates
                var now = DateTime.Now;
                var delta = (now - lastUpdateTime).TotalMilliseconds;
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

    protected T AddComponent<T>(T toAdd) where T : ServerComponent {
        Components.Add(toAdd);
        return toAdd;
    }
}
