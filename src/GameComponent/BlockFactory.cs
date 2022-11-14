using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent;

public class BlockFactory
{
    private static BlockFactory instance;
    
    private static readonly Object _lock = new Object();
    
    private Dictionary<int, Block> blocks = new Dictionary<int, Block>();

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
    

    public const string  AIR_BLOCK = "airblock";
    public readonly int AIR_BLOCK_ID;

    private BlockFactory() {
        AIR_BLOCK_ID = AIR_BLOCK.GetHashCode();
        blockIdToNameDictionnary = new Dictionary<int, string>();
        addBlock(new Block(Vector3D<int>.Zero, AIR_BLOCK, true));
        addBlock(new Block(Vector3D<int>.Zero, "stone", false));
        addBlock(new Block(Vector3D<int>.Zero, "grass", false));
        addBlock(new Block(Vector3D<int>.Zero, "dirt", false));
        addBlock(new Block(Vector3D<int>.Zero, "metal", false));
    }


    public Block build(Vector3D<int> position, int id)
    {
        Block block;
        if (blocks.ContainsKey(id)) {
            block = (Block)blocks[id].Clone();
        }
        else {
            block = (Block)blocks[AIR_BLOCK_ID].Clone();
        }

        block.position = position;
        return block;
    }


    public BlockData buildData(string name)
    {
        if (blocks.ContainsKey(name.GetHashCode())) {
            return blocks[name.GetHashCode()].toBlockData();
        }
        return blocks[AIR_BLOCK_ID].toBlockData();
    }


    public Block buildFromBlockData(Vector3D<int> position, BlockData blockData)
    {
        return build(position, blockData.id );
    }

    public bool isBlockTransparent(BlockData blockData)
    {
        for (var i = 0; i < transparentBlockId.Count; i++) {
            if (transparentBlockId[i].Equals(blockData.id)) return true;
        }
        return false;
    }

    private void addBlock(Block block)
    {
        blocks.Add(block.name.GetHashCode(), block);
        blockIdToNameDictionnary.Add(block.name.GetHashCode(), block.name);
        if (block.transparent) {
            transparentBlockId.Add(block.name.GetHashCode());
        }
    }
}