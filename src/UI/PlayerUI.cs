using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class PlayerUi
{

    private Player player;
    private PlayerInteractionToWorld playerInteraction;
    public PlayerUi(Player player)
    {
        this.player = player;
        playerInteraction = player.getPlayerInteractionToWorld();
    }

    public void start(PlayerInteractionToWorld playerInteractionToWorld) {
        playerInteraction = playerInteractionToWorld;
    }
    
    private Chunk lastChunkDebuged;
    static float newX = 5;
    static float newY = 5;
    static float newZ = 5;

    static bool hoveredHiglihtMode = false;  // default value, the button is disabled 
    static bool isPlayerDebugEnabled = false;  // default value, the button is disabled 
    static float b = 0.3f; //  test whatever color you need from imgui_demo.cpp e.g.
    static float c = 0.5f; // 
    static int i = 3;
    
    
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
        
        switchPlayerDebug();    
        
        ImGui.Separator();
        
        if (playerInteraction.haveHitedBlock()) {
            Block block = playerInteraction.getBlock();
            ImGui.Text("intersect with :");

            ImGui.Text(block.ToString());
            Chunk? chunkToDebug = playerInteraction.getChunk();
            if (chunkToDebug != null) {
                ImGui.Text("world coord block " + (chunkToDebug.getPosition() + block.position));
            }
            if (chunkToDebug != null &&  chunkToDebug != lastChunkDebuged) {
                ImGui.Text("world coord block " + chunkToDebug.getPosition() + block.position);
                if (hoveredHiglihtMode) {
                    lastChunkDebuged?.debug(false);
                    chunkToDebug?.debug(true);
                    lastChunkDebuged =  chunkToDebug;
                }
            }

            Face? face = playerInteraction.getFace();
            ImGui.Text("face toucher : " + face.ToString());
        }
        else {
            ImGui.Text("nothing intersect with player");
        }

        switchDebugChunkHovered();

    }

  
    private void switchDebugChunkHovered()
    {

        if (hoveredHiglihtMode == true) {

            ImGui.PushID("remove chunk highlight hovered");
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(i / 7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(i / 7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(i / 7.0f, c, c, 1.0f));
            ImGui.Button("remove chunk highlight hovered");
            if (ImGui.IsItemClicked(0)) {
                hoveredHiglihtMode = !hoveredHiglihtMode;
            }

            ImGui.PopStyleColor(3);
            ImGui.PopID();
        }
        else {
            if (ImGui.Button("display chunk hovered")) {
                hoveredHiglihtMode = true;
            }
        }
    }
    
    private void switchPlayerDebug()
    {
        

        if (isPlayerDebugEnabled == true)
        {

            ImGui.PushID("disable player debug click");
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(i/7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(i/7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(i/7.0f, c, c, 1.0f));
            ImGui.Button("disable player debug click");
            if (ImGui.IsItemClicked(0))
            {
                isPlayerDebugEnabled = !isPlayerDebugEnabled;
                player.debug();
            }
            ImGui.PopStyleColor(3);
            ImGui.PopID();
        }
        else
        {
            if (ImGui.Button("enable player debug click")) {
                isPlayerDebugEnabled = true;
                player.debug(true);
            }
        }
    }

    

}