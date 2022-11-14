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
using MinecraftCloneSilk.UI;
using Silk.NET.Maths;

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
        public Camera mainCamera { get; set; }

        public List<DebugRay> debugRays = new List<DebugRay>();

        private Scene scene;
        
        //game element
        public Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
        
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
            awake();
            startables?.Invoke();
            
        }


        public void update(double deltaTime)
        {
            updatables?.Invoke( deltaTime);
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


            drawables?.Invoke(gl, deltaTime);
            
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
