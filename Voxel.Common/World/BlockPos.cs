using Microsoft.Xna.Framework;

namespace Voxel.Common.World;

public readonly struct BlockPos {
    public static readonly BlockPos Empty = new(0, 0, 0);
    public static readonly BlockPos North = new(0, 0, -1);
    public static readonly BlockPos South = new(0, 0, 1);
    public static readonly BlockPos East = new(1, 0, 0);
    public static readonly BlockPos West = new(-1, 0, 0);
    public static readonly BlockPos Up = new(0, 1, 0);
    public static readonly BlockPos Down = new(0, -1, 0);

    public readonly int x;
    public readonly int y;
    public readonly int z;

    public BlockPos(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public BlockPos(Vector3 vector3) : this((int)vector3.X, (int)vector3.Y, (int)vector3.Z) {}

    public BlockPos(int value) : this(value, value, value) {}

    public BlockPos(ChunkPos chunk, ChunkBlockPos block) : this((chunk.x << 5) + block.X, (chunk.y << 5) + block.Y, (chunk.z << 5) + block.Z) {}

    public ChunkPos ChunkPos() => new(x >> 5, y >> 5, z >> 5);
    public ChunkBlockPos ChunkBlockPos(bool fluid) => new(fluid, x, y, z);
    
    public readonly Vector3 vector3 => new(x, y, z);

    public static BlockPos operator + (BlockPos self, BlockPos other)
        => new(self.x + other.x, self.y + other.y, self.z + other.z);

    public static BlockPos operator - (BlockPos self, BlockPos other)
        => new(self.x - other.x, self.y - other.y, self.z - other.z);

    public static BlockPos operator * (BlockPos self, BlockPos other)
        => new(self.x * other.x, self.y * other.y, self.z * other.z);

    public static BlockPos operator / (BlockPos self, BlockPos other)
        => new(self.x / other.x, self.y / other.y, self.z / other.z);

    public static BlockPos operator + (BlockPos self, int other)
        => self + new BlockPos(other);
    
    public static BlockPos operator + (int other, BlockPos self)
        => self + new BlockPos(other);
    
    public static BlockPos operator - (BlockPos self, int other)
        => self - new BlockPos(other);
    
    public static BlockPos operator - (int other, BlockPos self)
        => new BlockPos(other) - self;
    
    public static BlockPos operator * (BlockPos self, int other)
        => self * new BlockPos(other);
    
    public static BlockPos operator * (int other, BlockPos self)
        => self * new BlockPos(other);
    
    public static BlockPos operator / (BlockPos self, int other)
        => self / new BlockPos(other);
    
    public static BlockPos operator / (int other, BlockPos self)
        => new BlockPos(other) / self;

    public static BlockPos operator + (BlockPos self, Vector3 other)
        => self + new BlockPos(other);

    public static BlockPos operator + (Vector3 other, BlockPos self)
        => self + new BlockPos(other);

    public static BlockPos operator - (BlockPos self, Vector3 other)
        => self - new BlockPos(other);

    public static BlockPos operator - (Vector3 other, BlockPos self)
        => new BlockPos(other) - self;

    public static BlockPos operator * (BlockPos self, Vector3 other)
        => self * new BlockPos(other);

    public static BlockPos operator * (Vector3 other, BlockPos self)
        => self * new BlockPos(other);

    public static BlockPos operator / (BlockPos self, Vector3 other)
        => self / new BlockPos(other);

    public static BlockPos operator / (Vector3 other, BlockPos self)
        => new BlockPos(other) / self;
}
