using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.Lighting;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.UI;

public class WorldUi
{
    private readonly World world;
    private readonly string[] blockNames;
    private LightCalculator lightCalculator;
    public WorldUi(World world, LightCalculator lightCalculator)
    {
        this.world = world;
        this.lightCalculator = lightCalculator;
        blockNames = new string[BlockFactory.GetInstance().blocks.Count];
        int index = 0;
        foreach (short id in BlockFactory.GetInstance().blocks.Keys) {
            blockNames[index] = BlockFactory.GetInstance().GetBlockNameById(id);
            index++;
        }

    }

    private static int newBlockX;
    private static int newBlockY;
    private static int newBlockZ;

    private static string newBlockName = "metal";
    private static WorldNaturalGeneration.GenerationParameter parameter;
    
    
    public void DrawUi() {
        ImGui.DragInt("chunk render distance", ref world.radius, 1, 1, 30);
        ImGui.Separator();
        BlockManagementUi();
        ImGui.Separator();
        WorldGenerationUi();
        ImGui.Separator();
        ChunkManager();
        ImGui.Separator();
        LightUi();

    }


    private void ChunkManager() {
        world.chunkManager.ToImGui();
    }
    
    
    private void BlockManagementUi() {
        ImGui.Text("Block management");
        ImGui.Text("add block");

        if(ImGui.BeginCombo("blockname",newBlockName )) {
            for (int n = 0; n < blockNames.Length; n++)
            {
                bool isSelected = (newBlockName == blockNames[n]);
                if (ImGui.Selectable(blockNames[n], isSelected))
                    newBlockName = blockNames[n];
                if (isSelected)
                    ImGui.SetItemDefaultFocus();   // Set the initial focus when opening the combo (scrolling + for keyboard navigation support in the upcoming navigation branch)
            }
            ImGui.EndCombo();
        }
        
        

        ImGui.InputInt("new block x", ref newBlockX);
        ImGui.InputInt("new block y", ref newBlockY);
        ImGui.InputInt("new block z", ref newBlockZ);
        if (ImGui.Button("set block")) {
            world.SetBlock(newBlockName, new Vector3D<int>(newBlockX, newBlockY, newBlockZ));
        }

        WorldMode[] worldModes = (WorldMode[])Enum.GetValues(typeof(WorldMode));
        if(ImGui.BeginCombo("worldMode",world.worldMode.ToString() )) {
            for (int n = 0; n < worldModes.Length; n++)
            {
                bool isSelected = (world.worldMode.ToString() == worldModes[n].ToString());
                if (ImGui.Selectable(worldModes[n].ToString(), isSelected)) {
                    world.SetWorldMode(Enum.Parse<WorldMode>(worldModes[n].ToString()));                    
                }
                if (isSelected)
                    ImGui.SetItemDefaultFocus();   // Set the initial focus when opening the combo (scrolling + for keyboard navigation support in the upcoming navigation branch)
            }
            ImGui.EndCombo();
        }
    }

    private void WorldGenerationUi() {
        if (ImGui.CollapsingHeader("World generation", ImGuiTreeNodeFlags.Bullet) ){
            for (int i = 0; i < WorldNaturalGeneration.generationParameters.Count; i++) {
                parameter = WorldNaturalGeneration.generationParameters[i];
                ImGui.InputFloat("freq" + i, ref parameter.freq);
                ImGui.InputFloat("amp"+ i, ref parameter.amp);
                ImGui.Separator();
                WorldNaturalGeneration.generationParameters[i] = parameter;
            }
        }
    }
    
    private void LightUi() {
        ImGui.Text("Light");
        ImGui.DragFloat("light intensity", ref lightCalculator.lightLevel, 0.01f, 0, 1);
    }
}