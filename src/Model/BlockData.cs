namespace MinecraftCloneSilk.Model;

public struct BlockData
{
    public int id; //block id

    public BlockData(ReadOnlySpan<byte> buffer) {
        this.id = BitConverter.ToInt32(buffer);
    }
    
    public BlockData(int id)
    {
        this.id = id;
    }

    public ReadOnlySpan<byte> tobyte() {
        return BitConverter.GetBytes(id);
    }

    public static int sizeofSerializeData() => 4;


}

