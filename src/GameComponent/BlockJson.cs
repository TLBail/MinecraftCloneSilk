namespace MinecraftCloneSilk.GameComponent;

[Serializable]
public class BlockJson
{
    public bool transparent;
    public string? name { get; set; }
    public Dictionary<Face, int[]>  texture { get; set; }
    
    
}