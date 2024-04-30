using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Audio;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Input;
using Button = MinecraftCloneSilk.UI.UiComponent.Button;
namespace MinecraftCloneSilk.UI.Start;

internal interface Screen
{
   public void DrawUi(); 
}

internal class Config
{
    public string savePath; // ignored if saveTheWorld is false
    public int seed;
    public int renderDistance;
    public bool saveTheWorld;
    public WorldMode worldMode;

    public Config(string savePath, int seed, int renderDistance, bool saveTheWorld, WorldMode worldMode) {
        this.savePath = savePath;
        this.seed = seed;
        this.renderDistance = renderDistance;
        this.saveTheWorld = saveTheWorld;
        this.worldMode = worldMode;
    }
}

public class StartingWindow : UiWindow
{


    internal Config config;
    private Home home;
    private Options optionScreen;
    private Screen activeScreen;

    
    public StartingWindow(Game game, Key? key) : base(game, key) {
        config = new Config("Worlds/newWorld", 1234, 10, true, WorldMode.DYNAMIC);
        home = new Home(this);
        optionScreen = new Options(this);
        activeScreen = home;
    }
    public StartingWindow(Game game) : this(game, null) {}

    public override void Destroy() {
        base.Destroy();
        home.Dispose();
        optionScreen.Dispose();
    }

    protected override void  DrawUi() {
        activeScreen.DrawUi();
    }

    internal void Quit() {
       game.Stop(); 
    }

    internal void Play() {
        game.AddGameObject(new DebugRayManager(game));
        game.AddGameObject(new ConsoleUi(game));
        game.AddGameObject(new ChunkRendererUi(game));
        game.AddGameObject(new InventaireUi(game));
        game.AddGameObject(new ItemBarUi(game));
        game.AddGameObject(new GameUi(game));
        game.AddGameObject(new PauseMenu(game));
        

        World world = game.FindGameObject<World>();
        if (config.saveTheWorld) {
            world.Reset(new WorldNaturalGeneration(config.seed), config.worldMode, config.savePath);
        } else {
            world.Reset(new WorldNaturalGeneration(config.seed), config.worldMode);
        }
        world.radius = config.renderDistance;

        this.Destroy();
    }
    
    public void Option() {
        activeScreen = optionScreen;
    }

    public void RetourHome() {
        activeScreen = home;
    }
}