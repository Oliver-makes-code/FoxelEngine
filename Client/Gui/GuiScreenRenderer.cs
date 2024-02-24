using System;
using System.Collections.Generic;
using Voxel.Client.World.Gui;
using Voxel.Common.World.Gui;

namespace Voxel.Client.Gui;

public static class GuiScreenRendererRegistry {
    public delegate GuiScreenRenderer CreateRenderer(GuiScreen screen);
    public delegate GuiScreenRenderer CreateRenderer<T>(T screen) where T : GuiScreen;

    private static readonly Dictionary<Type, CreateRenderer> Map = [];

    public static void Register<T>(CreateRenderer<T> constructor) where T : GuiScreen
        => Map[typeof(T)] = s => constructor(s as T);

    public static GuiScreenRenderer GetRenderer(GuiScreen screen)
        => Map[screen.GetType()](screen);
}

public abstract class GuiScreenRenderer<T> : GuiScreenRenderer where T : GuiScreen {

    public readonly T Screen;

    protected GuiScreenRenderer(T screen) {
        Screen = screen;
    }
}

public abstract class GuiScreenRenderer {
    public abstract void Build();
}
