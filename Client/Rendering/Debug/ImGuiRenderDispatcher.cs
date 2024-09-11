using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GlmSharp;
using ImGuiNET;
using Foxel.Client.Rendering.World.Chunks;
using Foxel.Common.Util;
using Foxel.Core.Util.Profiling;

namespace Foxel.Client.Rendering.Debug;

//Organizes all of our ImGui rendering code into one class so it's not cluttering up the GameRenderer
public class ImGuiRenderDispatcher : Renderer {
    private readonly List<string> ProfilerStateNamesCache = new();
    private readonly List<Profiler.ProfilerEntry> ProfilerEntriesCache = new();
    private readonly Queue<Profiler.ProfilerEntry> ProfilerEntriesQueue = new();
    private readonly IntPtr ProfilerPointer = new IntPtr(GCHandle.ToIntPtr(GCHandle.Alloc(new object())));

    public ImGuiRenderDispatcher(VoxelClient client) : base(client) {}


    public override void Render(double delta) {
        if (VoxelClient.isMouseCapruted)
            return;
        DrawGeneralDebug();
        DrawProfiler();
        DrawInputDebug();
        ImGui.ShowMetricsWindow();
    }


    public override void Dispose() {}


    private void DrawGeneralDebug() {
        if (ImGui.Begin("General Debug")) {
            ImGui.Text($"Player Position: {(Client.playerEntity?.blockPosition ?? ivec3.Zero)}");
            ImGui.Text($"Player Velocity: {(Client.playerEntity?.velocity.WorldToBlockPosition() ?? ivec3.Zero)}");
            ImGui.Text($"Player Grounded: {Client.playerEntity?.isOnFloor ?? false}");

            ImGui.Text("");

            for (int i = 0; i < ChunkMeshBuilder.count; i++)
                ImGui.Text($"Chunk thread {i} active: {ChunkMeshBuilder.IsActive(i)}");
        }
        ImGui.End();
    }

    private void DrawProfiler() {
        nint labelID = ProfilerPointer;
        int level = 0;
        int indent = 0;

        nint GetID() {
            nint ptr = labelID;
            labelID++;
            return ptr;
        }

        //Draws the current top entry.
        void DrawEntry() {
            var topEntry = ProfilerEntriesQueue.Dequeue();
            string entryText = $"{topEntry.Key.Name}{(topEntry.meta == null ? string.Empty : $" {topEntry.meta}")} : {(topEntry.endTime - topEntry.startTime).TotalMilliseconds:000.0}ms";

            if (topEntry.level > level) {
                //If this entry's level is higher than the last entry, indent
                ImGui.Indent(16.0f);
                indent++;
            } else if (topEntry.level < level) {
                //If this entry's level is lower than the last entry, unindent.
                for (int i = topEntry.level; i < level; i++)
                    ImGui.Unindent();
            }

            ImGui.Text(entryText);

            level = topEntry.level;
        }

        if (ImGui.Begin("Profiler")) {
            Profiler.GetStateNames(ProfilerStateNamesCache);

            foreach (string stateName in ProfilerStateNamesCache) {
                Profiler.GetStateEntries(stateName, ProfilerEntriesCache);

                if (ProfilerEntriesCache.Count == 0)
                    continue;

                ProfilerEntriesQueue.Clear();
                for (int index = ProfilerEntriesCache.Count - 1; index >= 0; index--) {
                    var entry = ProfilerEntriesCache[index];
                    ProfilerEntriesQueue.Enqueue(entry);
                }

                var startTime = ProfilerEntriesCache[0].startTime;
                var endTime = ProfilerEntriesCache[^1].endTime;

                if (ImGui.TreeNode(GetID(), $"{stateName} : {(endTime - startTime).TotalMilliseconds:000.0}ms")) {
                    level = 0;

                    while (ProfilerEntriesQueue.Count > 0)
                        DrawEntry();

                    ImGui.TreePop();
                }

                while (indent > 0) {
                    indent--;
                    ImGui.Unindent();
                }
            }
        }

        ImGui.End();
    }

    private void DrawInputDebug() {
        if (ImGui.Begin("Input State")) {
            ImGui.Text("Connected Gamepads");
            if (ImGui.BeginTable("gamepad", 2)) {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.TableHeader("Index");
                ImGui.TableSetColumnIndex(1);
                ImGui.TableHeader($"Name");
                var gamepads = Client.inputManager!.GetRawGamepads();
                foreach (var gamepad in gamepads) {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{gamepad.Index}");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(gamepad.ControllerName);
                }
                ImGui.EndTable();
            }
        }
        ImGui.End();
    }
}
