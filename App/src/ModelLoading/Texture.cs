namespace MinecraftCloneSilk.ModelLoading;

public enum TextureType
{
    DIFFUSE,
    SPECULAR
}
public class Texture
{
     uint id;
     private TextureType type;
     private uint index;
     
     public Texture(uint id, TextureType type, uint index) {
         this.id = id;
         this.type = type;
         this.index = index;
     }
}