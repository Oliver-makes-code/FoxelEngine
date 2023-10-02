using System;
using GlmSharp;
using Microsoft.Xna.Framework;

namespace Voxel.Common.World;

public readonly struct TilePos {
    public static readonly TilePos Origin = new(0, 0, 0);
    public static readonly TilePos North = new(0, 0, -1);
    public static readonly TilePos South = new(0, 0, 1);
    public static readonly TilePos East = new(1, 0, 0);
    public static readonly TilePos West = new(-1, 0, 0);
    public static readonly TilePos Up = new(0, 1, 0);
    public static readonly TilePos Down = new(0, -1, 0);
    public static readonly TilePos[] Directions = { East, West, Up, Down, South, North };

    public readonly int x;
    public readonly int y;
    public readonly int z;

    public TilePos(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public TilePos(float x, float y, float z) : this((int)MathF.Floor(x), (int)MathF.Floor(y), (int)MathF.Floor(z)) {}

    public TilePos(vec3 vector3) : this(vector3.x, vector3.y, vector3.z) {}

    public TilePos(int value) : this(value, value, value) {}

    public TilePos(ref ChunkPos chunk, ref ChunkTilePos tile) : this((chunk.x << 5) + tile.X, (chunk.y << 5) + tile.Y, (chunk.z << 5) + tile.Z) {}

    public ChunkPos ChunkPos() => new(x >> 5, y >> 5, z >> 5);
    public ChunkTilePos ChunkTilePos(bool fluid) => new(fluid, x, y, z);

    public override string ToString() => $"({x},{y},{z})";

    public readonly vec3 vector3 => new(x, y, z);

    public static bool operator == (TilePos self, TilePos other)
        => self.x == other.x && self.y == other.y && self.z == other.z;
    
    public static bool operator != (TilePos self, TilePos other)
        => !(self == other);

    public static TilePos operator + (TilePos self, TilePos other)
        => new(self.x + other.x, self.y + other.y, self.z + other.z);

    public static TilePos operator - (TilePos self, TilePos other)
        => new(self.x - other.x, self.y - other.y, self.z - other.z);

    public static TilePos operator * (TilePos self, TilePos other)
        => new(self.x * other.x, self.y * other.y, self.z * other.z);

    public static TilePos operator / (TilePos self, TilePos other)
        => new(self.x / other.x, self.y / other.y, self.z / other.z);

    public static TilePos operator + (TilePos self, int other)
        => self + new TilePos(other);
    
    public static TilePos operator + (int other, TilePos self)
        => self + new TilePos(other);
    
    public static TilePos operator - (TilePos self, int other)
        => self - new TilePos(other);
    
    public static TilePos operator - (int other, TilePos self)
        => new TilePos(other) - self;
    
    public static TilePos operator * (TilePos self, int other)
        => self * new TilePos(other);
    
    public static TilePos operator * (int other, TilePos self)
        => self * new TilePos(other);
    
    public static TilePos operator / (TilePos self, int other)
        => self / new TilePos(other);
    
    public static TilePos operator / (int other, TilePos self)
        => new TilePos(other) / self;

    public static TilePos operator + (TilePos self, vec3 other)
        => self + new TilePos(other);

    public static TilePos operator + (vec3 other, TilePos self)
        => self + new TilePos(other);

    public static TilePos operator - (TilePos self, vec3 other)
        => self - new TilePos(other);

    public static TilePos operator - (vec3 other, TilePos self)
        => new TilePos(other) - self;

    public static TilePos operator * (TilePos self, vec3 other)
        => self * new TilePos(other);

    public static TilePos operator * (vec3 other, TilePos self)
        => self * new TilePos(other);

    public static TilePos operator / (TilePos self, vec3 other)
        => self / new TilePos(other);

    public static TilePos operator / (vec3 other, TilePos self)
        => new TilePos(other) / self;

    public static TilePos operator +(TilePos pos, Axis axis)
        => pos + axis.UnitVector();
    public static TilePos operator - (TilePos pos, Axis axis)
        => pos - axis.UnitVector();

    public enum Axis {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ
    }
}

/// Why can't we have members on enums...
public static class TilePosAxisExtension {
    public static TilePos UnitVector(this TilePos.Axis axis) => TilePos.Directions[(int)axis];
    
    public static TilePos.Axis Opposite(this TilePos.Axis axis) => axis switch {
        TilePos.Axis.PositiveX => TilePos.Axis.NegativeX,
        TilePos.Axis.NegativeX => TilePos.Axis.PositiveX,
        TilePos.Axis.PositiveY => TilePos.Axis.NegativeY,
        TilePos.Axis.NegativeY => TilePos.Axis.PositiveY,
        TilePos.Axis.PositiveZ => TilePos.Axis.NegativeZ,
        TilePos.Axis.NegativeZ => TilePos.Axis.PositiveZ,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };
}
