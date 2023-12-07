namespace Common.Util.Serialization;

public interface VSerializable {


    public void Write(VDataWriter writer);
    public void Read(VDataReader reader);
}
