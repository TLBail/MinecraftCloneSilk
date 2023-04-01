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
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.UI;
using Silk.NET.Maths;
using Console = MinecraftCloneSilk.UI.Console;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.GameComponent
{

    public delegate void Startable();

    public delegate void Update(double deltaTime);
    public delegate void Draw(GL gl,double deltaTime);


    public delegate void DrawUI();

    public sealed class Game
    {
        private static Game instance;
        private static readonly object _lock = new object();
        public OpenGl openGl { get;  }
        
        public Update updatables;
        public Draw drawables;
        public DrawUI uiDrawables;
        public Startable startables;
        public Action stopable;
        
        
        public Camera mainCamera { get; set; }

        public List<Line> debugRays = new List<Line>();

        private Scene scene;

        private TextureManager textureManager;
        
        
        //game element
        public Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
        private Console console;
        
        //frame count
        private TaskCompletionSource frameCountTaskSource = new TaskCompletionSource();
        private int frameCount;
        
        private Game(Scene scene, bool start = true) {
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
        }

        public void awake()
        {
            foreach (InitGameData data in scene.gameObjects) {
                Object[] param = new []{this}.Concat(data.pars).ToArray();
                GameObject gameObject = (GameObject)Activator.CreateInstance(
                    Type.GetType(data.typeName) ?? throw new InvalidOperationException("Impossible de trouver la class " + data.typeName),
                    param)!;
                gameObjects.Add(data.typeName, gameObject); 
            }
        }


        public void start(GL Gl)
        {
            //load textures
            textureManager = TextureManager.getInstance();
            textureManager.load(Gl);
            
            // init shaders 
            initShaders(Gl);
            
            
            awake();
            
            if (gameObjects.ContainsKey(typeof(Console).FullName))
                console = (Console)gameObjects[typeof(Console).FullName];
            startables?.Invoke();
        }

        private void initShaders(GL Gl) {
            Shader chunkShader = new Shader(Gl, "./Shader/3dPosOneTextUni/VertexShader.glsl",
                "./Shader/3dPosOneTextUni/FragmentShader.glsl");
            chunkShader.Use();
            chunkShader.SetUniform("texture1", 0);
            Chunk.initStaticMembers(Gl, chunkShader);
        }


        public void update(double deltaTime)
        {
            try {
                updatables?.Invoke( deltaTime);
            }
            catch (GameException gameException) {
                console?.log(gameException.ToString(), Console.LogType.ERROR);
            }
        }


        public unsafe void draw(GL gl, double deltaTime)
        {

            gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboWorld);
            System.Numerics.Matrix4x4 projectionMatrix = mainCamera.GetProjectionMatrix();
            gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(System.Numerics.Matrix4x4), projectionMatrix);
            gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);


            gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboWorld);
            System.Numerics.Matrix4x4 viewMatrix = mainCamera.GetViewMatrix();
            gl.BufferSubData(BufferTargetARB.UniformBuffer, sizeof(System.Numerics.Matrix4x4), (uint)sizeof(System.Numerics.Matrix4x4), viewMatrix);
            gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

            try{   
            drawables?.Invoke(gl, deltaTime);
            }catch (GameException gameException) {
                console?.log(gameException.ToString(), Console.LogType.ERROR);
            }
            if(frameCountTaskSource.TrySetResult()) frameCount++;
        }

        
        
        public async Task waitForFrame(int i) {
            int currentFrameCount = frameCount;
            while (true) {
                frameCountTaskSource = new TaskCompletionSource();
                await frameCountTaskSource.Task;
                if (frameCount - currentFrameCount == i) {
                    break;
                }
            }
        }
        
        public void drawUI()
        {
            uiDrawables?.Invoke();
        }


        public IWindow getWindow()
        {
            return openGl.window;
        }

        public IInputContext getInput()
        {
            return openGl.input;
        }

        public GL getGL()
        {
            return openGl.Gl;
        }

        public IKeyboard getKeyboard()
        {
            return openGl.primaryKeyboard;
        }

        public IMouse getMouse()
        {
            return openGl.primaryMouse;
        }


        public static Game getInstance(Scene? scene = null, bool run = true)
        {
            if (instance == null)
            {
                lock (_lock)
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
}
