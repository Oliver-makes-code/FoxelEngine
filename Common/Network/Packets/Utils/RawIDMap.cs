using System.Diagnostics.CodeAnalysis;
using Common.Util.Serialization;

namespace Common.Network.Packets.Utils;

/// <summary>
/// Maps a RawID to some type and back.
///
/// Needs to be synced before use.
/// Either map can be shared with other maps if need be.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class RawIDMap<T> where T : notnull {
    public Dictionary<T, uint> outgoingMap = new();
    public Dictionary<uint, T> incomingMap = new();

    public bool TryConvertIncoming(uint raw, [NotNullWhen(true)] out T? result) => incomingMap.TryGetValue(raw, out result);
    public bool TryConvertOutgoing(T instance, out uint result) => outgoingMap.TryGetValue(instance, out result);


    public abstract void WriteOutgoingMap(VDataWriter writer);
    public abstract void ReadIncomingMap(VDataReader reader);
}
