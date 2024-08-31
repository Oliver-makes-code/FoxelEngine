using System.Collections;

namespace Foxel.Core.Util;

public class DefferedList<T> : ICollection<T> {
    private readonly List<T> RemoveList = new List<T>();
    private readonly List<T> Tickables = new List<T>();
    private readonly List<T> AddList = new List<T>();

    public int Count => Tickables.Count;
    public bool IsReadOnly => false;
    
    public void UpdateCollection() {
        Tickables.AddRange(AddList);
        foreach (var t in RemoveList) Tickables.Remove(t);

        AddList.Clear();
        RemoveList.Clear();
    }

    public IEnumerator<T> GetEnumerator() => Tickables.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Tickables.GetEnumerator();
    public void Add(T item) => AddList.Add(item);
    public void Clear() {
        RemoveList.Clear();
        AddList.Clear();
        Tickables.Clear();
    }
    public bool Contains(T item) => Tickables.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => Tickables.CopyTo(array, arrayIndex);
    public bool Remove(T item) {
        RemoveList.Add(item);
        return true;
    }

}
