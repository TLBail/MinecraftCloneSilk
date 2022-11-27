namespace MinecraftCloneSilk.Model;

public struct BlockData
{
    public int id; //block id

    public BlockData(string name)
    {
        this.id = name.GetHashCode();
    }

    public BlockData(int id)
    {
        this.id = id;
    }
}

