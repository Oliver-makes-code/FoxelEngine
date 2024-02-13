using System;
using System.Collections.Generic;
using Voxel.Client.World.Gui;
using Voxel.Common.World.Gui;

namespace Voxel.Client.Gui;

public static class GuiScreenRendererRegistry {
    private delegate GuiScreenRenderer<GuiScreen>? CreateRenderer(GuiScreen screen);
    public delegate GuiScreenRenderer<T> CreateRendererGeneric<T>(T screen) where T : GuiScreen;

    private static readonly Dictionary<Type, CreateRenderer> Map = [];

    public static void Register<T>(CreateRendererGeneric<T> constructor) where T : GuiScreen
        => Map[typeof(T)] = screen => constructor(screen as T ?? throw new ArgumentException("how??")) as GuiScreenRenderer<GuiScreen>;

    public static GuiScreenRenderer<T>? GetRenderer<T>(T screen) where T : GuiScreen
        => Map[typeof(T)](screen) as GuiScreenRenderer<T> ?? throw new ArgumentException("how??");
}

public abstract class GuiScreenRenderer<T> where T : GuiScreen {

    public readonly T Screen;

    protected GuiScreenRenderer(T screen) {
        Screen = screen;
    }
    
    public abstract void Build();
}
