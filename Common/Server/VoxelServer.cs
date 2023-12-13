using Voxel.Common.Content;
using Voxel.Common.Server.Components;
using Voxel.Common.Server.Components.Networking;
using Voxel.Common.Util;

namespace Voxel.Common.Server;

/// <summary>
/// A logical server for the game, used for both internal and external servers.
/// </summary>
public class VoxelServer {

    private readonly List<ServerComponent> Components = new();

    public readonly PlayerManager PlayerManager;
    public readonly WorldManager WorldManager;
    public readonly ConnectionManager ConnectionManager;
    public readonly LNLHostManager InternetHostManager;

    private Thread? serverThread;
    public bool isRunning { get; private set; }

    public VoxelServer() {
        PlayerManager = AddComponent(new PlayerManager(this));
        WorldManager = AddComponent(new WorldManager(this));
        ConnectionManager = AddComponent(new ConnectionManager(this));
        InternetHostManager = AddComponent(new LNLHostManager(this));
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

        //Tick all components
        foreach (var component in Components)
            component.Tick();
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
            Console.WriteLine(e);
        } finally {
            //Tell all components the server is stopping.
            foreach (var component in Components) {

                //Each component is given the stop command independently so that one failing won't fail them all.
                try {
                    component.OnServerStop();
                } catch (Exception e) {
                    Console.WriteLine(e);
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
