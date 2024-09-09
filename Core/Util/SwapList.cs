using System.Collections;
using System.Numerics;

namespace Foxel.Core.Util;

/// <summary>
/// Unordered list-like collection that fills in spaces on the list to prevent large copies.
/// </summary>
public class SwapList<T> : IList<T>, IReadOnlyList<T> {
    public int Count { get; private set; }
    public bool IsReadOnly => false;

    private T[] data;

    public T this[int index] {
        get => data[index];
        set => data[index] = value;
    }

    public SwapList(int capacity = 16) {
        data = new T[capacity];
    }

    public IEnumerator<T> GetEnumerator() {
        for (int i = 0; i < Count; i++)
            yield return data[i];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(T item) {
        ExpandIfNeeded(1);

        data[Count - 1] = item;
        Count++;
    }

    public bool Remove(T item) {
        int index = Array.IndexOf(data, item);

        if (index == -1)
            return false;
        RemoveAt(index);
        return true;
    }

    public bool Contains(T item) => Array.IndexOf(data, item) != -1;
    public void CopyTo(T[] array, int arrayIndex) => data.CopyTo(array, arrayIndex);

    public void Clear() {
        Array.Fill(data, default);
        Count = 0;
    }
    public int IndexOf(T item) => Array.IndexOf(data, item);
    public void Insert(int index, T item) => throw new NotImplementedException(); //tbh just can't be bothered to implement
    
    
    public void RemoveAt(int index) {
        if (Count - 1 == index) {
            //If element is last element, simply remove it.
            data[index] = default!;
        } else {
            //If element is not last element, put the last element in its place.
            data[index] = data[Count - 1];
            data[Count - 1] = default!;
        }

        Count--;
    }

    private void ExpandIfNeeded(int by) {
        int newCount = Count + by;

        if (newCount >= data.Length) {
            var oldData = data;
            var newData = new T[BitOperations.RoundUpToPowerOf2((uint)data.Length + 1)];

            oldData.CopyTo(newData.AsSpan());
            data = newData;
        }
    }
}
