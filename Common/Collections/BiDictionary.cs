using System.Diagnostics.CodeAnalysis;

namespace Foxel.Common.Collections;

class BiDictionary<TLeft, TRight> where TLeft : notnull where TRight : notnull {
    private readonly Dictionary<TLeft, TRight> LeftToRight = [];
    private readonly Dictionary<TRight, TLeft> RightToLeft = [];

    public void Put(TLeft left, TRight right) {
        if (LeftToRight.TryGetValue(left, out var r))
            RightToLeft.Remove(r);
        if (RightToLeft.TryGetValue(right, out var l))
            LeftToRight.Remove(l);
        LeftToRight[left] = right;
        RightToLeft[right] = left;
    }

    public TRight GetRight(TLeft left)
        => LeftToRight[left];

    public bool TryGetRight(TLeft left, [NotNullWhen(true)] out TRight? right)
        => LeftToRight.TryGetValue(left, out right);

    public IEnumerable<TLeft> Lefts()
        => LeftToRight.Keys;
    
    public TLeft GetLeft(TRight right)
        => RightToLeft[right];

    public bool TryGetLeft(TRight right, [NotNullWhen(true)] out TLeft? left)
        => RightToLeft.TryGetValue(right, out left);

    public IEnumerable<TRight> Rights()
        => RightToLeft.Keys;
}
