using System.Diagnostics;
using System.Text.Json;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Core;

public class TextureManager
{
    private static TextureManager? instance;

    private static readonly Object Lock = new Object();

    public static TextureManager GetInstance()
    {
        if (instance == null) {
            lock (Lock) {
                if (instance == null) {
                    instance = new TextureManager();
                }
            }
        }
        return instance;
    }


    [Serializable]
    public class TexturesJson
    {
        public string[] texturesPath { get; set; }

        public TexturesJson(string[] texturesPath) {
            this.texturesPath = texturesPath;
        }
    }
    
    public const string PATH_TO_TEXTURES_JSON = "./Assets/textures.json";
    public Dictionary<string, Texture> textures { get; private set; }
    
    
    private  TextureManager() {
        textures = new Dictionary<string, Texture>();
    }
    
    public void Load(GL gl) {
        textures.Clear();
        string jsonString = File.ReadAllText(PATH_TO_TEXTURES_JSON);
        TexturesJson? textJson = JsonSerializer.Deserialize<TexturesJson>(jsonString);
        if (textJson == null) {
            throw new Exception("failed to load textures.json");
        }
        foreach(string filepath in textJson.texturesPath) {
            FileAttributes attr = File.GetAttributes(filepath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                string[] files = Directory.GetFiles(filepath);
                foreach(string subfilepath in files)
                {
                    if (Path.GetExtension(subfilepath).Equals(".png"))
                        textures.Add(Path.GetFileName(subfilepath), new Texture(gl,subfilepath));
                }
            } else {
                textures.Add(Path.GetFileName(filepath), new Texture(gl,filepath));
            }
        }
    }

}