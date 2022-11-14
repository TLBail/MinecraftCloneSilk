﻿using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.UI;

public class DebugRayManagerUI : UiWindow
{
    static float newX = 5;
    static float newY = 5;
    static float newZ = 5;

    static float newendX = 5;
    static float newendY = 5;
    static float newendZ = 5;


    private World world;
    public DebugRayManagerUI(Game game, Key? key) : base(game, key) { }
    public DebugRayManagerUI(Game game) : this(game, null) {}

    protected override void start()
    {
        world = (World)game.gameObjects[typeof(World).FullName];
    }


    
    protected override void drawUi()
    {
        ImGui.Begin("DebugRayManager");
        ImGui.Text("ray coordonnate");

        ImGui.InputFloat("start x", ref newX);
        ImGui.InputFloat("start y", ref newY);
        ImGui.InputFloat("start z", ref newZ);

        ImGui.InputFloat("end x", ref newendX);
        ImGui.InputFloat("end y", ref newendY);
        ImGui.InputFloat("end z", ref newendZ);

        if (ImGui.Button("add new ray")) {
            new DebugRay(new Vector3D<float>(newX, newY, newZ),
                new Vector3D<float>(newendX, newendY, newendZ));
        }


        if (ImGui.Button("add ray around chunk")) {
            foreach(Chunk chunk in world.getWorldChunks().Values)
            {
                chunk.debug();
            }
        }
        ImGui.End();
    }
}