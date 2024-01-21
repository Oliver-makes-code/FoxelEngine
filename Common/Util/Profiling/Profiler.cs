using GlmSharp;

namespace Voxel.Common.Util.Profiling;

public static class Profiler {

    private static uint lastEntryID = 0;
    private static ThreadLocal<ProfilerState> State = new ThreadLocal<ProfilerState>();


    public static ProfilerKey GetProfilerKey(string name) => GetProfilerKey(name, vec3.Zero);

    public static ProfilerKey GetProfilerKey(string name, vec3 color) {
        return new ProfilerKey(lastEntryID++, name, color);
    }

    public static void Push(ProfilerKey key) {
        var time = DateTime.Now;

        if (!State.IsValueCreated)
            State.Value = new ProfilerState();

        State.Value.Push(key, time);
    }

    public static void Pop(ProfilerKey key) {
        var time = DateTime.Now;

        if (!State.IsValueCreated)
            throw new InvalidOperationException("Profiler state not created");

        State.Value.Pop(key, time);
    }

    private class ProfilerState {
        private readonly Stack<ProfilerEntry> entryStack = new Stack<ProfilerEntry>();
        private readonly List<ProfilerEntry> completeEntries = new List<ProfilerEntry>();

        public void Push(ProfilerKey key, DateTime time) {
            var entry = new ProfilerEntry(key, entryStack.Count);
            entry.StartTime = time;
            entryStack.Push(entry);
        }

        public void Pop(ProfilerKey key, DateTime time) {
            var top = entryStack.Pop();

            if (top.Key != key)
                throw new InvalidOperationException("Did not pop profiler off stack!");
        }
    }

    public class ProfilerEntry {
        public readonly ProfilerKey Key;
        public int Level;
        public DateTime StartTime;
        public DateTime EndTime;

        public ProfilerEntry(ProfilerKey key, int level) {
            Key = key;
            Level = level;
        }
    }

    public class ProfilerKey : IDisposable {
        public readonly uint Id;
        public readonly string Name;
        public readonly vec3 Color;

        public ProfilerKey(uint id, string name, vec3 color) {
            Id = id;
            Name = name;
            Color = color;
        }

        public IDisposable Push() {
            Profiler.Push(this);
            return this;
        }

        public void Dispose() {
            Profiler.Pop(this);
        }
    }
}
