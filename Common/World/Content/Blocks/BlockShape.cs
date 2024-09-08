using Foxel.Common.Collections;
using Foxel.Common.Collision;
using Foxel.Common.Server;
using Foxel.Common.Util;
using GlmSharp;

namespace Foxel.Common.World.Content.Blocks; 

public readonly struct BlockShape {
    public static readonly BlockShape FullCube = new([new(new(0, 0, 0), new(1, 1, 1))]);
    public static readonly BlockShape Empty = new([]);

    public const int Size = 64;

    public readonly int BoxCount => Boxes.Length;

    private readonly BitVector Indices;
    private readonly Box[] Boxes;
    private readonly bool?[] Cache = new bool?[6];

    public BlockShape(Span<Box> boxes) {
        Indices = new(Size * Size * Size);
        foreach (var pos in Iteration.Cubic(Size)) {
            var fPos = new dvec3(pos) / Size;
            foreach (var box in boxes) {
                if (box.Contains(fPos)) {
                    Set(pos);
                    break;
                }
            }
        }
        
        Boxes = [..new BoxBuilder(Indices).Boxes()];
    }

    public static int Index(ivec3 pos)
        => pos.x + pos.y * Size + pos.z * Size * Size;

    public bool Get(ivec3 index)
        => Indices.Get(Index(index));

    public IEnumerable<Box> LocalBoxes(ivec3 pos) {
        foreach (var box in Boxes)
            yield return new(box.min + pos, box.max + pos);
    }

    public bool SideFullSquare(Face face) {
        (ivec3 start, ivec3 expand) = face switch {
            Face.North => (new ivec3(0, 0, 0), new ivec3(1, 1, 0)),
            Face.South => (new ivec3(0, 0, 63), new ivec3(1, 1, 0)),
            Face.Up => (new ivec3(0, 63, 0), new ivec3(1, 0, 1)),
            Face.Down => (new ivec3(0, 0, 0), new ivec3(1, 0, 1)),
            Face.West => (new ivec3(0, 0, 0), new ivec3(0, 1, 1)),
            Face.East => (new ivec3(63, 0, 0), new ivec3(0, 1, 1)),
            _ => (new ivec3(0, 0, 0), new ivec3(0, 0, 0))
        };

        if (expand == ivec3.Zero)
            return false;
        
        if (Conditions.IsNonNull(Cache[(int)face], out bool cached))
            return cached;
            
        while ((start * expand).All(it => it < Size - 1)) {
            if (!Get(start)) {
                Cache[(int)face] = false;
                return false;
            }
            start.x += expand.x;
            if (start.x >= Size)
                start.x = 63;
            if (!Get(start)) {
                Cache[(int)face] = false;
                return false;
            }
            start.y += expand.y;
            if (start.y >= Size)
                start.y = 63;
            if (!Get(start)) {
                Cache[(int)face] = false;
                return false;
            }
            start.z += expand.z;
            if (start.z >= Size)
                start.z = 63;
            if (!Get(start)) {
                Cache[(int)face] = false;
                return false;
            }
        }
        Cache[(int)face] = true;
        return true;
    }

    private void Set(ivec3 index)
        => Indices.Set(Index(index));

    private record BoxBuilder(BitVector Indices) {
        public readonly BitVector Explored = new(Indices.Size);
        public ivec3 start = ivec3.Zero;
        public ivec3 end = ivec3.Zero;

        public IEnumerable<Box> Boxes() {
            foreach (var pos in Iteration.Cubic(Size)) {
                int index = Index(pos);
                if (Explored.Get(index))
                    continue;
                Explored.Set(index);
                if (!Indices.Get(index))
                    continue;
                start = pos;
                end = pos + new ivec3(1, 1, 1);
                while (TryExpandX())
                    end.x++;
                while (TryExpandZ())
                    end.z++;
                while (TryExpandY())
                    end.y++;
                yield return new Box((dvec3)start / Size, (dvec3)end / Size);
            }
        }

        public bool TryExpandX() {
            if (end.x >= Size)
                return false;
            foreach (var pos in Iteration.Square(new ivec2(start.y, start.z), new ivec2(end.y, end.z))) {
                var acutalPos = new ivec3(end.x, pos.x, pos.y);
                if (Explored.Get(Index(acutalPos)))
                    return false;
                Explored.Set(Index(acutalPos));
                if (!Indices.Get(Index(acutalPos)))
                    return false;
            }
            return true;
        }

        public bool TryExpandY() {
            if (end.y >= Size)
                return false;
            foreach (var pos in Iteration.Square(new ivec2(start.x, start.z), new ivec2(end.x, end.z))) {
                var acutalPos = new ivec3(pos.x, end.y, pos.y);
                if (Explored.Get(Index(acutalPos)))
                    return false;
                Explored.Set(Index(acutalPos));
                if (!Indices.Get(Index(acutalPos)))
                    return false;
            }
            return true;
        }

        public bool TryExpandZ() {
            if (end.z >= Size)
                return false;
            foreach (var pos in Iteration.Square(new ivec2(start.x, start.y), new ivec2(end.x, end.y))) {
                var acutalPos = new ivec3(pos.x, pos.y, end.z);
                if (Explored.Get(Index(acutalPos)))
                    return false;
                Explored.Set(Index(acutalPos));
                if (!Indices.Get(Index(acutalPos)))
                    return false;
            }
            return true;
        }
    }
}

public enum Face : byte {
    West = 0,
    East = 1,
    Down = 2,
    Up = 3,
    North = 4,
    South = 5
}

public static class FaceExtensions {
    public static Face Opposite(this Face face)
        => face switch {
            Face.West => Face.East,
            Face.East => Face.West,
            Face.North => Face.South,
            Face.South => Face.South,
            Face.Up => Face.Down,
            Face.Down => Face.Up,
            _ => face
        };
}
