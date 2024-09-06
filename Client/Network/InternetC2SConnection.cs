using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using Foxel.Common.Network.Packets;
using Foxel.Common.Network.Packets.C2S.Handshake;
using Foxel.Common.Network.Packets.S2C;
using Foxel.Core;
using LiteNetLib.Utils;
using Foxel.Common.Network.Serialization;
using Foxel.Common.World.Content;

namespace Foxel.Client.Network;

public class InternetC2SConnection : C2SConnection, INetEventListener {

    private readonly NetManager NetClient;

    private readonly NetDataWriter NetWriter = new(autoResize: true, initialSize: 256);

    private NetPeer? peer;
    private bool synced = false;

    public InternetC2SConnection(string address, int port = 24564) {
        NetClient = new(this);

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
        
        var codec = toSend.GetCodec();
        int id = ContentStores.PacketCodecs.GetId(codec);
        var key = ContentStores.PacketCodecs.GetKey(id);
        VoxelClient.Logger.Debug($"Sending packet {key} to server.");

        NetWriter.Reset();
        var packetWriter = new PacketDataWriter(NetWriter);
        packetWriter.Primitive().Int(id);
        codec.WriteGeneric(packetWriter, toSend);
        peer.Send(NetWriter, DeliveryMethod.ReliableOrdered);

        //Console.WriteLine($"Sending {writer.currentBytes.Length} bytes to server for packet {toSend}");
    }

    public void OnPeerConnected(NetPeer peer) {
        this.peer = peer;

        Game.Logger.Info("Client Connected!");
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader nReader, byte channelNumber, DeliveryMethod deliveryMethod) {
        if (packetHandler == null)
            return;
        
        var packetReader = new PacketDataReader(nReader);

        if (!synced) {
            _ = packetReader.Primitive().Byte();
            synced = true;
            Game.Logger.Info("S2C Map Synced");

            //After maps have been synced, client handshake is done.
            DeliverPacket(new HandshakeDoneC2SPacket());
            return;
        }

        int id = packetReader.Primitive().Int();
        var codec = ContentStores.PacketCodecs.GetValue(id);
        var key = ContentStores.PacketCodecs.GetKey(id);
        VoxelClient.Logger.Debug($"Recieved packet {key} from server.");

        var packet = codec.ReadGeneric(packetReader);

        packetHandler.HandlePacket((S2CPacket)packet);
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
