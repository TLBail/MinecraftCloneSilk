﻿using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.UI;

public class WorldUI
{
    private World world;
    private string[] blockNames;
    public WorldUI(World world)
    {
        this.world = world;
        worldMode = world.worldMode.ToString();
        blockNames = new string[BlockFactory.getInstance().blocksReadOnly.Count];
        int index = 0;
        foreach (int id in BlockFactory.getInstance().blocksReadOnly.Keys) {
            blockNames[index] = BlockFactory.getInstance().getBlockNameById(id);
            index++;
        }

    }

    private static int newBlockX;
    private static int newBlockY;
    private static int newBlockZ;

    private static string newBlockName = "metal";
    private static string worldMode = "EMPTY";

    private string previousWorldMode;
    private static WorldGeneration.GenerationParameter parameter;
    
    public void drawUi()
    {
        ImGui.Text("add block");

        if(ImGui.BeginCombo("blockname",newBlockName )) {
            for (int n = 0; n < blockNames.Length; n++)
            {
                bool is_selected = (newBlockName == blockNames[n]);
                if (ImGui.Selectable(blockNames[n], is_selected))
                    newBlockName = blockNames[n];
                if (is_selected)
                    ImGui.SetItemDefaultFocus();   // Set the initial focus when opening the combo (scrolling + for keyboard navigation support in the upcoming navigation branch)
            }
            ImGui.EndCombo();
        }
        
        

        ImGui.InputInt("new block x", ref newBlockX);
        ImGui.InputInt("new block y", ref newBlockY);
        ImGui.InputInt("new block z", ref newBlockZ);
        if (ImGui.Button("set block")) {
            world.setBlock(newBlockName, new Vector3D<int>(newBlockX, newBlockY, newBlockZ));
        }

        WorldMode[] worldModes = (WorldMode[])Enum.GetValues(typeof(WorldMode));
        if(ImGui.BeginCombo("worldMode",worldMode )) {
            for (int n = 0; n < worldModes.Length; n++)
            {
                bool is_selected = (worldMode == worldModes[n].ToString());
                if (ImGui.Selectable(worldModes[n].ToString(), is_selected))
                    worldMode = worldModes[n].ToString();
                if (is_selected)
                    ImGui.SetItemDefaultFocus();   // Set the initial focus when opening the combo (scrolling + for keyboard navigation support in the upcoming navigation branch)
            }
            ImGui.EndCombo();
        }
        
        ImGui.Separator();
        ImGui.Text("World generation");
        
        if (previousWorldMode != worldMode) {
            world.setWorldMode(Enum.Parse<WorldMode>(worldMode));
            previousWorldMode = worldMode;
        }
        
        ImGui.InputInt("seed", ref WorldGeneration.seed);
        ImGui.Separator();
        for (int i = 0; i < WorldGeneration.generationParameters.Count; i++) {
            parameter = WorldGeneration.generationParameters[i];
            ImGui.InputFloat("freq" + i, ref parameter.freq);
            ImGui.InputFloat("amp"+ i, ref parameter.amp);
            ImGui.Separator();
            WorldGeneration.generationParameters[i] = parameter;
        }

        
        
        if (ImGui.Button("reload Chunks")) {
            world.setWorldMode(WorldMode.EMPTY);
            world.setWorldMode(WorldMode.DYNAMIC);
        }
        
        
        
    }
}