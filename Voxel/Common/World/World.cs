using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Voxel.Common.Tile;

namespace Voxel.Common.World;

public class World {
    public delegate void ChunkUpdateEvent(ChunkPos[] chunks);

    public event ChunkUpdateEvent? OnChunkLoaded;
    public event ChunkUpdateEvent? OnChunkUnloaded;
    public event ChunkUpdateEvent? OnChunkChanged;

    public ConcurrentDictionary<ChunkPos, Chunk> chunks = new();

    public ConcurrentQueue<ChunkPos> ChunksToLoad = new();
    public ConcurrentQueue<ChunkPos> ChunksToRemove = new();

    private Thread _chunkLoadingThread = new(o => {
        var self = o as World ?? throw new InvalidOperationException();
        while (true) {
            List<ChunkPos> unloaded = new();
            while (self.ChunksToRemove.TryDequeue(out var toRemove)) {
                if (!self.IsChunkLoaded(toRemove))
                    continue;
                self.Unload(toRemove);
                unloaded.Add(toRemove);
            }
            if (unloaded.Count != 0)
                self.OnChunkUnloaded?.Invoke(unloaded.ToArray());

            List<ChunkPos> loaded = new();
            while (self.ChunksToLoad.TryDequeue(out var toAdd)) {
                if (self.IsChunkLoaded(toAdd)) 
                    continue;
                self.Load(toAdd, out var chunk);
                chunk.FillWithSimplexNoise(toAdd);
                loaded.Add(toAdd);
            }
            if (loaded.Count != 0)
                self.OnChunkLoaded?.Invoke(loaded.ToArray());
        }
    });

    public Chunk? this[ChunkPos pos] {
        get {
            chunks.TryGetValue(pos, out Chunk? chunk);
            return chunk;
        }
    }

    public bool IsChunkLoaded(ChunkPos pos) => chunks.ContainsKey(pos);

    public void Load(ChunkPos pos, out Chunk chunk) {
        if (IsChunkLoaded(pos)) {
            chunk = chunks[pos];
            return;
        }
        chunk = new();
        chunks[pos] = chunk;
    }

    public void Unload(ChunkPos pos) {
        if (!IsChunkLoaded(pos))
            return;
        chunks.Remove(pos, out _);
    }
    
    public ushort GetTile(TilePos pos, bool fluid) => this[pos.ChunkPos()]?[pos.ChunkTilePos(fluid)] ?? 0;
    public Block GetBlock(TilePos pos) => Blocks.GetBlock(GetTile(pos, false));
    public ushort GetFluid(TilePos pos) => GetTile(pos, true);

    public void SetTile(TilePos pos, bool fluid, ushort tileId) {
        var chunk = this[pos.ChunkPos()];
        if (chunk == null)
            return;
        chunk[pos.ChunkTilePos(fluid)] = tileId;
        OnChunkChanged?.Invoke(new[] {pos.ChunkPos()});
    }

    public void SetBlock(TilePos pos, Block block, byte blockstate = 0) => SetTile(pos, false, block.GetWorldId(blockstate));

    public World() {
        _chunkLoadingThread.Start(this);
    }

    public void Tick() {
        
    }

    public void OnExiting() {
        _chunkLoadingThread.Interrupt();
    }
}

public class ChunkView {
    public ChunkPos pos;
    public Chunk[] chunks;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIdx(int x, int y, int z) => x * 9 + y * 3 + z;
    
    public ChunkView(World world, ChunkPos pos) {
        this.pos = pos + new ChunkPos(-1, -1, -1);
        chunks = new Chunk[27];

        for (var x = 0; x < 3; x++) {
            for (var y = 0; y < 3; y++) {
                for (var z = 0; z < 3; z++) {
                    chunks[GetIdx(x,y,z)] = world[pos + new ChunkPos(x-1,y-1,z-1)] ?? Chunk.Full;
                }
            }
        }
    }

    public ushort GetTile(TilePos tilePos, bool fluid)
        => chunks[GetIdx(
            (tilePos.x >> 5) - pos.x,
            (tilePos.y >> 5) - pos.y,
            (tilePos.z >> 5) - pos.z
        )][
            ChunkTilePos.GetRawFrom(fluid, tilePos.x, tilePos.y, tilePos.z)
        ];
    
    public Block GetBlock(TilePos tilePos) => Blocks.GetBlock(GetTile(tilePos, false));
    
    public ushort GetFluid(TilePos tilePos) => GetTile(tilePos, true);
}
