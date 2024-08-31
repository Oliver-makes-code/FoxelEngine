using Foxel.Common.Util.Serialization;

namespace Foxel.Common.Util.Registration;

public interface BaseRegistry {

    public void GenerateIds();

    public void Write(VDataWriter writer);
    public void Read(VDataReader reader);

    public void Clear();
}
