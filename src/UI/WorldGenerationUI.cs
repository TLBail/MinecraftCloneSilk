using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class WorldGenerationUI : GameObject
{
    private Key? key;
    private IKeyboard keyboard;
    private bool visible;
    private World world = null;
    
    public WorldGenerationUI(Game game, Key? key) : base(game)
    {
        this.key = key;
        game.uiDrawables += UiDrawables;
        keyboard = game.getKeyboard();
        visible = (key == null);
    }
    public WorldGenerationUI(Game game) : this(game, null) {}

    protected override void start()
    {
        world = (World)game.gameObjects[nameof(World)];
    }

    static WorldGeneration.GenerationParameter parameter;
    private void UiDrawables()
    {
        if (key != null && keyboard.IsKeyPressed((Key)key)) visible = !visible;
        if(!visible) return;
        
        
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