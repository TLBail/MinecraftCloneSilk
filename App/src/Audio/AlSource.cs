using Silk.NET.OpenAL;

namespace MinecraftCloneSilk.Audio;

public class AlSource : IDisposable
{
    private AL al;
    public uint sourcehandle;
    public AlSource() {
        sourcehandle = AudioMaster.GetInstance().al.GenSource();
        al = AudioMaster.GetInstance().al;
    }
    
    public void Dispose() {
        al.DeleteSource(sourcehandle);
    }

    public void SetProperty(SourceBoolean looping, bool b) {
        al.SetSourceProperty(sourcehandle, looping, b);
    }

    public void SetBuffer(AlBuffer buffer) {
        al.SetSourceProperty(sourcehandle, SourceInteger.Buffer, buffer.bufferhandle);
    }
    
    public void SetProperty(SourceFloat gain, float b) {
        al.SetSourceProperty(sourcehandle, gain, b);
    }
    
    
    public int GetProcessedBuffers() {
        al.GetSourceProperty(sourcehandle, GetSourceInteger.BuffersProcessed, out int processed);
        return processed;
    }
    
    public void UnqueueBuffer(AlBuffer[] buffers) {
        al.SourceUnqueueBuffers(sourcehandle, buffers.Select((a) => a.bufferhandle).ToArray());
    }
    
    public void QueueBuffers(AlBuffer[] buffers) {
        al.SourceQueueBuffers(sourcehandle, buffers.Select((a) => a.bufferhandle).ToArray());
    }

    public void Play() {
        al.SourcePlay(sourcehandle);
    }

    public void Stop() {
        al.SourceStop(sourcehandle);
    }
}