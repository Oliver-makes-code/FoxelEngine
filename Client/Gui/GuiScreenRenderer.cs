using System;
using System.Collections.Generic;
using Voxel.Client.World.Gui;
using Voxel.Common.World.Gui;

namespace Voxel.Client.Gui;

public static class GuiScreenRendererRegistry {
    public delegate GuiScreenRenderer<T> CreateRenderer<T>(T screen) where T : GuiScreen;

    private static readonly Dictionary<Type, CreateRenderer<GuiScreen>> Map = [];

    public static void Register<T>(CreateRenderer<T> constructor) where T : GuiScreen
        => Map[typeof(T)] = screen => constructor(screen as T) as GuiScreenRenderer<GuiScreen>;

    public static GuiScreenRenderer<T> GetRenderer<T>(T screen) where T : GuiScreen
        => GetRenderer(typeof(T), screen) as GuiScreenRenderer<T>;
    
    public static GuiScreenRenderer<GuiScreen> GetRenderer(Type type, GuiScreen screen)
        => Map[type](screen);
}

public abstract class GuiScreenRenderer<T> where T : GuiScreen {

    public readonly T Screen;

    protected GuiScreenRenderer(T screen) {
        Screen = screen;
    }
    
    public abstract void Build();
}
