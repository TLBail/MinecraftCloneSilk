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
        public BlockData blockData;
        public TextureBlock textureBlock;
        public Texture fullTexture { get; private set; } 

        public Block(Vector3D<int> position) : this(position, BlockFactory.AIR_BLOCK, true, 0, null) { }
        
        
        public Block(BlockJson blockJson) : this(Vector3D<int>.Zero, blockJson.name,  blockJson.transparent, blockJson.id, new TextureBlock(blockJson)) {}

        public Block(Vector3D<int> position, string name, bool transparent,int id, TextureBlock textureBlock)
        {
            airBlock = BlockFactory.AIR_BLOCK.Equals(name) || id == 0; 
            this.position = position;
            this.name = name;
            this.transparent = transparent;
            blockData = new BlockData(id);
            this.textureBlock = textureBlock;
            this.fullTexture = (!airBlock) ? TextureManager.getInstance().textures.TryGetValue(name + ".png", out Texture value) ? value: null : null;
        }


        public BlockData getBlockData() => blockData;

        public override string ToString()
        {
            return "[" + position + "]" + " name : " + name;
        }

        public object Clone()
        {
            Block block = new Block(position, this.name, this.transparent, blockData.id, this.textureBlock);
            return block;
        }
    }
}
