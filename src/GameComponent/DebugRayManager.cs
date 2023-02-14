using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent;

public class DebugRayManager : GameObject
{

        
    static float newX = 5;
    static float newY = 5;
    static float newZ = 5;

    static float newendX = 5;
    static float newendY = 5;
    static float newendZ = 5;

    private World world;

    
    public DebugRayManager(Game game) : base(game) {}

    protected override void start() {
        world = (World)game.gameObjects[typeof(World).FullName];
    }


    public override void toImGui() {
        ImGui.Text("ray coordonnate");

        ImGui.InputFloat("start x", ref newX);
        ImGui.InputFloat("start y", ref newY);
        ImGui.InputFloat("start z", ref newZ);

        ImGui.InputFloat("end x", ref newendX);
        ImGui.InputFloat("end y", ref newendY);
        ImGui.InputFloat("end z", ref newendZ);

        if (ImGui.Button("add new ray")) {
            new DebugRay(new Vector3D<float>(newX, newY, newZ),
                new Vector3D<float>(newendX, newendY, newendZ));
        }


        if (ImGui.Button("add ray around chunk")) {
            foreach(Chunk chunk in world.getWorldChunks())
            {
                chunk.debug();
            }
        }
    }
}