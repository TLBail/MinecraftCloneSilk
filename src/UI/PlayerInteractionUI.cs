using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Collision;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.UI;

public class PlayerInteractionUI : GameObject
{
    private PlayerInteractionToWorld playerInteraction;
    private Key? key;
    private IKeyboard keyboard;
    private bool visible;
    private Chunk lastChunkDebuged;
    
    public PlayerInteractionUI(Game game, Key? key) : base(game)
    {
        this.key = key;
        this.game = game;
        game.uiDrawables += UiDrawables;
        keyboard = game.getKeyboard();
        visible = (key == null);
    }
    public PlayerInteractionUI(Game game) : this(game, null) {}

    protected override void start()
    {
        playerInteraction = ((Player)game.gameObjects[nameof(Player)]).getPlayerInteractionToWorld();
    }


    private void UiDrawables()
    {
        if (key != null && keyboard.IsKeyPressed((Key)key)) visible = !visible;
        if(!visible) return;
        
        
        ImGui.Begin("Player interaction");
        ImGui.Text("hovered block");

        
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
                lastChunkDebuged?.debug(false);
                chunkToDebug?.debug(true);
                lastChunkDebuged =  chunkToDebug;
            }

            Face? face = playerInteraction.getFace();
            ImGui.Text("face toucher : " + face.ToString());
        }
        else {
            ImGui.Text("nothing intersect with player");
        }
        
        switchDebug();
        
        ImGui.End();
    }
    
    
    
    static bool hoveredHiglihtMode = false;  // default value, the button is disabled 
    static float b = 0.3f; //  test whatever color you need from imgui_demo.cpp e.g.
    static float c = 0.5f; // 
    static int i = 3;


    private void switchDebug()
    {

        if (hoveredHiglihtMode == true) {

            ImGui.PushID("disable player debug click");
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(i / 7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(i / 7.0f, b, b, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(i / 7.0f, c, c, 1.0f));
            ImGui.Button("disable player debug click");
            if (ImGui.IsItemClicked(0)) {
                hoveredHiglihtMode = !hoveredHiglihtMode;
            }

            ImGui.PopStyleColor(3);
            ImGui.PopID();
        }
        else {
            if (ImGui.Button("enable player debug click")) {
                hoveredHiglihtMode = true;
            }
        }

    }
}