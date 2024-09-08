using System;
using Foxel.Client;
using Foxel.Client.Rendering.World;
using Foxel.Common.Util;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Generation;
using GlmSharp;

// Console.WriteLine(SlabBlock.Shape.SideFullSquare(Face.Up));
// Console.WriteLine(SlabBlock.Shape.SideFullSquare(Face.Down));
// Console.WriteLine(SlabBlock.Shape.SideFullSquare(Face.North));
// Console.WriteLine(SlabBlock.Shape.SideFullSquare(Face.South));
// Console.WriteLine(SlabBlock.Shape.SideFullSquare(Face.East));
// Console.WriteLine(SlabBlock.Shape.SideFullSquare(Face.West));

GenerationUtils.LoadNativeLibraries();

ClientConfig.Load();
ClientConfig.Save();

ChunkMeshBuilder.Init(ClientConfig.General.chunkBuildThreadCount);

using var game = new VoxelClient();

await game.Run(Constants.TicksPerSecond, "Foxel Engine");
