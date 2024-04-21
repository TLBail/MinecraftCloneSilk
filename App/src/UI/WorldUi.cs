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
        chunkRenderDistance = this.world.radius;
        worldMode = world.worldMode.ToString();
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
    private static string worldMode = "EMPTY";

    private static WorldNaturalGeneration.GenerationParameter parameter;
    private static int chunkRenderDistance;
    
    
    public void DrawUi() {
        if (ImGui.DragInt("chunk render distance", ref chunkRenderDistance, 1, 1, 30)) {
            world.radius = chunkRenderDistance;
        }
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
        if(ImGui.BeginCombo("worldMode",worldMode )) {
            for (int n = 0; n < worldModes.Length; n++)
            {
                bool isSelected = (worldMode == worldModes[n].ToString());
                if (ImGui.Selectable(worldModes[n].ToString(), isSelected)) {
                    worldMode = worldModes[n].ToString();
                    world.SetWorldMode(Enum.Parse<WorldMode>(worldMode));                    
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