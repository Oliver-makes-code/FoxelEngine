using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using Voxel.Common.Content;
using Voxel.Common.Network.Packets;
using Voxel.Common.Network.Packets.C2S;
using Voxel.Common.Network.Packets.C2S.Handshake;
using Voxel.Common.Network.Packets.S2C;
using Voxel.Common.Network.Packets.Utils;
using Voxel.Common.Util.Registration;
using Voxel.Common.Util.Serialization.Compressed;

namespace Voxel.Client.Network;

public class InternetC2SConnection : C2SConnection, INetEventListener {

    private readonly NetManager NetClient;
    private NetPeer? peer;

    private readonly CompressedVDataReader Reader = new();
    private readonly CompressedVDataWriter Writer = new();

    public Registries Registries => ContentDatabase.Instance.Registries;

    private bool synced = false;

    public InternetC2SConnection(string address, int port = 24564) {
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

        if (!Registries.PacketTypes.TypeToRaw(toSend.GetType(), out var rawID))
            throw new InvalidOperationException($"Cannot send unknown packet {toSend}");

        writer.Write(rawID);
        writer.Write(toSend);
        peer.Send(writer.currentBytes, 0, DeliveryMethod.ReliableOrdered);

        //Console.WriteLine($"Sending {writer.currentBytes.Length} bytes to server for packet {toSend}");
    }

    public void OnPeerConnected(NetPeer peer) {
        this.peer = peer;

        Console.Out.WriteLine("Client: Connected!");
    }

    public void OnNetworkReceive(NetPeer _, NetPacketReader nReader, byte channelNumber, DeliveryMethod deliveryMethod) {
        if (packetHandler == null)
            return;

        Reader.LoadData(nReader.RawData.AsSpan(nReader.UserDataOffset, nReader.UserDataSize));

        if (!synced) {
            Registries.ReadSync(Reader);
            synced = true;
            Console.Out.WriteLine("Client: S2C Map Synced");

            //After maps have been synced, client handshake is done.
            DeliverPacket(new C2SHandshakeDone());
            return;
        }

        var rawID = Reader.ReadUInt();
        if (!Registries.PacketTypes.RawToType(rawID, out var packetType))
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
