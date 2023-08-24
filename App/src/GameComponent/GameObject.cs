using ImGuiNET;

namespace MinecraftCloneSilk.GameComponent;

public abstract class GameObject
{
    protected Game game;

    protected GameObject(Game game)
    {
        this.game = game;
        game.startables += Start;
        game.updatables += Update;
        game.stopable += Stop;
    }

    protected virtual void Stop() { }

    protected GameObject() : this(Game.GetInstance()){    }

    protected virtual  void Start() {}

    protected virtual void Update(double deltaTime) {}

    public virtual void ToImGui()
    {
        ImGui.Text("gameObject : " + this.GetType().Name);
    }

}