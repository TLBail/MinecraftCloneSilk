using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.UI;
using Silk.NET.Core;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing.Glfw;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;
using Console = System.Console;
using Glfw = Silk.NET.GLFW.Glfw;
using Image = SixLabors.ImageSharp.Image;
using Monitor = Silk.NET.Windowing.Monitor;
using VideoMode = Silk.NET.Windowing.VideoMode;

namespace MinecraftCloneSilk.Core
{
    public class OpenGlConfig
    {
        public bool enableVSync = false;
        public bool fullScreen = false;
        public Vector2D<int> windowSize = new Vector2D<int>(1920, 1080);

        public OpenGlConfig(bool enableVSync = false, bool fullScreen = false, Vector2D<int>? windowSize = null)
        {
            this.enableVSync = enableVSync;
            this.fullScreen = fullScreen;
            if(windowSize is not null) this.windowSize = windowSize.Value;
        }
    }
    
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
        public uint uboUi;

        public static readonly Color DEFAULT_CLEAR_COLOR = Color.Lavender;
        public Color ClearColor = DEFAULT_CLEAR_COLOR;

        private Glfw glfw;

        private bool running = true;
        
        public OpenGl(Game game, OpenGlConfig? config = null!)
        {
            this.game = game;

            //Create a window.
            var options = WindowOptions.Default;

            if (config is not null) {
                if (config.fullScreen) {
                    IMonitor mainMonitor = Monitor.GetMainMonitor(null);
                    if (mainMonitor.VideoMode.Resolution is null) {
                        options.Size = config.windowSize;
                        options.WindowState = WindowState.Normal;
                    } else {
                        options.VideoMode = mainMonitor.VideoMode;
                        options.Size = mainMonitor.VideoMode.Resolution.Value;
                        options.WindowState = WindowState.Fullscreen;
                    }
                } else {
                    options.Size = config.windowSize;
                }
            } 
            options.Title = "MinecraftCloneSilk";
            options.Samples = 4; //Anti-aliasing
            
            if(config is not null ) options.VSync = config.enableVSync;
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
            
            imGuiController = new ImGuiController(Gl, window, input, Fonts.DEFAULT_FONT_CONFIG, () => Fonts.LoadFonts());
            ImGuiPlus.SetupStyle();
            
            
            primaryKeyboard = input.Keyboards.FirstOrDefault()!;
            primaryMouse = input.Mice.FirstOrDefault()!;
            primaryKeyboard.KeyDown += KeyDown;
            
            EnableFaceCulling();
            EnableAntiAliasing();
            EnableBlending();
            
            game.Start(Gl);
        }



        private void LoadIcon() {
            DecoderOptions options = new DecoderOptions();
            options.Configuration.PreferContiguousImageBuffers = true;
            using (var img = Image.Load<Rgba32>(options, Generated.FilePathConstants.Sprite.minecraftLogo_png))
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
            
            
            uboUi = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.UniformBuffer, uboUi);
            Gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)(2 * sizeof(Matrix4X4<float>)), null, GLEnum.StaticDraw);
            Gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

            Gl.BindBufferRange(BufferTargetARB.UniformBuffer, 1, uboUi, 0, (nuint)(2 * sizeof(Matrix4X4<float>)));
        }

        private void EnableFaceCulling()
        {
            InitUniformBuffers();
            Gl.Enable(GLEnum.CullFace);
            Gl.CullFace(GLEnum.Front);
            Gl.FrontFace(GLEnum.CW);
        }
        private void EnableAntiAliasing() {
            Gl.Enable(GLEnum.Multisample);
        }
        private void EnableBlending() {
            Gl.Enable(GLEnum.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
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

        private void OnRender(double delta)
        {

            Gl.Enable(EnableCap.DepthTest);
            Gl.ClearColor(ClearColor);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            game.Draw(Gl, delta);
            game.DrawUi(Gl, delta);

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
