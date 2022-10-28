using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class PlayerUi
{

    private Player player;
    public PlayerUi(Player player)
    {
        this.player = player;
    }
    

    static float newX = 5;
    static float newY = 5;
    static float newZ = 5;

    public void drawUi()
    {
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
        
        switchDebug();    
    }

    
    static bool enable_7m = false;  // default value, the button is disabled 
    static float b = 0.3f; //  test whatever color you need from imgui_demo.cpp e.g.
    static float c = 0.5f; // 
    static int i = 3;

    
    private void switchDebug()
    {
        

        if (enable_7m == true)
        {

            ImGui.PushID("disable player debug click");
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(i/7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(i/7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(i/7.0f, c, c, 1.0f));
            ImGui.Button("disable player debug click");
            if (ImGui.IsItemClicked(0))
            {
                enable_7m = !enable_7m;
                player.debug();
            }
            ImGui.PopStyleColor(3);
            ImGui.PopID();
        }
        else
        {
            if (ImGui.Button("enable player debug click")) {
                enable_7m = true;
                player.debug(true);
            }
        }
    }

    

}