// See https://aka.ms/new-console-template for more information

using Voxel.Client;
using Voxel.Client.Rendering.World;
using Voxel.Common.Util;
using Voxel.Common.World.Generation;

GenerationUtils.LoadNativeLibraries();

ClientConfig.Load();
ClientConfig.Save();

ChunkMeshBuilder.Init(ClientConfig.General.chunkBuildThreadCount);

using var game = new VoxelClient();

await game.Run(Constants.TicksPerSecond, "Foxel Engine");
