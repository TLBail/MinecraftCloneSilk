using System.Diagnostics;

namespace MinecraftCloneSilk.Model;

public struct BlockData
{
    private int data; 
    // 16 bits for block id, 4 bits for light level, 4 bits for sky light level
    
    public readonly int id => data & 0xFFFF;

    public BlockData(ReadOnlySpan<byte> buffer) {
        this.data = BitConverter.ToInt32(buffer);
    }
    
    public BlockData(int id)
    {
        this.data = id & 0xFFFF;
    }

    public BlockData(BinaryReader br) {
        this.data = br.ReadInt32();
    }
    
    public byte GetLightLevel() {
        return (byte)((data >> 16)& 0xF);
    }
    
    public void SetLightLevel(byte lightLevel) {
        Debug.Assert(lightLevel < 16);
        data = (data & ~(0xF << 16)) | (lightLevel << 16);
    }
    
    public byte GetSkyLightLevel() {
        return (byte)(((data >> 16) & 0xF0) >> 4);
    }
    
    public void SetSkyLightLevel(byte lightLevel) {
        data = (data & ~(0xF0 << 16)) | (lightLevel << (4 + 16));
    }

    public const int SIZEOF_SERIALIZE_DATA = 4;
    
    public void WriteToStream(BinaryWriter bw) {
        bw.Write(data);
    }

    public override bool Equals(object? obj) {
        return obj is BlockData block && this.data == block.data;
    }

    public bool Equals(BlockData other) {
        return data == other.data;
    }

    public override int GetHashCode() {
        return data & 0xFFFF;
    }
}

