using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;


namespace MinecraftCloneSilk.src
{
    class OpenGl
    {
        public IWindow window { get; private set; }
        public IInputContext input { get; private set; }
        public GL Gl { get; private set; }
        public IKeyboard primaryKeyboard { get; private set; }
        public IMouse primaryMouse { get; private set; }

        public Game game;

        public OpenGl(Game game)
        {
            this.game = game;

            //Create a window.
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1280, 780);
            options.Title = "MinecraftCloneSilk";

            window = Window.Create(options);

            //Assign events.
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Update += OnUpdate;
            window.Closing += OnClose;

        }

        public void Run()
        {
            //Run the window.
            window.Run();
        }

        private unsafe  void OnLoad()
        {
            //Set-up input context.
            input = window.CreateInput();
            primaryKeyboard = input.Keyboards.FirstOrDefault();
            primaryMouse = input.Mice.FirstOrDefault();

            if (primaryKeyboard != null)
            {
                primaryKeyboard.KeyDown += KeyDown;
            }
            
            Gl = GL.GetApi(window);

            game.start(Gl);
        }

        private void OnUpdate(double deltaTime)
        {
            game.update(deltaTime);
        }

        private unsafe void OnRender(double delta)
        {
            Gl.Enable(EnableCap.DepthTest);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            game.draw(Gl, delta);
        }


        private  void OnClose()
        {
            game.dispose();
        }

        private  void KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            if (key == Key.Escape)
            {
                window.Close();
            }
        }
    }
}
