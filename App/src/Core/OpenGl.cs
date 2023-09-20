﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Core;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Extensions.ImGui;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;
using Glfw = Silk.NET.GLFW.Glfw;
using Image = SixLabors.ImageSharp.Image;

namespace MinecraftCloneSilk.Core
{
    public class OpenGl
    {
        public IWindow window { get; private set; }
        public IInputContext input { get; private set; } = null!;
        public GL Gl { get; private set; } = null!;
        public IKeyboard primaryKeyboard { get; private set; } = null!;
        public IMouse primaryMouse { get; private set; } = null!;

        ImGuiController imGuiController = null!;

        public Game game;

        public uint uboWorld;

        private static readonly Color ClearColor = Color.Lavender;

        private Glfw glfw;

        private bool running = true;
        private const string PATHICON = "Assets/minecraftLogo.png";
        
        public OpenGl(Game game)
        {
            this.game = game;

            //Create a window.
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1920, 1080);
            options.Title = "MinecraftCloneSilk";
            options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(4, 3));
            window = Window.Create(options);

            //Assign events.
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Update += OnUpdate;
            window.Closing += OnClose;
            window.FramebufferResize += FrameBufferResize;
            
            glfw = Glfw.GetApi();
        }

        public void Run()
        {
            //Run the window.
            window.Run();
        }

        public void Stop() {
            running = false;
        }

        private unsafe void OnLoad()
        {
            //Set-up input context.
            input = window.CreateInput();
            Gl = window.CreateOpenGL();
            Gl.GetInteger(GetPName.MajorVersion, out int major);
            Gl.GetInteger(GetPName.MinorVersion, out int minor);
            Console.WriteLine("OpenGl Version : "  + major + "." + minor);
            byte* bytePtr = Gl.GetString(StringName.Renderer);
            List<byte> vendor = new List<byte>();
            while (*bytePtr != 0) {
                vendor.Add(*bytePtr);
                bytePtr++;
            }
            Console.WriteLine("Vendor : " + Encoding.ASCII.GetString(vendor.ToArray()) );
            LoadIcon();
            
            imGuiController = new ImGuiController(Gl, window, input);
            
            primaryKeyboard = input.Keyboards.FirstOrDefault()!;
            primaryMouse = input.Mice.FirstOrDefault()!;
            primaryKeyboard.KeyDown += KeyDown;
            
            EnableFaceCulling();
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            game.Start(Gl);
        }

        private void LoadIcon() {
            Configuration configuration = Configuration.Default;
            configuration.PreferContiguousImageBuffers = true;
            using (var img = Image.Load<Rgba32>(configuration, PATHICON))
            {
                img.DangerousTryGetSinglePixelMemory(out var imageSpan);
                var imageBytes = MemoryMarshal.AsBytes(imageSpan.Span).ToArray();
                RawImage[] iconsApp = new[] { new RawImage(img.Width, img.Height, imageBytes)};
                ReadOnlySpan<RawImage> span = new ReadOnlySpan<RawImage>(iconsApp); 
                window.SetWindowIcon(span);
            }
        }



        private unsafe void InitUniformBuffers()
        {
            uboWorld = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.UniformBuffer, uboWorld);
            Gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)(2 * sizeof(Matrix4X4<float>)), null, GLEnum.StaticDraw);
            Gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

            Gl.BindBufferRange(BufferTargetARB.UniformBuffer, 0, uboWorld, 0, (nuint)(2 * sizeof(Matrix4X4<float>)));
        }

        private void EnableFaceCulling()
        {
            InitUniformBuffers();
            Gl.Enable(GLEnum.CullFace);
            Gl.CullFace(GLEnum.Front);
            Gl.FrontFace(GLEnum.CW);
        }
        
        private void OnUpdate(double deltaTime)
        {

            if (!running) {
                CloseWindow();
                return;
            }
            imGuiController.Update((float)deltaTime);
            game.Update(deltaTime);
        }

        private unsafe void OnRender(double delta)
        {

            Gl.Enable(EnableCap.DepthTest);
            Gl.ClearColor(ClearColor);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            game.Draw(Gl, delta);
            game.DrawUi();

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
            Gl?.Dispose();
        }

        public unsafe void SetCursorMode(CursorModeValue cursorMode)
        {
            glfw.SetInputMode((WindowHandle*)window.Handle, CursorStateAttribute.Cursor, cursorMode);
        }

        public bool CursorIsNotAvailable() => GetCursorMode() != CursorModeValue.CursorNormal;
        
        public unsafe CursorModeValue GetCursorMode()
        {
            return (CursorModeValue)glfw
                .GetInputMode((WindowHandle*)window.Handle, CursorStateAttribute.Cursor);
        }

        private void CloseWindow() {
            window.Close();
        }

        private void KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            if (key == Key.Escape)
            {
                game.Stop();
                return;
            }

            if (key == Key.F1)
            {
                unsafe
                {
                    SetCursorMode((GetCursorMode() == CursorModeValue.CursorNormal) ? CursorModeValue.CursorDisabled : CursorModeValue.CursorNormal);
                }
            }
        }
    }
}