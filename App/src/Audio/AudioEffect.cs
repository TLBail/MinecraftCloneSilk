using System.Buffers.Binary;
using NVorbis;
using Silk.NET.OpenAL;

namespace MinecraftCloneSilk.Audio;

public class AudioEffect : IDisposable
{

    private AL al;
    private AlSource alSource;
    private AlBuffer alBuffer;
    
    public AudioEffect(string path) {
        this.al = AudioMaster.GetInstance().al;
        VorbisReader vorbisReader = new VorbisReader(path);
        
        int channels = vorbisReader.Channels;
        int sampleRate = vorbisReader.SampleRate;
        BufferFormat format = BufferFormat.Mono16; 
        if (channels == 2)
        {
            format = BufferFormat.Stereo16; 
        }
        
        
        alSource = new AlSource();
        alBuffer = new AlBuffer();
        
        float[] readBuffer = new float[channels * vorbisReader.TotalSamples]; 
        Span<byte> rawData = new Span<byte>(new byte[readBuffer.Length * sizeof(short)]);
        int samplesRead = vorbisReader.ReadSamples(readBuffer, 0, readBuffer.Length);
        for (int i = 0; i < samplesRead; i++) {
            var sampleShort = (short)(readBuffer[i] * short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(rawData.Slice(i * sizeof(short), sizeof(short)), sampleShort);
        }
        
        alBuffer.SetData(format, rawData, sampleRate);
        alSource.SetBuffer(alBuffer);
    }
    
    
    public void Play() {
        alSource.Stop();
        alSource.Play();
    }

    public void Dispose() {
        alSource.Dispose();
        alBuffer.Dispose();
    }
}