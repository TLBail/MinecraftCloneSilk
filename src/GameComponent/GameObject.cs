using ImGuiNET;

namespace MinecraftCloneSilk.GameComponent;

public abstract class GameObject
{
    protected Game game;

    protected GameObject(Game game)
    {
        this.game = game;
        game.startables += start;
        game.updatables += update;
        game.stopable += stop;
    }

    protected virtual void stop() { }

    protected GameObject() : this(Game.getInstance()){    }

    protected virtual  void start() {}

    protected virtual void update(double deltaTime) {}

    public virtual void toImGui()
    {
        ImGui.Text("gameObject : " + this.GetType().Name);
    }

}