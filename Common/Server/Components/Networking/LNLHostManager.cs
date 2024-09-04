using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Foxel.Common.Content;
using Foxel.Common.Network;
using Foxel.Common.Network.Packets;
using Foxel.Common.Network.Packets.C2S;
using Foxel.Common.Network.Packets.S2C;
using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Util.Registration;
using Foxel.Common.Util.Serialization.Compressed;
using Foxel.Common.World.Content;
using Foxel.Common.Network.Serialization;

namespace Foxel.Common.Server.Components.Networking;

/// <summary>
/// Server component that opens this server up to being connected to over the internet.
///
/// Uses LiteNetLib for its underlying network structure.
/// </summary>
public class LNLHostManager : ServerComponent, INetEventListener {

    private NetManager? lnlServer;

    private readonly Dictionary<int, LNLS2CConnection> ActiveConnections = new();
    private readonly Queue<int> DeadConnections = new();

    private readonly CompressedVDataWriter Writer = new();
    private readonly NetDataWriter NetWriter = new(autoResize: true, initialSize: 256);

    public LNLHostManager(VoxelServer server) : base(server) {}

    public override void OnServerStart() {}
    public override void Tick() {
        if (lnlServer == null)
            return;

        while (DeadConnections.TryDequeue(out var id))
            ActiveConnections.Remove(id);

        lnlServer.PollEvents();
    }
    public override void OnServerStop() {
        Close();
    }

    /// <summary>
    /// Opens the server to be connected to on a given port.
    /// </summary>
    public void Open(int port = 24564) {
        if (lnlServer != null)
            throw new InvalidOperationException("Server is already open");

        lnlServer = new(this);
        lnlServer.Start(port);

        VoxelServer.Logger.Info($"Hosting server on port {port}");
    }

    public void Close() {
        if (lnlServer == null)
            return;

        VoxelServer.Logger.Info("Closing server");
        lnlServer.Stop(true);
    }

    public void OnConnectionRequest(ConnectionRequest request) {
        var peer = request.Accept();
        var connection = new LNLS2CConnection(this, peer);

        ActiveConnections[peer.Id] = connection;
        Server.ConnectionManager.AddConnection(connection);

        VoxelServer.Logger.Info($"Accepting connection from {request.RemoteEndPoint}");
    }

    public void OnPeerConnected(NetPeer peer) {}
    
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod) {
        if (!ActiveConnections.TryGetValue(peer.Id, out var activeConnection))
            return;

        activeConnection.HandleNetReceive(reader);
    }
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {

        VoxelServer.Logger.Info($"Peer {peer.Id} disconnected!");

        //Ignore the disconnect if it's because the server wanted to disconnect
        if (disconnectInfo.Reason == DisconnectReason.DisconnectPeerCalled)
            return;

        //Try to find matching active connection
        if (!ActiveConnections.TryGetValue(peer.Id, out var connection))
            return;

        //If it exists, close it.
        connection.Close();
    }


    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) {

    }
    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) {

    }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) {

    }

    public class LNLS2CConnection : S2CConnection {
        private readonly LNLHostManager Manager;
        private readonly NetPeer Peer;

        private Registries Registries => ContentDatabase.Instance.Registries;

        public LNLS2CConnection(LNLHostManager manager, NetPeer peer) {
            Manager = manager;
            Peer = peer;

            OnClosed += () => {
                //Disconnect the peer if it was connected.
                if (peer.ConnectionState != ConnectionState.Disconnected)
                    peer.Disconnect();

                //Remove from list of connections.
                manager.ActiveConnections.Remove(peer.Id);
            };

            //SYNC MAPS HERE
            var writer = manager.Writer;
            writer.Reset();

            Registries.WriteSync(writer);

            peer.Send(writer.currentBytes, 0, DeliveryMethod.ReliableOrdered);
            
            VoxelServer.Logger.Info("Server sending sync packet");
        }

        public override void DeliverPacket(Packet toSend) {
            var codec = toSend.GetCodec();
            int id = ContentStores.PacketCodecs.GetId(codec);
            var key = ContentStores.PacketCodecs.GetKey(id);
            VoxelServer.Logger.Debug($"Sending packet {key} to client.");
            
            Manager.NetWriter.Reset();
            var packetWriter = new PacketDataWriter(Manager.NetWriter);
            packetWriter.Primitive().Int(id);
            codec.WriteGeneric(packetWriter, toSend);

            Peer.Send(Manager.NetWriter, DeliveryMethod.ReliableOrdered);
        }

        public void HandleNetReceive(NetDataReader nReader) {

            //Console.WriteLine($"Got {nReader.UserDataSize} bytes from client");

            if (packetHandler == null)
                return;

            var packetReader = new PacketDataReader(nReader);

            int id = packetReader.Primitive().Int();
            var codec = ContentStores.PacketCodecs.GetValue(id);
            var key = ContentStores.PacketCodecs.GetKey(id);
            VoxelServer.Logger.Debug($"Recieved packet {key} from client.");

            var packet = codec.ReadGeneric(packetReader);

            packetHandler.HandlePacket((C2SPacket)packet);
        }
    }
}
