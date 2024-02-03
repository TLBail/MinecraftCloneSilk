using System.Buffers.Binary;
using NVorbis;
using Silk.NET.OpenAL;

namespace MinecraftCloneSilk.Audio;

public class AudioStream : IDisposable
{
    public const int NB_BUFFERS = 4;
    private VorbisReader vorbisReader;
    private int channels;
    private int sampleRate;
    private Queue<AlBuffer> buffers = new();
    private AL al;
    private AlSource alSource;
    private bool isPlaying = true;
    private BufferFormat format;
    private bool disposed = false;
    private bool loop = true;
    private Thread? thread;
    

    public AudioStream(string path, bool loop = true) {
        this.loop = loop;
        this.al = AudioMaster.GetInstance().al;
        vorbisReader = new VorbisReader(path);
        
        channels = vorbisReader.Channels;
        sampleRate = vorbisReader.SampleRate;
        format = BufferFormat.Mono16; 
        if (channels == 2)
        {
            format = BufferFormat.Stereo16; 
        }
        
        
        alSource = new AlSource();

        for (int i = 0; i < 4; i++) {
            AlBuffer alBuffer = new AlBuffer();
            bool haveNext = FillBufferWithSound(alBuffer, channels, sampleRate, format);
            buffers.Enqueue(alBuffer);
            if(!haveNext) break;
        }
        alSource.QueueBuffers(buffers.ToArray());

    }

    public void Play() {
        alSource.Play();
        if(thread is  null) {
            thread = new Thread(ThreadUpdater);
            thread.Start();
        }
    }

    public void Pause() {
        alSource.Stop();
    }

    public void Stop() => Dispose();

    private void ThreadUpdater() {
        while (isPlaying) {
            UpdateBuffer();
            Thread.Sleep(500);
        }
        Dispose();
    }

    public void UpdateBuffer() {
        al.GetSourceProperty(alSource.sourcehandle, GetSourceInteger.BuffersProcessed,
            out int processedBuffersCount);
        while (processedBuffersCount-- > 0) {
            AlBuffer buffer = buffers.Dequeue();
            alSource.UnqueueBuffer([buffer]);
            if (!FillBufferWithSound( buffer, channels, sampleRate, format)) {
                isPlaying = false; 
                break;
            }
            alSource.QueueBuffers([buffer]);
            buffers.Enqueue(buffer);
        }
    }
    
    bool FillBufferWithSound(AlBuffer buffer, int channels, int sampleRate, BufferFormat format) {
        float[] readBuffer = new float[channels * sampleRate / 5]; // Durée ajustée pour des mises à jour plus fréquentes
        Span<byte> rawData = new Span<byte>(new byte[readBuffer.Length * sizeof(short)]);
    
        int samplesRead = vorbisReader.ReadSamples(readBuffer, 0, readBuffer.Length);
        for (int i = 0; i < samplesRead; i++) {
            var sampleShort = (short)(readBuffer[i] * short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(rawData.Slice(i * sizeof(short), sizeof(short)), sampleShort);
        }
        if (loop && samplesRead < readBuffer.Length) {
            vorbisReader.SeekTo(0, SeekOrigin.Begin);
            int samplesRead2 = vorbisReader.ReadSamples(readBuffer, samplesRead, readBuffer.Length - samplesRead);
            for (int i = samplesRead; i < samplesRead + samplesRead2; i++) {
                var sampleShort = (short)(readBuffer[i] * short.MaxValue);
                BinaryPrimitives.WriteInt16LittleEndian(rawData.Slice(i * sizeof(short), sizeof(short)), sampleShort);
            }
        }
        buffer.SetData(format, rawData.ToArray(), sampleRate);
        
        if(!loop && samplesRead < readBuffer.Length) {
            return false;
        } else {
            return true;
        }
    }

    public void Dispose() {
        if (disposed) return;
        alSource.Stop();
        isPlaying = false;
        vorbisReader.Dispose();
        alSource.Dispose();
        foreach (var buffer in buffers) {
            buffer.Dispose();
        }
        disposed = true;
    }

    public void Seek(int i) {
        vorbisReader.SeekTo(i, SeekOrigin.Begin);
    }
}