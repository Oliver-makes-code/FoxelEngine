using System.Collections.Concurrent;
using GlmSharp;

namespace Foxel.Core.Util.Profiling;

public static class Profiler {
    private readonly static ThreadLocal<ProfilerState> State = new ThreadLocal<ProfilerState>();
    private readonly static ConcurrentDictionary<string, ProfilerState> StatesByName = new ConcurrentDictionary<string, ProfilerState>();

    private static uint lastEntryID = 0;

    public static ProfilerKey GetProfilerKey(string name) => GetProfilerKey(name, vec3.Zero);

    public static ProfilerKey GetProfilerKey(string name, vec3 color) {
        return new ProfilerKey(lastEntryID++, name, color);
    }

    public static void Init(string profileName) {
        if (!StatesByName.TryGetValue(profileName, out var state))
            StatesByName[profileName] = state = new(profileName);

        State.Value = state;
    }

    public static void Push(ProfilerKey key, string? meta) {
        var time = DateTime.Now;

        if (!State.IsValueCreated)
            throw new InvalidOperationException("Profiler state not created");

        State.Value!.Push(key, time, meta);
    }

    public static void Pop(ProfilerKey key) {
        var time = DateTime.Now;

        if (!State.IsValueCreated)
            throw new InvalidOperationException("Profiler state not created");

        State.Value!.Pop(key, time);
    }

    public static void SetCurrentMeta(string? value) {
        if (!State.IsValueCreated)
            throw new InvalidOperationException("Profiler state not created");

        State.Value!.SetCurrentMeta(value);
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

    public class ProfilerEntry {
        public readonly ProfilerKey Key;
        public int level;
        public DateTime startTime;
        public DateTime endTime;

        public string? meta;

        public ProfilerEntry(ProfilerKey key, int level, string? meta) {
            this.Key = key;
            this.level = level;
            this.meta = meta;
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

    private class ProfilerState {
        public readonly string ProfileName;
        private readonly Stack<ProfilerEntry> EntryStack = new();
        private readonly object EntryLock = new();

        private readonly List<ProfilerEntry> LastFullEntries = [];
        private readonly List<ProfilerEntry> CompleteEntries = [];

        public ProfilerState(string profileName) {
            ProfileName = profileName;
        }

        public void Push(ProfilerKey key, DateTime time, string? meta) {
            var entry = new ProfilerEntry(key, EntryStack.Count, meta);
            entry.startTime = time;
            EntryStack.Push(entry);
        }

        public void SetCurrentMeta(string? meta) {
            var entry = EntryStack.Peek();
            entry.meta = meta;
        }

        public void Pop(ProfilerKey key, DateTime time) {
            var top = EntryStack.Pop();

            if (top.Key != key)
                throw new InvalidOperationException("Did not pop profiler off stack!");

            top.endTime = time;
            CompleteEntries.Add(top);

            if (EntryStack.Count == 0) {
                lock (EntryLock) {
                    //Copy into last full entries list.
                    LastFullEntries.Clear();
                    LastFullEntries.AddRange(CompleteEntries);

                    //Clear in-progress entries.
                    CompleteEntries.Clear();
                }
            }
        }

        public void GrabEntries(List<ProfilerEntry> target) {
            lock (EntryLock) {
                target.Clear();
                target.AddRange(LastFullEntries);
            }
        }
    }
}
