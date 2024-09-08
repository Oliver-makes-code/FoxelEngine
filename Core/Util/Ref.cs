namespace Foxel.Core.Util;

public readonly record struct Ref<T>(nint Handle) where T : struct {
    public unsafe T *Ptr => this;

    public unsafe Ref(T *handle) : this((nint)handle) {}

    public static unsafe implicit operator T *(Ref<T> _ref)
        => (T *)_ref.Handle;

    public static unsafe implicit operator Ref<T>(T *ptr)
        => new(ptr);
}
