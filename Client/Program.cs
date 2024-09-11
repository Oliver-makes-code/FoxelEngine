using Foxel.Client;
using Foxel.Client.Rendering.World.Chunks;
using Foxel.Common.Util;
using Foxel.Common.World.Generation;

GenerationUtils.LoadNativeLibraries();

ClientConfig.Load();
ClientConfig.Save();

ChunkMeshBuilder.Init(ClientConfig.General.chunkBuildThreadCount);

using var game = new VoxelClient();

await game.Run(Constants.TicksPerSecond, "Foxel Engine");
