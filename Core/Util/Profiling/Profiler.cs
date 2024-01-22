using System.Collections.Concurrent;
using GlmSharp;

namespace Voxel.Common.Util.Profiling;

public static class Profiler {

    private static uint lastEntryID = 0;
    private static ThreadLocal<ProfilerState> State = new ThreadLocal<ProfilerState>();
    private static ConcurrentDictionary<string, ProfilerState> StatesByName = new ConcurrentDictionary<string, ProfilerState>();

    public static ProfilerKey GetProfilerKey(string name) => GetProfilerKey(name, vec3.Zero);

    public static ProfilerKey GetProfilerKey(string name, vec3 color) {
        return new ProfilerKey(lastEntryID++, name, color);
    }

    public static void Init(string profileName) {
        if (!StatesByName.TryGetValue(profileName, out var state))
            StatesByName[profileName] = state = new ProfilerState(profileName);

        State.Value = state;
    }

    public static void Push(ProfilerKey key, string? meta) {
        var time = DateTime.Now;

        if (!State.IsValueCreated)
            throw new InvalidOperationException("Profiler state not created");

        State.Value.Push(key, time, meta);
    }

    public static void Pop(ProfilerKey key) {
        var time = DateTime.Now;

        if (!State.IsValueCreated)
            throw new InvalidOperationException("Profiler state not created");

        State.Value.Pop(key, time);
    }


    public static void GetStateNames(List<string> target) {
        target.Clear();
        target.AddRange(StatesByName.Keys);
    }
    public static void GetStateEntries(string threadName, List<ProfilerEntry> target) {
        target.Clear();

        if (!StatesByName.TryGetValue(threadName, out var state))
            return;

        state.GrabEntries(target);
    }

    private class ProfilerState {
        public readonly string ProfileName;
        private readonly Stack<ProfilerEntry> entryStack = new Stack<ProfilerEntry>();
        private readonly object entryLock = new();

        private readonly List<ProfilerEntry> lastFullEntries = new List<ProfilerEntry>();
        private readonly List<ProfilerEntry> completeEntries = new List<ProfilerEntry>();

        public ProfilerState(string profileName) {
            ProfileName = profileName;
        }

        public void Push(ProfilerKey key, DateTime time, string? meta) {
            var entry = new ProfilerEntry(key, entryStack.Count, meta);
            entry.StartTime = time;
            entryStack.Push(entry);
        }

        public void Pop(ProfilerKey key, DateTime time) {
            var top = entryStack.Pop();

            if (top.Key != key)
                throw new InvalidOperationException("Did not pop profiler off stack!");

            top.EndTime = time;
            completeEntries.Add(top);

            if (entryStack.Count == 0) {
                lock (entryLock) {
                    //Copy into last full entries list.
                    lastFullEntries.Clear();
                    lastFullEntries.AddRange(completeEntries);

                    //Clear in-progress entries.
                    completeEntries.Clear();
                }
            }
        }

        public void GrabEntries(List<ProfilerEntry> target) {
            lock (entryLock) {
                target.Clear();
                target.AddRange(lastFullEntries);
            }
        }
    }

    public class ProfilerEntry {
        public readonly ProfilerKey Key;
        public int Level;
        public DateTime StartTime;
        public DateTime EndTime;

        public string? Meta;

        public ProfilerEntry(ProfilerKey key, int level, string? meta) {
            Key = key;
            Level = level;
            Meta = meta;
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

        public IDisposable Push(string? meta = null) {
            Profiler.Push(this, meta);
            return this;
        }

        public void Dispose() {
            Profiler.Pop(this);
        }
    }
}
