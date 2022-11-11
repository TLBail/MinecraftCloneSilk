using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class WorldGenerationUI : UiWindow
{
    private World world;
    
    public WorldGenerationUI(Game game, Key? key) : base(game, key) {    }
    public WorldGenerationUI(Game game) : this(game, null) {}

    protected override void start()
    {
        world = (World)game.gameObjects[nameof(World)];
    }

    static WorldGeneration.GenerationParameter parameter;
    protected override void  drawUi()
    {
        ImGui.Begin("World Generation");

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
        
        ImGui.End();
    }
    
    
    
}