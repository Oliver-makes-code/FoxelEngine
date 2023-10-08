// See https://aka.ms/new-console-template for more information

using Voxel.Client;
using Voxel.Client.Rendering.World;

ChunkMeshBuilder.Init(16);

using var game = new VoxelNewClient();

game.Run(20, "Voxel Game");
