using Silk.NET.Windowing;
using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Logger;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.RegionDrawing;
using MinecraftCloneSilk.UI;
using Silk.NET.Maths;
using Console = MinecraftCloneSilk.UI.Console;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.GameComponent;

public delegate void Startable();
public delegate void Update(double deltaTime);
public delegate void Draw(GL gl,double deltaTime);
public delegate void DrawUi();

public sealed class Game
{
    private static Game? instance;
    private static readonly object Lock = new object();
    public OpenGl openGl { get;  }
        
    public Update? updatables;
    public Draw? drawables;
    public DrawUi? uiDrawables;
    public Startable? startables;
    public Action? stopable;

    public Camera? mainCamera { get; set; }

    private Scene scene;

    private TextureManager? textureManager;
        
        
    //game element
    public Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
    private Console? console;
        
    //frame count
    private TaskCompletionSource frameCountTaskSource = new TaskCompletionSource();
    private int frameCount;
        
    //ChunkRenderer
    public ChunkBufferObjectManager? chunkBufferObjectManager;
        
        
    private Game(Scene scene, bool start = true) {
#if DEBUG
        ChromeTrace.Init();
#endif
        this.scene = scene;
        openGl = new OpenGl(this);
    }
        

    public void Run()
    {
        openGl.Run();
    }

    public void Stop() {
        stopable?.Invoke();
        openGl.Stop();
#if DEBUG
        ChromeTrace.Dispose();
#endif
    }

    [Logger.Timer]
    public void Awake()
    {

        foreach (InitGameData data in scene.gameObjects) {
            Object[] param = new []{this}.Concat(data.pars).ToArray();
            GameObject gameObject = (GameObject)Activator.CreateInstance(
                Type.GetType(data.typeName) ?? throw new InvalidOperationException("Impossible de trouver la class " + data.typeName),
                param)!;
            gameObjects.Add(data.typeName, gameObject); 
        }
    }


    [Logger.Timer]
    public void Start(GL gl)
    {
        //load textures
        textureManager = TextureManager.GetInstance();
        textureManager.Load(gl);
        
        chunkBufferObjectManager = new ChunkBufferObjectManager(TextureManager.GetInstance().textures["spriteSheet.png"], this);
        ChunkDrawableStrategy.InitStaticMembers(chunkBufferObjectManager);

        // init shaders 
        InitShaders(gl);
        Awake();
            
        if (gameObjects.ContainsKey(typeof(Console).FullName!))
            console = (Console)gameObjects[typeof(Console).FullName!];
        startables?.Invoke();
    }

    private void InitShaders(GL gl) {
        Shader chunkShader = new Shader(gl, "./Shader/3dPosOneTextUni/VertexShader.glsl",
            "./Shader/3dPosOneTextUni/FragmentShader.glsl");
        chunkShader.Use();
        chunkShader.SetUniform("texture1", 0);
        Chunk.InitStaticMembers(chunkShader, BlockFactory.GetInstance(), gl);
    }


    [Logger.Timer]
    public void Update(double deltaTime)
    {
        try {
            updatables?.Invoke( deltaTime);
        }
        catch (GameException gameException) {
            console?.Log(gameException.ToString(), Console.LogType.ERROR);
        }
    }


    [Logger.Timer]
    public unsafe void Draw(GL gl, double deltaTime)
    {
        gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboWorld);
        System.Numerics.Matrix4x4 projectionMatrix = mainCamera!.GetProjectionMatrix();
        gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(System.Numerics.Matrix4x4), projectionMatrix);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);


        gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboWorld);
        System.Numerics.Matrix4x4 viewMatrix = mainCamera!.GetViewMatrix();
        gl.BufferSubData(BufferTargetARB.UniformBuffer, sizeof(System.Numerics.Matrix4x4), (uint)sizeof(System.Numerics.Matrix4x4), viewMatrix);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

        try{   
            drawables?.Invoke(gl, deltaTime);
        }catch (GameException gameException) {
            console?.Log(gameException.ToString(), Console.LogType.ERROR);
        }
        if(frameCountTaskSource.TrySetResult()) frameCount++;
    }

        
        
    public async Task WaitForFrame(int i) {
        int currentFrameCount = frameCount;
        while (true) {
            frameCountTaskSource = new TaskCompletionSource();
            await frameCountTaskSource.Task;
            if (frameCount - currentFrameCount == i) {
                break;
            }
        }
    }
        
    public void DrawUi()
    {
        uiDrawables?.Invoke();
    }


    public IWindow GetWindow()
    {
        return openGl.window;
    }

    public IInputContext GetInput()
    {
        return openGl.input;
    }

    public GL GetGl()
    {
        return openGl.Gl;
    }

    public IKeyboard GetKeyboard()
    {
        return openGl.primaryKeyboard;
    }

    public IMouse GetMouse()
    {
        return openGl.primaryMouse;
    }


    public static Game GetInstance(Scene? scene = null, bool run = true)
    {
        if (instance == null)
        {
            lock (Lock)
            {
                if (instance == null)
                {
                    instance = new Game(scene ?? new Scene(new List<InitGameData>()), true);
                }
            }
        }
        return instance;
    }

}