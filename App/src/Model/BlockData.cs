namespace MinecraftCloneSilk.Model;

public struct BlockData
{
    public int id; //block id
    public int data1; // lightLevel [0..4]

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

    public byte[] Tobyte() {
        return BitConverter.GetBytes((short)id);
    }

    public const int SIZEOF_SERIALIZE_DATA = 2;

    public override bool Equals(object? obj) {
        return obj is BlockData data && id == data.id;
    }

    public bool Equals(BlockData other) {
        return id == other.id;
    }

    public override int GetHashCode() {
        return id;
    }
}

