using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
namespace MinecraftCloneSilk.src.GameComponent
{
    public class TextureBlock
    {
        private class TextureBlockJson
        {
            public string name { get; set; }
            public Dictionary<Face, int[]> texture { get; set; }
        }


        public TextureBlock(string path)
        {
            string jsonString = File.ReadAllText(path);
            TextureBlockJson textureBlockJson = JsonSerializer.Deserialize<TextureBlockJson>(jsonString)!;
            


        }
    }
}
