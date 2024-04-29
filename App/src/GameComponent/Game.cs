using Silk.NET.Windowing;
using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinecraftCloneSilk.Audio;
using Silk.NET.OpenGL;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Logger;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.ChunkManagement;
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
public delegate void DrawUi(GL gl, double deltaTime);

public sealed class Game
{
    private static Game? instance;
    private static readonly object Lock = new object();
    public OpenGl openGl { get;  }
        
    public Update? updatables;
    public Draw? drawables;
    public DrawUi? uiDrawables;
    public Startable? startables;

    public Camera? mainCamera { get; set; }

    private GameParameter gameParameter;
    private TextureManager? textureManager;
    private AudioMaster audioMaster;

    public Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
    public Console console;
        
    private TaskCompletionSource frameCountTaskSource = new TaskCompletionSource(); //use to wait for a frame to complete
    private int frameCount;
        
    //ChunkRenderer
    public ChunkBufferObjectManager? chunkBufferObjectManager;
        
        
    private Game(GameParameter gameParameter) {
#if DEBUG
        ChromeTrace.Init();
#endif
        this.gameParameter = gameParameter;
        openGl = new OpenGl(this, gameParameter.openGlConfig);
        console = new Console(this);
        audioMaster = AudioMaster.GetInstance();
    }
        

    public void Run()
    {
        openGl.Run();
    }

    public void Stop() {
        foreach(GameObject gameObject in gameObjects.Values) gameObject.Destroy();
        openGl.Stop();
        audioMaster.Dispose();
#if DEBUG
        ChromeTrace.Dispose();
#endif
        instance = null;
    }

    [Logger.Timer]
    public void Awake() {
        foreach (InitGameData data in gameParameter.gameObjects) {
            AddGameObject(data);
        }
    }


    [Logger.Timer]
    public void Start(GL gl)
    {
        //load textures
        textureManager = TextureManager.GetInstance();
        textureManager.Load(gl);
        BlockFactory.GetInstance().UpdateTextures();
        
        chunkBufferObjectManager = new ChunkBufferObjectManager(this,TextureManager.GetInstance().textures["spriteSheet.png"]);
        ChunkDrawableStrategy.InitStaticMembers(chunkBufferObjectManager);

        // init shaders 
        InitShaders(gl);
        Awake();
            
        startables?.Invoke();
        startables = null;
    }

    private void InitShaders(GL gl) {
        Shader chunkShader = new Shader(gl, Generated.FilePathConstants.__Shader_World.VertexShader_glsl,
            Generated.FilePathConstants.__Shader_World.FragmentShader_glsl);
        chunkShader.Use();
        chunkShader.SetUniform("texture1", 0);
        Chunk.InitStaticMembers(chunkShader, BlockFactory.GetInstance(), gl);
    }


    [Logger.Timer]
    public void Update(double deltaTime)
    {
        
        try {
            if (startables is not null) {
                startables.Invoke();
                startables = null;
            }
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
        
    public unsafe void DrawUi(GL gl, double deltaTime)
    {
        
        gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboUi);
        System.Numerics.Matrix4x4 projectionMatrix = mainCamera!.GetUiProjectionMatrix();
        gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(System.Numerics.Matrix4x4), projectionMatrix);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);


        gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboUi);
        System.Numerics.Matrix4x4 viewMatrix = mainCamera!.GetUiViewMatrix();
        gl.BufferSubData(BufferTargetARB.UniformBuffer, sizeof(System.Numerics.Matrix4x4), (uint)sizeof(System.Numerics.Matrix4x4), viewMatrix);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
 
        gl.Disable(GLEnum.DepthTest);
        uiDrawables?.Invoke(gl, deltaTime);
        gl.Enable(GLEnum.DepthTest);
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


    public static Game GetInstance(GameParameter? scene = null, bool run = true)
    {
        if (instance == null)
        {
            lock (Lock)
            {
                if (instance == null)
                {
                    instance = new Game(scene ?? new GameParameter(new List<InitGameData>(), new OpenGlConfig()));
                }
            }
        }
        return instance;
    }

    public void AddGameObject(InitGameData data) {
        Object[] param = new []{this}.Concat(data.pars).ToArray();
        GameObject gameObject = (GameObject)Activator.CreateInstance(
            Type.GetType(data.typeName) ?? throw new InvalidOperationException("Impossible de trouver la class " + data.typeName),
            param)!;
        gameObjects.Add(data.typeName, gameObject); 
    }
    
    public void AddGameObject(GameObject gameObject) {
        gameObjects.Add(gameObject.GetType().FullName!, gameObject); 
    }

    public T FindGameObject<T>() where T : GameObject {
        return (T)gameObjects[typeof(T).FullName!];
    }
}