﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class BlockFactory
{
    private static BlockFactory? instance;
    
    private static readonly Object Lock = new Object();
    
    public Dictionary<int, Block> blocks = new Dictionary<int, Block>();

    private BitVector32 bitVectorTransparentBlock = new();

    public string GetBlockNameById(int id) => blocks.TryGetValue(id, out Block? value) ? value.name : AIR_BLOCK;

    private Dictionary<string, Block> blockNameToBlockDictionary = new Dictionary<string, Block>();

    public int GetBlockIdByName(string name) =>
        blockNameToBlockDictionary.TryGetValue(name, out Block? value) ? value.blockData.id : 0;


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

    
    private BlockFactory() {
        string[] files = Directory.GetFiles(Generated.FilePathConstants.__Blocks_json.DirectoryPath);
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

    public bool IsBlockTransparent(BlockData blockData) {
        return bitVectorTransparentBlock[blockData.id];
    }

    private void AddBlock(Block block)
    {
        blocks.Add(block.blockData.id, block);
        blockNameToBlockDictionary.Add(block.name, block);
        if (block.transparent) {
            if(block.blockData.id >= 32) throw new Exception("id of transparent block is too high");
            bitVectorTransparentBlock[block.blockData.id] = true;
        }
    }

    public void UpdateTextures() {
        foreach (Block block in blocks.Values) block.UpdateFullTexture();
    }
}