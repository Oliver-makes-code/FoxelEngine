using Voxel.Client;

namespace Voxel;

public class Init {
	#if CLIENT
	public static readonly bool IsClient = true;
	#else
	public static readonly bool IsClient = false;
	#endif

	public static void Main(string[] args) {
		if (IsClient) {
			VoxelClient.Init();
			VoxelClient.Run();
		} else {
			Console.WriteLine("TODO!");
		}
	}
}
