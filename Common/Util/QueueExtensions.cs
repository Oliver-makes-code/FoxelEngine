namespace Voxel.Common.Util;

public static class QueueExtensions {
    public static void Add<T>(this Queue<T> queue, T value)
        => queue.Enqueue(value);
    public static T Remove<T>(this Queue<T> queue)
        => queue.Dequeue();
}
