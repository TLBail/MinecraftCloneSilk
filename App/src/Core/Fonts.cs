using System.Diagnostics;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace MinecraftCloneSilk.Core;

public static class Fonts
{
    
    public static readonly string[] FONTS_PATH = [
        "./Assets/fonts/cousine.ttf",
        "./Assets/fonts/Roboto-Medium.ttf"
    ];
    
    public const string DEFAULT_FONT_PATH = "./Assets/fonts/Minecraftia-Regular.ttf";
    
    public static readonly ImGuiFontConfig DEFAULT_FONT_CONFIG = new ImGuiFontConfig(Fonts.DEFAULT_FONT_PATH, 24);
    
    

    public static void LoadFonts() {
        fonts = new Dictionary<string, ImFontPtr>(FONTS_PATH.Length);
        ImGuiIOPtr io = ImGui.GetIO();
        foreach (string path in FONTS_PATH) {
            Debug.Assert(Path.Exists(path), $"Path to font {path} doesn't exist");
            var ptr = io.Fonts.AddFontFromFileTTF(path, 24.0f);
            fonts.Add(Path.GetFileNameWithoutExtension(path), ptr);
        }
    }
    
    public static Dictionary<string, ImFontPtr> fonts;
    
    public static void PushFont(string name) {
        ImGui.PushFont(fonts[name]);
    }
    
    
}