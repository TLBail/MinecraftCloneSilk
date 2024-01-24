using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Collision;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class PlayerUi
{

    private Player player;
    private PlayerInteractionToWorld playerInteraction;
    public PlayerUi(Player player)
    {
        this.player = player;
        playerInteraction = player.GetPlayerInteractionToWorld();
    }

    public void Start(PlayerInteractionToWorld playerInteractionToWorld) {
        playerInteraction = playerInteractionToWorld;
    }
    
    private Chunk? lastChunkDebuged;
    static float newPlayerX = 5;
    static float newPlayerY = 5;
    static float newPlayerZ = 5;

    static bool hoveredHiglihtMode = false;  // default value, the button is disabled 
    static bool isPlayerDebugEnabled = false;  // default value, the button is disabled 
    static float b = 0.3f; //  test whatever color you need from imgui_demo.cpp e.g.
    static float c = 0.5f; // 
    static int i = 3;
    
    
    public void DrawUi()
    {
        ImGui.Text("player coordonate");
        ImGui.Text("x : [ " + player.position.X.ToString("0.00") +
                   " ] y : [ " + player.position.Y.ToString("0.00") +
                   " ]  z : [ " + player.position.Z.ToString("0.00") + " ]");

        ImGui.InputFloat("new player x", ref newPlayerX);
        ImGui.InputFloat("new player y", ref newPlayerY);
        ImGui.InputFloat("new player z", ref newPlayerZ);
        if (ImGui.Button("tp player to"))
        {
            player.position = new Vector3(newPlayerX, newPlayerY, newPlayerZ);
        }
        
        SwitchPlayerDebug();    
        
        ImGui.Separator();
        
        if (playerInteraction.HaveHitedBlock()) {
            Block block = playerInteraction.GetBlock()!;
            ImGui.Text("intersect with :");
            ImGui.Text(block.ToString());
            Chunk? chunkToDebug = playerInteraction.GetChunk();
            if (chunkToDebug != null) {
                ImGui.Text("world coord block " + (chunkToDebug.position + block.position));
            }
            if (chunkToDebug != null &&  chunkToDebug != lastChunkDebuged) {
                ImGui.Text("world coord block " + chunkToDebug.position + block.position);
                if (hoveredHiglihtMode) {
                    lastChunkDebuged?.Debug(false);
                    chunkToDebug?.Debug(true);
                    lastChunkDebuged =  chunkToDebug;
                }
            }

            Face? face = playerInteraction.GetFace();
            ImGui.Text("face toucher : " + face.ToString());
        }
        else {
            ImGui.Text("nothing intersect with player");
        }

        SwitchDebugChunkHovered();
        
        ImGui.Separator();

        SwitchFrustrumCulling();

    }

    private void SwitchFrustrumCulling() {
        Camera camera = player.camera;
        if (!camera.frustrumUpdate)
        {

            ImGui.PushID("enable player frustrum culling");
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(i/7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(i/7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(i/7.0f, c, c, 1.0f));
            ImGui.Button("enable player frustrum culling");
            if (ImGui.IsItemClicked(0))
            {
                camera.frustrumUpdate = true;
            }
            ImGui.PopStyleColor(3);
            ImGui.PopID();
        }
        else
        {
            if (ImGui.Button("disable player frustrum culling")) {
                camera.frustrumUpdate = false;
            }
        }

        if(ImGui.Button("display frustrum")) {
            camera.DisplayFrustrum();



        }
    }


    private void SwitchDebugChunkHovered()
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
    
    private void SwitchPlayerDebug()
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
                player.Debug();
            }
            ImGui.PopStyleColor(3);
            ImGui.PopID();
        }
        else
        {
            if (ImGui.Button("enable player debug click")) {
                isPlayerDebugEnabled = true;
                player.Debug(true);
            }
        }
    }

    

}