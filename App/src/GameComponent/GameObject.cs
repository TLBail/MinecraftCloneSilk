﻿using ImGuiNET;
using Console = MinecraftCloneSilk.UI.Console;

namespace MinecraftCloneSilk.GameComponent;

public abstract class GameObject
{
    protected Game game;
    protected Console console;
    private object haveStartedLock = new object();
    private bool haveStarted = false;
    private bool isDestroyed = false;

    protected GameObject(Game game)
    {
        this.game = game;
        this.console = this.game.console;
        game.startables += mStart;
    }

    public virtual void Destroy() {
        //Todo find a way to remove the gameobject faster like storing the key in the gameobject
        var key= game.gameObjects.First((obj) => obj.Value == this).Key;
        game.gameObjects.Remove(key);
        lock (haveStartedLock) {
            if (!haveStarted) return;
            game.startables -= mStart;
            game.updatables -= Update;
        }
        isDestroyed = true;
    }

    protected virtual void Start() { }

    protected virtual void mStart() {
        lock (haveStartedLock) {
            game.updatables += Update;
            haveStarted = true;
        }
        if (isDestroyed) throw new Exception("GameObject is destroyed");
        Start();
    }

    protected virtual void Update(double deltaTime) {}

    public virtual void ToImGui()
    {
        ImGui.Text("gameObject : " + this.GetType().Name);
    }

}