using System.Collections;

namespace Voxel.Common.Collections;

public class RollingQueue<T> : IEnumerable<HashSet<T>>, IEnumerable {
    public HashSet<T> this[int index] {
        get => CreateIfEmpty(index);
        set => AppendEach(index, value);
    }

    private Dictionary<int, HashSet<T>> internalHolder = [];

    public HashSet<T> Shift() {
        var output = this[0];

        var next = new Dictionary<int, HashSet<T>>();

        foreach (var elem in internalHolder) {
            var newKey = elem.Key-1;
            if (newKey >= 0)
                next[newKey] = elem.Value;
        }

        internalHolder = next;

        return output;
    }

    public void Append(int index, T value)
        => this[index].Add(value);
    
    public void AppendEach(int index, HashSet<T> value)
        => this[index].UnionWith(value);

    public IEnumerator<HashSet<T>> GetEnumerator()
        => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private HashSet<T> CreateIfEmpty(int index) {
        if (!internalHolder.ContainsKey(index))
            internalHolder[index] = [];
        return internalHolder[index];
    }
}
