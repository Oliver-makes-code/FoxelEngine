// See https://aka.ms/new-console-template for more information

using Voxel.Client;
using Voxel.Client.Rendering.World;
using Voxel.Common.World.Generation;

GenerationUtils.LoadNativeLibraries();
ChunkMeshBuilder.Init(4);

using var game = new VoxelClient();

game.Run(20, "Voxel Game");
