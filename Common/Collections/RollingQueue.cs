using System.Collections;

namespace Voxel.Common.Collections;

public class RollingQueue<T> : IEnumerable<List<T>>, IEnumerable {
    private readonly Queue<List<T>> InternalHolder = [];

    public void Shift(List<T> input) {
        if (InternalHolder.Count == 0)
            return;
        var output = InternalHolder.Dequeue();
        input.AddRange(output);
        output.Clear();
        InternalHolder.Enqueue(output);
    }

    public void Append(int index, T value)
        => CreateIfEmpty(index).Add(value);
    
    public void AppendEach(int index, IEnumerable<T> values) {
        var list = CreateIfEmpty(index);
        list.AddRange(values);
    }

    public IEnumerator<List<T>> GetEnumerator()
        => InternalHolder.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private List<T> CreateIfEmpty(int index) {
        while (InternalHolder.Count <= index)
            InternalHolder.Enqueue([]);
        
        return InternalHolder.ElementAt(index);
    }
}
