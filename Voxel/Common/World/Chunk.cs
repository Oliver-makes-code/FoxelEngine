using GlmSharp;
using Voxel.Common.Tile;

namespace Voxel.Common.World;

public class Chunk {
    public static readonly Chunk Empty = new();
    public static readonly Chunk Full = FullChunk();

    private static Chunk FullChunk() {
        Chunk chunk = new();
        for (var x = 0; x < 32; x++) {
            for (var y = 0; y < 32; y++) {
                for (var z = 0; z < 32; z++) {
                    chunk[false, x, y, z] = Blocks.Stone.id;
                }
            }
        }
        return chunk;
    }

    readonly ushort[] data = new ushort[ushort.MaxValue];

    public Chunk() {}

    float StackedNoise(long seed, float x, float y, int octaves, float persistence) {
        float total = 0;
        float amplitude = 1;
        float frequency = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += OpenSimplex2.Noise2(seed, x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

    public void FillWithSimplexNoise(ChunkPos pos) {
        for (byte x = 0; x < 0b10_0000u; x++) {
            for (byte z = 0; z < 0b10_0000u; z++) {
                var noise = StackedNoise(0, (pos.x*32f + x)/128, (pos.z*32f + z)/128, 4, 0.4f);
                noise += 1;
                noise *= 32;
                var bin_noise = (byte)noise - pos.y*32;
                if (bin_noise > 32)
                    bin_noise = 32;
                for (byte y = 0; y < bin_noise; y++) {
                    this[false, x, y, z] = (ushort)0b0000_0000_0010_0000u;
                }
            }
        }
    }

    public ushort this[int idx] {
        get => data[idx];
        set => data[idx] = value;
    }

    public ushort this[ChunkTilePos pos] {
        get => this[pos.Raw];
        set => this[pos.Raw] = value;
    }

    public ushort this[bool fluid, int x, int y, int z] {
        get => this[ChunkTilePos.GetRawFrom(fluid, x, y, z)];
        set => this[ChunkTilePos.GetRawFrom(fluid, x, y, z)] = value;
    }
}

public readonly struct ChunkPos {
    public readonly int x;
    public readonly int y;
    public readonly int z;

    public ChunkPos(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public vec3 ToVector() => new(x * 32, y * 32, z * 32);

    public override int GetHashCode() {
        var hashCode = x;
        hashCode *= 23;
        hashCode += y;
        hashCode *= 23;
        hashCode += z;
        return hashCode;
    }

    public static ChunkPos operator + (ChunkPos a, ChunkPos b)
        => new(a.x+b.x, a.y+b.y, a.z+b.z);

    public static ChunkPos operator - (ChunkPos a, ChunkPos b)
        => new(a.x-b.x, a.y-b.y, a.z-b.z);
    
    public ChunkPos Up() => new(x, y+1, z);
    
    public ChunkPos Down() => new(x, y-1, z);
    
    public ChunkPos North() => new(x, y, z-1);
    
    public ChunkPos South() => new(x, y, z+1);
    
    public ChunkPos East() => new(x+1, y, z);
    
    public ChunkPos West() => new(x-1, y, z);

    public override string ToString() => $"({x}, {y}, {z})";
}

public readonly struct ChunkTilePos {
    private const int TestFluid = 0b1000000000000000;
    private const int BitmaskCoord = 0b11111;
    private const int ShiftX = 10;
    private const int ShiftY = 5;
    
    private readonly int _x;
    private readonly int _y;
    private readonly int _z;
    private readonly bool _fluid;
    
    public static int GetRawFrom(bool isFluid, int x, int y, int z)
        => (isFluid ? TestFluid : 0) | ((x & BitmaskCoord) << ShiftX) | ((y & BitmaskCoord) << ShiftY) | (z & BitmaskCoord);
    
    public int Raw => GetRawFrom(_fluid, _x, _y, _z);

    public ChunkTilePos(int raw) {
        _fluid = (raw & TestFluid) != 0;
        _x = (raw >> ShiftX) & BitmaskCoord;
        _y = (raw >> ShiftY) & BitmaskCoord;
        _z = raw & BitmaskCoord;
    }
    
    public ChunkTilePos(bool fluid, int x, int y, int z) {
        _fluid = fluid;
        _x = x & BitmaskCoord;
        _y = y & BitmaskCoord;
        _z = z & BitmaskCoord;
    }
    
    public bool IsFluid => _fluid;

    public int X => _x;
    public int Y => _y;
    public int Z => _z;
}
