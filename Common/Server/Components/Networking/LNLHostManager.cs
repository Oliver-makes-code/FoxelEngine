using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Voxel.Common.Content;
using Voxel.Common.Network;
using Voxel.Common.Network.Packets;
using Voxel.Common.Network.Packets.C2S;
using Voxel.Common.Network.Packets.S2C;
using Voxel.Common.Network.Packets.Utils;
using Voxel.Common.Util.Registration;
using Voxel.Common.Util.Serialization.Compressed;

namespace Voxel.Common.Server.Components.Networking;

/// <summary>
/// Server component that opens this server up to being connected to over the internet.
///
/// Uses LiteNetLib for its underlying network structure.
/// </summary>
public class LNLHostManager : ServerComponent, INetEventListener {

    private NetManager? lnlServer;

    private readonly Dictionary<int, LNLS2CConnection> ActiveConnections = new();
    private readonly Queue<int> DeadConnections = new();

    private readonly CompressedVDataReader Reader = new();
    private readonly CompressedVDataWriter Writer = new();

    public LNLHostManager(VoxelServer server) : base(server) {

    }

    public override void OnServerStart() {

    }
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

        lnlServer = new NetManager(this);
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

    public void OnPeerConnected(NetPeer peer) {

    }
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
            var writer = Manager.Writer;
            writer.Reset();

            if (!Registries.PacketTypes.TypeToRaw(toSend.GetType(), out var rawID))
                return;

            writer.Write(rawID);
            writer.Write(toSend);
            Peer.Send(writer.currentBytes, 0, DeliveryMethod.ReliableOrdered);
        }

        public void HandleNetReceive(NetDataReader nReader) {

            var reader = Manager.Reader;
            reader.LoadData(nReader.RawData.AsSpan(nReader.UserDataOffset, nReader.UserDataSize));

            //Console.WriteLine($"Got {nReader.UserDataSize} bytes from client");

            if (packetHandler == null)
                return;

            var rawID = reader.ReadUInt();
            if (!Registries.PacketTypes.RawToType(rawID, out var packetType))
                return;

            //Console.WriteLine($"Got packet {packetType.Name} from client");

            var packet = PacketPool.GetPacket<C2SPacket>(packetType);
            packet.Read(reader);

            packetHandler.HandlePacket(packet);
        }
    }
}
