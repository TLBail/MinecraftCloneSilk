using Silk.NET.OpenAL;

namespace MinecraftCloneSilk.Audio;

public class AudioMaster : IDisposable
{
    
    private static AudioMaster? instance;

    public static AudioMaster GetInstance() {
        if (instance == null) {
            instance = new AudioMaster();
        }
        return instance;
    }
    private ALContext alc;
    public AL al { get; private set; }
    private unsafe Context* context;
    private unsafe Device* device;
    private bool disposed = false;

    private unsafe AudioMaster() {
        alc = ALContext.GetApi();
        al = AL.GetApi();
        device = alc.OpenDevice("");
        if (device == null)
            throw new Exception("Could not create device");

        context = alc.CreateContext(device, null);
        MakeContextCurrent();
        GetError();
    }
    
    private unsafe void MakeContextCurrent() {
        alc.MakeContextCurrent(context);
    }

    public void GetError() {
        AudioError err = al.GetError();
        if (err != AudioError.NoError) {
            throw new Exception($"Audio error {Enum.GetName(typeof(AudioError), err)}");
        }
    }


    public unsafe void Dispose() {
        if (!this.disposed) {
            alc.DestroyContext(context);
            alc.CloseDevice(device);
            
            al.Dispose();
            alc.Dispose();
            disposed = true;
        }
        GC.SuppressFinalize(this);
    }
    
    
    
    
}