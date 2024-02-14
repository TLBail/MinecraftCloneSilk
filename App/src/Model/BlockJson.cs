using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.Model;

[Serializable]
public class BlockJson
{
    public string name { get; set; }
    public bool transparent { get; set; }
    public byte lightEmitting { get; set; }
    public int id { get; set; }
    public Dictionary<Face, int[]>  texture { get; set; }

    public BlockJson(string name, bool transparent, byte lightEmitting, int id, Dictionary<Face, int[]> texture) {
        this.name = name;
        this.transparent = transparent;
        this.lightEmitting = lightEmitting;
        this.id = id;
        this.texture = texture;
    }
}