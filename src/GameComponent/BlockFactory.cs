using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent;

public class BlockFactory
{
    private static BlockFactory instance;
    
    private static readonly Object _lock = new Object();
    
    private Dictionary<string, Block> blocks = new Dictionary<string, Block>();

    private List<string> transparentBlockName = new List<string>(); 
    
    public static BlockFactory getInstance()
    {
        if (instance == null) {
            lock (_lock) {
                if (instance == null) {
                    instance = new BlockFactory();
                }
            }
        }
        return instance;
    }
    
    
    private BlockFactory()
    {
        blocks.Add(AIR_BLOCK, new Block(Vector3D<int>.Zero, AIR_BLOCK, true));
        transparentBlockName.Add(AIR_BLOCK);
        blocks.Add("stone", new Block(Vector3D<int>.Zero, "stone", false));
        blocks.Add("grass",  new Block(Vector3D<int>.Zero, "grass", false));
        blocks.Add("dirt" ,new Block(Vector3D<int>.Zero, "dirt", false));
        blocks.Add("metal", new Block(Vector3D<int>.Zero, "metal", false));
    }


    public const string  AIR_BLOCK = "airblock";    

    public Block build(Vector3D<int> position, string name)
    {
        Block block;
        if (blocks.ContainsKey(name)) {
            block = (Block)blocks[name].Clone();
        }
        else {
            block = (Block)blocks[AIR_BLOCK].Clone();
        }

        block.position = position;
        return block;
    }


    public BlockData buildData(string name)
    {
        if (blocks.ContainsKey(name)) {
            return blocks[name].toBlockData();
        }
        return blocks[AIR_BLOCK].toBlockData();
    }


    public Block buildFromBlockData(Vector3D<int> position, BlockData blockData)
    {
        return build(position, (blockData.name != null) ? blockData.name : AIR_BLOCK );
    }

    public bool isBlockTransparent(BlockData blockData)
    {
        return transparentBlockName.Any(name => name.Equals(blockData.name));
    }
}