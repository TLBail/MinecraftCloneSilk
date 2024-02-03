using Silk.NET.OpenAL;

namespace MinecraftCloneSilk.Audio;

public class AlBuffer : IDisposable
{
    internal uint bufferhandle;
    private AL al;
    public AlBuffer()
    {
        this.al = AudioMaster.GetInstance().al;
        bufferhandle = al.GenBuffer();
    }

    public unsafe void SetData(BufferFormat bufferFormat, ReadOnlySpan<byte> data, int frequency) {
        fixed(byte* ptr = data) {
            al.BufferData(bufferhandle, bufferFormat, ptr, data.Length, frequency);
        }
    }
    


    
    public void Dispose()
    {
        al.DeleteBuffer(bufferhandle);
    }
}