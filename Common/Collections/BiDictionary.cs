namespace Voxel.Common.Collections; 

public class BiDictionary<TKey, TValue> : Dictionary<TKey, Dictionary<TKey, TValue>> where TKey : notnull {
    public TValue this[TKey a, TKey b] {
        get => this[a][b];
        set {
            if (!ContainsKey(a))
                this[a] = new();
            this[a][b] = value;
        }
    }

    public bool ContainsKey(TKey a, TKey b) {
        return ContainsKey(a) && this[a].ContainsKey(b);
    }
}
