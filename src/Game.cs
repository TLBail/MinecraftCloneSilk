using Silk.NET.Windowing;
using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.src
{
    public delegate void Update(double deltaTime);
    public delegate void Draw(GL gl,Camera camera, double deltaTime);
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
        private Cube cube;

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
            cube = new Cube(Gl);
        }

        public void update(double deltaTime)
        {
            updatables?.Invoke( deltaTime);
        }


        public void draw(GL gl, double deltaTime)
        {
           drawables?.Invoke(gl, mainCamera, deltaTime);
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
