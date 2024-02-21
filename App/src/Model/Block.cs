using System.Diagnostics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model
{
    public class Block : ICloneable
    {
        public Vector3D<int> position;
        public string name = "air";
        public bool airBlock = true;
        public bool transparent = true;
        public byte lightEmitting = 0;
        public BlockData blockData;
        public BlockJson? blockJson { get; private set; }
        public Texture? fullTexture { get; private set; }


        public Block(BlockJson blockJson) : this(Vector3D<int>.Zero, blockJson.name,  blockJson.transparent,blockJson.lightEmitting, blockJson.id, blockJson) {}

        public Block(Vector3D<int> position, string name = BlockFactory.AIR_BLOCK, bool transparent = true,byte lightEmitting = 0, int id = 0, BlockJson? blockJson = null)
        {
            Debug.Assert((id == 0 && BlockFactory.AIR_BLOCK.Equals(name)) || (id != 0 && !BlockFactory.AIR_BLOCK.Equals(name)));
            airBlock = id == 0; 
            this.position = position;
            this.name = name;
            this.transparent = transparent;
            this.lightEmitting = lightEmitting;
            this.blockJson = blockJson;
            blockData = new BlockData(id);
            blockData.SetLightLevel(this.lightEmitting);
        }

        public void UpdateFullTexture() {
            this.fullTexture = (!airBlock) ? TextureManager.GetInstance().textures.TryGetValue(name + ".png", out Texture? value) ? value: null : null;
        }


        public BlockData GetBlockData() => blockData;

        public override string ToString()
        {
            return "[" + position + "]" + " name : " + name;
        }

        public object Clone()
        {
            Block block = new Block(position, this.name, this.transparent,this.lightEmitting, blockData.id, this.blockJson);
            return block;
        }
    }
}
