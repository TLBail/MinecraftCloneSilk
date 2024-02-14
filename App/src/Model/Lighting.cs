using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class Lighting
{
    private struct LightNode
    {
        public Vector3D<int> position;
        public byte lightLevel;
        public LightNode(Vector3D<int> position, byte lightLevel)
        {
            this.position = position;
            this.lightLevel = lightLevel;
        }
    }
    
    
    public float lightLevel = 1f;


    public static void UpdateLighting(Chunk chunk) {
        if(chunk.chunkData.IsOnlyOneBlock()) {
            BlockData block = chunk.chunkData.GetBlock();
            block.SetLightLevel(15);
            block.SetSkyLightLevel(15);
            chunk.chunkData.SetBlock(block); 
            return;
        }
        PropageLight(chunk);
        PropageSunLight(chunk);
        
    }

    private static void PropageSunLight(Chunk chunk) {
        BlockFactory blockFactory = Chunk.blockFactory!;
        BlockData[,,] blocks = chunk.chunkData.GetBlocks();

        Queue<LightNode> sunLightSource = new Queue<LightNode>();
        Chunk topChunk = chunk.chunksNeighbors![(int)Face.TOP];
        
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    blocks[x,y,z].SetSkyLightLevel(12);
                }
            }
        }
        return;
        
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                if (blockFactory.blocks[blocks[x, 15, z].id].transparent &&
                    topChunk.chunkData.GetBlock(x, 0, z).id == 0) {
                    BlockData offsetBlock = chunk.chunkData.GetBlock(x, 15, z);
                    offsetBlock.SetSkyLightLevel(15);
                    chunk.SetBlockData(x, 15, z, offsetBlock);
                    sunLightSource.Enqueue(new (
                        new Vector3D<int>(x, 15, z),
                            15
                    ));
                }
            }
        }
        
        while (sunLightSource.TryDequeue(out LightNode lightNode)) {
            byte skylightlevel = lightNode.lightLevel;
            
            //downward
            Vector3D<int> posoffset = lightNode.position;
            posoffset.Y--;
            if (posoffset.Y >= 0) {
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                if (offsetBlock.GetSkyLightLevel() <= skylightlevel) {
                    offsetBlock.SetSkyLightLevel((byte) (skylightlevel));
                    chunk.SetBlockData(posoffset.X, posoffset.Y, posoffset.Z, offsetBlock);
                    if(skylightlevel > 1 && blockFactory.blocks[offsetBlock.id].transparent) sunLightSource.Enqueue(new (posoffset, (byte)(skylightlevel))); // Todo end infinite loop
                }
            }
            
            //upward
            posoffset = lightNode.position;
            posoffset.Y++;
            if (posoffset.Y < Chunk.CHUNK_SIZE - 1) {
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                if (offsetBlock.GetSkyLightLevel() < skylightlevel) {
                    offsetBlock.SetSkyLightLevel((byte)(skylightlevel - 1));
                    chunk.SetBlockData(posoffset.X, posoffset.Y, posoffset.Z, offsetBlock);
                    if (skylightlevel - 1 > 1 && blockFactory.blocks[offsetBlock.id].transparent)
                        sunLightSource.Enqueue(new (posoffset, (byte)(skylightlevel - 1)));
                }
            }
            
            //left
            posoffset = lightNode.position;
            posoffset.X--;
            if (posoffset.X >= 0) {
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                if (offsetBlock.GetSkyLightLevel() < skylightlevel) {
                    offsetBlock.SetSkyLightLevel((byte)(skylightlevel - 1));
                    chunk.SetBlockData(posoffset.X, posoffset.Y, posoffset.Z, offsetBlock);
                    if (skylightlevel - 1 > 1 && blockFactory.blocks[offsetBlock.id].transparent)
                        sunLightSource.Enqueue(new (posoffset, (byte)(skylightlevel - 1)));
                }
            }
            
            //right
            posoffset = lightNode.position;
            posoffset.X++;
            if(posoffset.X < Chunk.CHUNK_SIZE - 1){
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                if (offsetBlock.GetSkyLightLevel() < skylightlevel) {
                    offsetBlock.SetSkyLightLevel((byte)(skylightlevel - 1));
                    chunk.SetBlockData(posoffset.X, posoffset.Y, posoffset.Z, offsetBlock);
                    if (skylightlevel - 1 > 1 && blockFactory.blocks[offsetBlock.id].transparent)
                        sunLightSource.Enqueue(new (posoffset, (byte)(skylightlevel - 1)));
                }
            }
            
            //front
            posoffset = lightNode.position;
            posoffset.Z--;
            if(posoffset.Z >= 0){
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                if (offsetBlock.GetSkyLightLevel() < skylightlevel) {
                    offsetBlock.SetSkyLightLevel((byte)(skylightlevel - 1));
                    chunk.SetBlockData(posoffset.X, posoffset.Y, posoffset.Z, offsetBlock);
                    if (skylightlevel - 1 > 1 && blockFactory.blocks[offsetBlock.id].transparent)
                        sunLightSource.Enqueue(new (posoffset, (byte)(skylightlevel - 1)));
                }
            }
            
            //back
            posoffset = lightNode.position;
            posoffset.Z++;
            if(posoffset.Z < Chunk.CHUNK_SIZE - 1){
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                if (offsetBlock.GetSkyLightLevel() < skylightlevel) {
                    offsetBlock.SetSkyLightLevel((byte)(skylightlevel - 1));
                    chunk.SetBlockData(posoffset.X, posoffset.Y, posoffset.Z, offsetBlock);
                    if (skylightlevel - 1 > 1 && blockFactory.blocks[offsetBlock.id].transparent)
                        sunLightSource.Enqueue(new (posoffset, (byte)(skylightlevel - 1)));
                }
            }

        }
    }

    private static void PropageLight(Chunk chunk) {
        BlockData[,,] blocks = chunk.chunkData.GetBlocks();
        BlockFactory blockFactory = Chunk.blockFactory!;
        Queue<Vector3D<int>> lightSources = new Queue<Vector3D<int>>();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    byte lightLevel = blockFactory.blocks[blocks[x, y, z].id].lightEmitting;
                    blocks[x,y,z].SetLightLevel(lightLevel); 
                    if(lightLevel > 1) {
                        lightSources.Enqueue(new Vector3D<int>(x,y,z));
                    }
                }
            }
        }

        
        while (lightSources.TryDequeue(out Vector3D<int> lightSource)) {
            byte lightLevel = chunk.GetBlockData(lightSource).GetLightLevel();
            foreach (Face face in FaceFlagUtils.FACES) {
                Vector3D<int> posoffset = lightSource + FaceOffset.GetOffsetOfFace(face);
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                if (offsetBlock.GetLightLevel() < lightLevel - 1) {
                    offsetBlock.SetLightLevel((byte) (lightLevel - 1));
                    chunk.SetBlockData(posoffset.X, posoffset.Y, posoffset.Z, offsetBlock);
                    if(lightLevel - 1 > 1 && blockFactory.blocks[offsetBlock.id].transparent ) lightSources.Enqueue(posoffset);
                }
            }
        }
    }
}