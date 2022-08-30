using Silk.NET.Windowing;
using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using MinecraftCloneSilk.src.GameComponent;
using MinecraftCloneSilk.src.Core;

namespace MinecraftCloneSilk.src
{
    public delegate void Update(double deltaTime);
    public delegate void Draw(GL gl,double deltaTime);
    public delegate void Dispose();

    public sealed class Game
    {
        private static Game instance;
        private static readonly object _lock = new object();
        private OpenGl openGl;
        private Player player;
        public Update updatables;
        public Draw drawables;
        public Dispose disposables;
        public Camera mainCamera { get; set; }
        private World world;

        private Game()
        {
            openGl = new OpenGl(this);
        }

        public void Run()
        {
            openGl.Run();
        }


        public void start(GL Gl)
        {
            player = new Player();
            world = new World(player);

            TextureBlock texture = new TextureBlock("./Assets/blocks/json/dirt.json");
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
        }

        public void drawUI()
        {
            ImGuiNET.ImGui.ShowDemoWindow();
        }

        public void dispose()
        {
            disposables?.Invoke();
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


        public static Game getInstance()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new Game();
                    }
                }
            }
            return instance;
        }

    }
}
