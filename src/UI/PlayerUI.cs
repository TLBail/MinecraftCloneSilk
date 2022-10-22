using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class PlayerUi
{

    private Player player;
    private bool visible;
    private IKeyboard keyboard;
    private Key? key;
    public PlayerUi(Game game, Player player, Key? key)
    {
        game.uiDrawables += uiDrawables;
        this.player = player;
        keyboard = game.getKeyboard();
        visible = (key == null);
        this.key = key;
        
    }

    public PlayerUi(Game game, Player player) : this(game, player, null) {}

    static float newX = 5;
    static float newY = 5;
    static float newZ = 5;

    
    private void uiDrawables()
    {
        if (key != null && keyboard.IsKeyPressed((Key)key)) visible = !visible;
        if(!visible) return;
        
        
        ImGui.Begin("Player");
        ImGui.Text("player coordonate");
        ImGui.Text("x : [ " + player.position.X.ToString("0.00") +
                   " ] y : [ " + player.position.Y.ToString("0.00") +
                   " ]  z : [ " + player.position.Z.ToString("0.00") + " ]");

        ImGui.InputFloat("x", ref newX);
        ImGui.InputFloat("y", ref newY);
        ImGui.InputFloat("z", ref newZ);
        if (ImGui.Button("tp player to"))
        {
            player.position = new Vector3(newX, newY, newZ);
        }
        
        

        
        
        ImGui.End();
        
    }
}