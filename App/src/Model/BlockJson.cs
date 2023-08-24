using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.Model;

[Serializable]
public class BlockJson
{
    public string name { get; set; }
    public bool transparent { get; set; }
    public int id { get; set; }
    public Dictionary<Face, int[]>  texture { get; set; }

    public BlockJson(string name, bool transparent, int id, Dictionary<Face, int[]> texture) {
        this.name = name;
        this.transparent = transparent;
        this.id = id;
        this.texture = texture;
    }
}