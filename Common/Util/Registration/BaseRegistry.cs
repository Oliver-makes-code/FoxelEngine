using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Util.Registration;

public interface BaseRegistry {

    public void GenerateIDs();

    public void Write(VDataWriter writer);
    public void Read(VDataReader reader);

    public void Clear();
}
