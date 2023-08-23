namespace MinecraftCloneSilk.Model;

public struct BlockData
{
    public int id; //block id

    public BlockData(ReadOnlySpan<byte> buffer) {
        this.id = BitConverter.ToInt16(buffer);
    }
    
    public BlockData(int id)
    {
        this.id = id;
    }

    public BlockData(BinaryReader br) {
        this.id = br.ReadInt16();
    }

    public byte[] tobyte() {
        return BitConverter.GetBytes((short)id);
    }

    public const int sizeofSerializeData = 2;

    public override bool Equals(object? obj) {
        return obj is BlockData data && id == data.id;
    }
}

