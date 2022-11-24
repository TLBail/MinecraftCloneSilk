using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MinecraftCloneSilk.Core;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent
{
    public class Block : ICloneable
    {
        public Vector3D<int> position;
        public string name = "air";
        public bool airBlock = true;
        public bool transparent = true;
        public TextureBlock textureBlock;
        public Texture fullTexture { get; private set; } 

        public Block(Vector3D<int> position) : this(position, BlockFactory.AIR_BLOCK, true) { }

        public Block(Vector3D<int> position, string name) : this(position, name, false){}
        public Block(Vector3D<int> position, string name, bool transparent) : this(position, name, transparent, null){}

        
        public Block(BlockJson blockJson) : this(Vector3D<int>.Zero, blockJson.name, blockJson.transparent, new TextureBlock(blockJson)) {}

        public Block(Vector3D<int> position, string name, bool transparent, TextureBlock textureBlock)
        {
            airBlock = BlockFactory.AIR_BLOCK.Equals(name); 
            this.position = position;
            this.name = name;
            this.transparent = transparent;
            this.textureBlock = textureBlock;
            this.fullTexture = (!airBlock) ? TextureManager.getInstance().textures[name + ".png"] : null;
        }
        

        public BlockData toBlockData()
        {
            return new BlockData(name);
        }
        
        public override string ToString()
        {
            return "[" + position + "]" + " name : " + name;
        }

        public object Clone()
        {
            Block block = new Block(position, this.name);
            block.transparent = this.transparent;
            block.airBlock = this.airBlock;
            return block;
        }
    }
}
