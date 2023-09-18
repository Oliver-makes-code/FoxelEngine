
// TODO: Error checking probably

namespace Voxel.Common
{
	struct ChunkBlockID {
		ChunkBlockID(ushort rawData) { raw = rawData; }
		ChunkBlockID(byte xPos, byte yPos, byte zPos, bool blockIsFluid) {
			raw = (blockIsFluid << 15) | (xPos << 10) | (yPos << 5) | zPos;
		}

		public ushort raw { get; private set; }

		public bool isFluid {
			get { return raw & 0b1000000000000000; }
			set { raw = (value << 15) | (raw & 0b0111111111111111); }
		}
		public byte x {
			get { return raw & 0b0111110000000000; }
			set { raw = (value << 10) | (raw & 0b1000001111111111); }
		}
		public byte y {
			get { return raw & 0b0000001111100000; }
			set { raw = (value << 5) | (raw & 0b1111110000011111); }
		}
		public byte z {
			get { return raw & 0b0000000000011111; }
			set { raw = value | (raw & 0b1111111111100000); }
		}
	}
}
