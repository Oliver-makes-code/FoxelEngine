using System;
using System.Collections.Generic;
using Voxel.Client.World.Gui.Render;
using Voxel.Common.World.Gui;

namespace Voxel.Client.World.Gui;

public static class GuiScreenRendererRegistry {
    public delegate GuiScreenRenderer CreateRenderer(GuiScreen screen);
    public delegate GuiScreenRenderer CreateRenderer<T>(T screen) where T : GuiScreen;

    private static readonly Dictionary<Type, CreateRenderer> Map = [];

    public static void Register<T>(CreateRenderer<T> constructor) where T : GuiScreen
        => Map[typeof(T)] = s => constructor((s as T)!);

    public static GuiScreenRenderer GetRenderer(GuiScreen screen)
        => Map[screen.GetType()](screen);
}
