// See https://aka.ms/new-console-template for more information

using System;
using RenderSurface;
using Voxel.Client;
using Voxel.Client.Rendering.World;

ChunkMeshBuilder.Init(4);

using var game = new VoxelNewClient();

game.Run(20, "Voxel Game");
