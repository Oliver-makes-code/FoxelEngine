using Common.Util.Serialization;

namespace Common.Network.Packets;

public abstract class Packet : VSerializable {

    public Packet() {

    }
    
    public abstract void Write(VDataWriter writer);
    public abstract void Read(VDataReader reader);


    public virtual void OnReturnToPool() {
        
    }
}
