using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent;

public class BlockFactory
{
    private static BlockFactory instance;
    
    private static readonly Object _lock = new Object();
    
    private Dictionary<string, Block> blocks = new Dictionary<string, Block>();

    private List<int> transparentBlockId = new List<int>();

    private Dictionary<int, string> blockIdToNameDictionnary;
    public string getBlockNameById(int id) => blockIdToNameDictionnary[id];

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
        blockIdToNameDictionnary = new Dictionary<int, string>();
        addBlock(new Block(Vector3D<int>.Zero, AIR_BLOCK, true));
        addBlock(new Block(Vector3D<int>.Zero, "stone", false));
        addBlock(new Block(Vector3D<int>.Zero, "grass", false));
        addBlock(new Block(Vector3D<int>.Zero, "dirt", false));
        addBlock(new Block(Vector3D<int>.Zero, "metal", false));
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
        return build(position, (blockData.id != 0) ? getBlockNameById(blockData.id) : AIR_BLOCK );
    }

    public bool isBlockTransparent(BlockData blockData)
    {
        return transparentBlockId.Any(id => blockData.Equals(blockData.id));
    }

    private void addBlock(Block block)
    {
        blocks.Add(block.name, block);
        blockIdToNameDictionnary.Add(block.name.GetHashCode(), block.name);
        if (block.transparent) {
            transparentBlockId.Add(block.name.GetHashCode());
        }
    }
}