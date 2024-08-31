using Foxel.Common.Util.Serialization;

namespace Foxel.Common.Network.Packets;

public abstract class Packet : VSerializable {

    public Packet() {

    }

    public abstract void Write(VDataWriter writer);
    public abstract void Read(VDataReader reader);


    public virtual void OnReturnToPool() {

    }
}
