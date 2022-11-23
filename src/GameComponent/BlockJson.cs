namespace MinecraftCloneSilk.GameComponent;

[Serializable]
public class BlockJson
{
    public string? name { get; set; }
    public bool transparent { get; set; }
    public Dictionary<Face, int[]>  texture { get; set; }
    
}