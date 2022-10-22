using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.UI;

public class WorldUI
{
    private World world;
    private Game game;
    private Key? key;
    private IKeyboard keyboard;
    private bool visible;
    
    public WorldUI(Game game, World world, Key? key)
    {
        this.key = key;
        this.game = game;
        this.world = world;
        game.uiDrawables += UiDrawables;
        keyboard = game.getKeyboard();
        visible = (key == null);
    }
    public WorldUI(Game game, World world) : this(game, world, null) {}


    private static int newBlockX;
    private static int newBlockY;
    private static int newBlockZ;

    private static string newBlockName = "metal";
        
    
    private void UiDrawables()
    {
        if (key != null && keyboard.IsKeyPressed((Key)key)) visible = !visible;
        if(!visible) return;
        
        
        ImGui.Begin("World");
        ImGui.Text("add block");

        List<string> blockNames = TextureBlock.keys();
        blockNames.Add("airBlock");
        if(ImGui.BeginCombo("blockname",newBlockName )) {
            for (int n = 0; n < blockNames.Count; n++)
            {
                bool is_selected = (newBlockName == blockNames[n]);
                if (ImGui.Selectable(blockNames[n], is_selected))
                    newBlockName = blockNames[n];
                if (is_selected)
                    ImGui.SetItemDefaultFocus();   // Set the initial focus when opening the combo (scrolling + for keyboard navigation support in the upcoming navigation branch)
            }
            ImGui.EndCombo();
        }

        ImGui.InputInt("x", ref newBlockX);
        ImGui.InputInt("y", ref newBlockY);
        ImGui.InputInt("z", ref newBlockZ);
        if (ImGui.Button("set block")) {
            world.addBlock(newBlockName, new Vector3D<int>(newBlockX, newBlockY, newBlockZ));
        }
        
        
        ImGui.End();
    }
}