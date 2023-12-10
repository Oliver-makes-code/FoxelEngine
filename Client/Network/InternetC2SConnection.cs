using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using Voxel.Common.Network.Packets;
using Voxel.Common.Network.Packets.C2S;
using Voxel.Common.Network.Packets.C2S.Handshake;
using Voxel.Common.Network.Packets.S2C;
using Voxel.Common.Network.Packets.Utils;
using Voxel.Common.Util.Serialization.Compressed;

namespace Voxel.Client.Network;

public class InternetC2SConnection : C2SConnection, INetEventListener {

    private readonly NetManager NetClient;
    private NetPeer? peer;

    private readonly PacketMap<C2SPacket> PacketMap = new();
    private readonly CompressedVDataReader Reader = new();
    private readonly CompressedVDataWriter Writer = new();

    private bool synced = false;

    public InternetC2SConnection(string address, int port = 24564) {
        PacketMap.FillOutgoingMap();
        NetClient = new NetManager(this);

        OnClosed += () => {
            //Disconnect the peer if it was connected.
            if (peer != null && peer.ConnectionState != ConnectionState.Disconnected) {
                peer.Disconnect();
                peer = null;
                NetClient.Stop(true);
            }
        };


        NetClient.Start();
        NetClient.Connect(address, port, string.Empty);
    }

    public override void Tick() {
        NetClient.PollEvents();
    }

    public override void DeliverPacket(Packet toSend) {
        if (peer == null)
            throw new InvalidOperationException("Cannot send packet on disconnected line");

        var writer = Writer;
        writer.Reset();

        if (!PacketMap.outgoingMap.TryGetValue(toSend.GetType(), out var rawID))
            throw new InvalidOperationException($"Cannot send unknown packet {toSend}");

        writer.Write(rawID);
        writer.Write(toSend);
        peer.Send(writer.currentBytes, 0, DeliveryMethod.ReliableOrdered);

        //Console.WriteLine($"Sending {writer.currentBytes.Length} bytes to server");
    }

    public void OnPeerConnected(NetPeer peer) {
        this.peer = peer;

        Console.Out.WriteLine("Client: Connected!");

        //SYNC MAPS HERE
        var writer = Writer;
        writer.Reset();

        PacketMap.WriteOutgoingMap(writer);
        peer.Send(writer.currentBytes, 0, DeliveryMethod.ReliableOrdered);

        //After maps have been synced, client handshake is done.
        DeliverPacket(new C2SHandshakeDone());
    }

    public void OnNetworkReceive(NetPeer _, NetPacketReader nReader, byte channelNumber, DeliveryMethod deliveryMethod) {
        if (packetHandler == null)
            return;

        Reader.LoadData(nReader.RawData.AsSpan(nReader.UserDataOffset, nReader.UserDataSize));

        if (!synced) {
            PacketMap.ReadIncomingMap(Reader);
            synced = true;
            //Console.Out.WriteLine("Client: S2C Map Synced");
            return;
        }

        var rawID = Reader.ReadUint();
        if (!PacketMap.incomingMap.TryGetValue(rawID, out var packetType))
            return;

        //Console.WriteLine($"Got packet {packetType.Name} from server");

        var packet = PacketPool.GetPacket<S2CPacket>(packetType);
        packet.Read(Reader);

        packetHandler.HandlePacket(packet);
    }

    public void OnPeerDisconnected(NetPeer _, DisconnectInfo disconnectInfo) {

    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) {

    }


    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) {

    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) {

    }

    //UNUSED
    public void OnConnectionRequest(ConnectionRequest request) {

    }
}
