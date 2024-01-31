using System.Collections.ObjectModel;
using System.Text.Json;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class BlockFactory
{
    private static BlockFactory? instance;
    
    private static readonly Object Lock = new Object();
    
    private Dictionary<int, Block> blocks = new Dictionary<int, Block>();

    private List<int> transparentBlockId = new List<int>();

    public string GetBlockNameById(int id) => blocks.TryGetValue(id, out Block? value) ? value.name : AIR_BLOCK;

    private Dictionary<string, Block> blockNameToBlockDictionary = new Dictionary<string, Block>();

    public int GetBlockIdByName(string name) =>
        blockNameToBlockDictionary.TryGetValue(name, out Block? value) ? value.blockData.id : 0;

    public ReadOnlyDictionary<int, Block> blocksReadOnly => new ReadOnlyDictionary<int, Block>(blocks);

    public static BlockFactory GetInstance()
    {
        if (instance == null) {
            lock (Lock) {
                if (instance == null) {
                    instance = new BlockFactory();
                }
            }
        }
        return instance;
    }
    

    public const string  AIR_BLOCK = "airblock";
    public const  int AIR_BLOCK_ID = 0;
    private const string PATH_TO_JSON = "./Assets/blocks/json/";

    
    private BlockFactory() {
        string[] files = Directory.GetFiles(PATH_TO_JSON);
        AddBlock(new Block(Vector3D<int>.Zero));
        foreach(string filepath in files)
        {
            string jsonString = File.ReadAllText(filepath);
            BlockJson blockJson = JsonSerializer.Deserialize<BlockJson>(jsonString)!;
            AddBlock(new Block(blockJson));
        }
    }


    public Block Build(Vector3D<int> position, int id)
    {
        Block block;
        if (blocks.TryGetValue(id, out var blockModel)) {
            block = (Block)blockModel.Clone();
        }
        else {
            block = (Block)blocks[AIR_BLOCK_ID].Clone();
        }

        block.position = position;
        return block;
    }


    public BlockData GetBlockData(string name)
    {
        if (blockNameToBlockDictionary.TryGetValue(name, out var value)) {
            return value.GetBlockData();
        }
        return blocks[AIR_BLOCK_ID].GetBlockData();
    }

    public BlockData GetBlockData(int id) {
        return blocks.TryGetValue(id, out Block? block) ? block.blockData : blocks[AIR_BLOCK_ID].blockData;
    }


    public Block BuildFromBlockData(Vector3D<int> position, BlockData blockData)
    {
        return Build(position, blockData.id );
    }

    public bool IsBlockTransparent(BlockData blockData)
    {
        for (var i = 0; i < transparentBlockId.Count; i++) {
            if (transparentBlockId[i].Equals(blockData.id)) return true;
        }
        return false;
    }

    private void AddBlock(Block block)
    {
        blocks.Add(block.blockData.id, block);
        blockNameToBlockDictionary.Add(block.name, block);
        if (block.transparent) {
            transparentBlockId.Add(block.blockData.id);
        }
    }

    public void UpdateTextures() {
        foreach (Block block in blocks.Values) block.UpdateFullTexture();
    }
}