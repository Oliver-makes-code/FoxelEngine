namespace Common.Network.Packets;

public interface Packet {
    public void Write();
    public void Read();
}
