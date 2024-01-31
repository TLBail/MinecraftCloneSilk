namespace MinecraftCloneSilk.Core;



public struct InitGameData
{
    public string typeName;
    public Object[] pars;

    public InitGameData(string typeName, object[] pars) {
        this.typeName = typeName;
        this.pars = pars;
    }
    public InitGameData(string typeName) : this(typeName, new Object[]{}) {}
}



public class Scene
{
    public OpenGlConfig  openGlConfig;
    public List<InitGameData> gameObjects { get; init; }

    public Scene(List<InitGameData> gameObjects, OpenGlConfig config) {
        this.gameObjects = gameObjects;
        this.openGlConfig = config;
    }
    
    
}