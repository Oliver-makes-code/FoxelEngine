using GlmSharp;
using Voxel.Common.Util;

namespace Voxel.Client.Rendering.World;

public class ChunkRenderer : Renderer {

    private ChunkRenderSlot[]? _renderSlots;
    private int _renderDistance = 0;

    private ivec3 _renderPosition = ivec3.Zero;

    private ChunkRenderSlot? this[int x, int y, int z] {
        get {
            if (_renderSlots == null) return null;

            var index = z + y * PositionExtensions.CHUNK_STEP + x * PositionExtensions.CHUNK_SIZE;

            return _renderSlots[index];
        }
        set {
            if (_renderSlots == null) return;

            var index = z + y * PositionExtensions.CHUNK_STEP + x * PositionExtensions.CHUNK_SIZE;

            _renderSlots[index] = value;
        }
    }

    private ChunkRenderSlot? this[ivec3 pos] {
        get => this[pos.x, pos.y, pos.z];
        set => this[pos.x, pos.y, pos.z] = value;
    }

    public ChunkRenderer(VoxelNewClient client) : base(client) {
        SetRenderDistance(5);
    }

    public override void Render(double delta) {
        if (_renderSlots == null)
            return;

        foreach (var slot in _renderSlots)
            slot.Render(delta);
    }

    public void SetRenderDistance(int distance) {
        if (_renderSlots != null)
            foreach (var slot in _renderSlots)
                slot.Dispose(); //Todo - Cache and re-use instead of dispose

        var realRenderDistance = ((_renderDistance * 2) + 1);
        var totalChunks = realRenderDistance * realRenderDistance;
        _renderSlots = new ChunkRenderSlot[totalChunks];

        for (int x = 0; x < realRenderDistance; x++)
        for (int y = 0; y < realRenderDistance; y++)
        for (int z = 0; z < realRenderDistance; z++) {
            this[x, y, z] = new(Client, new ivec3(x, y, z) - distance);
        }
    }

    public void SetRenderPosition(dvec3 worldPosition) {
        var newPos = worldPosition.WorldToChunkPosition();

        if (newPos == _renderPosition || _renderSlots == null)
            return;
        _renderPosition = newPos;

        foreach (var slot in _renderSlots)
            slot.Move(_renderPosition);
    }

    public override void Dispose() {
        if (_renderSlots == null)
            return;

        foreach (var slot in _renderSlots)
            slot.Dispose();
        _renderSlots = null;
    }
}
