using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Extensions.ImGui;
using Glfw = Silk.NET.GLFW.Glfw;

namespace MinecraftCloneSilk.Core
{
    class OpenGl
    {
        public IWindow window { get; private set; }
        public IInputContext input { get; private set; }
        public GL Gl { get; private set; }
        public IKeyboard primaryKeyboard { get; private set; }
        public IMouse primaryMouse { get; private set; }

        ImGuiController imGuiController = null;

        public Game game;

        public uint uboWorld;

        private static readonly Color CLEAR_COLOR = Color.Aqua;

        public OpenGl(Game game)
        {
            this.game = game;

            //Create a window.
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1920, 1080);
            options.Title = "MinecraftCloneSilk";

            window = Window.Create(options);

            //Assign events.
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Update += OnUpdate;
            window.Closing += OnClose;
            window.FramebufferResize += FrameBufferResize;
        }

        public void Run()
        {
            //Run the window.
            window.Run();
        }

        private unsafe void OnLoad()
        {
            //Set-up input context.
            input = window.CreateInput();
            Gl = window.CreateOpenGL();

            imGuiController = new ImGuiController(Gl, window, input);
            
            primaryKeyboard = input.Keyboards.FirstOrDefault();
            primaryMouse = input.Mice.FirstOrDefault();

            if (primaryKeyboard != null)
            {
                primaryKeyboard.KeyDown += KeyDown;
            }
            
            enableFaceCulling();
            
            game.start(Gl);
        }

        
        
        private unsafe void initUniformBuffers()
        {
            uboWorld = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.UniformBuffer, uboWorld);
            Gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)(2 * sizeof(Matrix4X4<float>)), null, GLEnum.StaticDraw);
            Gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

            Gl.BindBufferRange(BufferTargetARB.UniformBuffer, 0, uboWorld, 0, (nuint)(2 * sizeof(Matrix4X4<float>)));
        }

        private void enableFaceCulling()
        {
            initUniformBuffers();
            Gl.Enable(GLEnum.CullFace);
            Gl.CullFace(GLEnum.Front);
            Gl.FrontFace(GLEnum.CW);
        }
        
        private void OnUpdate(double deltaTime)
        {
            imGuiController.Update((float)deltaTime);
            game.update(deltaTime);
        }

        private unsafe void OnRender(double delta)
        {
            Gl.Enable(EnableCap.DepthTest);
            Gl.ClearColor(CLEAR_COLOR);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            game.draw(Gl, delta);
            game.drawUI();

            imGuiController.Render();
        }

        private void FrameBufferResize(Vector2D<int> size)
        {

            Gl.Viewport(size);
        }

        private void OnClose()
        {
            imGuiController?.Dispose();
            input?.Dispose();
            game.dispose();
            Gl?.Dispose();
        }

        private unsafe void setCursorMode(CursorModeValue cursorMode)
        {
            Glfw.GetApi().SetInputMode((WindowHandle*)window.Handle, CursorStateAttribute.Cursor, cursorMode);
        }

        private unsafe CursorModeValue getCursorMode()
        {
            return (CursorModeValue)Glfw.GetApi()
                .GetInputMode((WindowHandle*)window.Handle, CursorStateAttribute.Cursor);
        }

        private void KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            if (key == Key.Escape)
            {
                window.Close();
            }

            if (key == Key.F1)
            {
                unsafe
                {
                    setCursorMode((getCursorMode() == CursorModeValue.CursorNormal) ? CursorModeValue.CursorDisabled : CursorModeValue.CursorNormal);
                }
            }
        }
    }
}
