using System.Diagnostics;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;
using Console = MinecraftCloneSilk.UI.Console;

namespace MinecraftCloneSilk.Model;

public class Lighting
{
    
    public float lightLevel = 1f;


    public static void LightChunk(Chunk chunk) {
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
    
    public static void OnBlockSet(Chunk chunk, Vector3D<int> position, BlockData oldBlockData, BlockData newBlockData) {
        BlockFactory blockFactory = Chunk.blockFactory!;
        //update torch light
        Queue<LightRemovalNode> removeLightQueue = new Queue<LightRemovalNode>();
        Queue<LightNode> addLightQueue = new Queue<LightNode>();
        if(newBlockData.GetLightLevel() > 1) addLightQueue.Enqueue(new(position, newBlockData.GetLightLevel()));

        if (oldBlockData.GetLightLevel() == 0 || oldBlockData.GetLightLevel() > newBlockData.GetLightLevel()) {
            removeLightQueue.Enqueue(new(position, oldBlockData.GetLightLevel()));
            while(removeLightQueue.TryDequeue(out LightRemovalNode lightSource)) {
                foreach (Face face in FaceFlagUtils.FACES) {
                    Vector3D<int> posoffset = lightSource.position + FaceOffset.GetOffsetOfFace(face);
                    BlockData offsetBlock = chunk.GetBlockData(posoffset);
                    if (blockFactory.blocks[offsetBlock.id].lightEmitting != 0) {
                        addLightQueue.Enqueue(new(posoffset, offsetBlock.GetLightLevel()));
                        continue;
                    }
                    if(!blockFactory.IsBlockTransparent(offsetBlock)) continue;
                    if(offsetBlock.GetLightLevel() < 1) continue;
                    if (offsetBlock.GetLightLevel() == lightSource.oldLightLevel - 1) {
                        removeLightQueue.Enqueue(new(posoffset, offsetBlock.GetLightLevel()));
                        offsetBlock.SetLightLevel(0);
                        SetBlockAndUpdateVertices(chunk,posoffset, offsetBlock);
                    } else {
                        addLightQueue.Enqueue(new(posoffset, offsetBlock.GetLightLevel()));
                    }
                }
            }
        }
        PropageLightSource(chunk, addLightQueue);
        
        
        //update sky light
        Queue<LightNode> sunLightSource = new Queue<LightNode>();
        Queue<LightRemovalNode> removalSunLightQueue = new Queue<LightRemovalNode>();
        removalSunLightQueue.Enqueue(new(position, oldBlockData.GetSkyLightLevel()));
        while(removalSunLightQueue.TryDequeue(out LightRemovalNode lightSource)) {
            foreach (Face face in FaceFlagUtils.FACES) {
                Vector3D<int> posoffset = lightSource.position + FaceOffset.GetOffsetOfFace(face);
                if (posoffset.Y < 0) {
                    //Todo say to the bottom chunk to update light
                    continue;
                }
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                if(!blockFactory.IsBlockTransparent(offsetBlock)) continue;
                if(offsetBlock.GetSkyLightLevel() < 1) continue;
                if (offsetBlock.GetSkyLightLevel() == lightSource.oldLightLevel - (face == Face.BOTTOM ? 0 : 1) ) {
                    removalSunLightQueue.Enqueue(new(posoffset, offsetBlock.GetSkyLightLevel()));
                    offsetBlock.SetSkyLightLevel(0);
                    SetBlockAndUpdateVertices(chunk,posoffset, offsetBlock);
                } else {
                    sunLightSource.Enqueue(new(posoffset, offsetBlock.GetSkyLightLevel()));
                }
            }
        }
        PropageSunlightSource(chunk, sunLightSource);
    }

    private static void PropageSunLight(Chunk chunk) {
        BlockFactory blockFactory = Chunk.blockFactory!;
        BlockData[,,] blocks = chunk.chunkData.GetBlocks();
        Queue<LightNode> sunLightSource = new Queue<LightNode>();
        Chunk topChunk = chunk.chunksNeighbors![(int)Face.TOP];
        
        
        //reset all skylight
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    blocks[x,y,z].SetSkyLightLevel(0);
                }
            }
        }
        
        //get all sunlight source of neighbor
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                if (blockFactory.blocks[blocks[x, 15, z].id].transparent &&
                    topChunk.chunkData.GetBlock(x, 0, z).id == 0) {
                    BlockData offsetBlock = chunk.chunkData.GetBlock(x, 15, z);
                    offsetBlock.SetSkyLightLevel(15);
                    chunk.SetBlockData(x, 15, z, offsetBlock);

                    if (topChunk.chunkData.GetBlock(x, 0, z).GetSkyLightLevel() != 15) {
                        BlockData topBlock = topChunk.chunkData.GetBlock(x, 0, z);
                        topBlock.SetSkyLightLevel(15);
                        topChunk.chunkData.SetBlock(topBlock,x, 0, z);
                    }
                    sunLightSource.Enqueue(new (
                        new Vector3D<int>(x, 15, z),
                        15
                    ));
                }
            }
        }
        PropageSunlightSource(chunk, sunLightSource);
        
    }

    private static void PropageLight(Chunk chunk) {
        Queue<LightNode> lightSources = new Queue<LightNode>();
        GetLightSourcesOfChunk(chunk, lightSources);
        GetLightSourcesOfNeighbors(chunk, lightSources); 
        PropageLightSource(chunk, lightSources);
    }


    private static void PropageLightSource(Chunk chunk, Queue<LightNode> lightSources) {
        BlockFactory blockFactory = Chunk.blockFactory!;
        while (lightSources.TryDequeue(out LightNode lightSource)) {
            byte lightLevel = chunk.GetBlockData(lightSource.position).GetLightLevel();
            if(lightLevel < 1 || lightLevel > lightSource.lightLevel) continue;
            foreach (Face face in FaceFlagUtils.FACES) {
                Vector3D<int> posoffset = lightSource.position + FaceOffset.GetOffsetOfFace(face);
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                if(!blockFactory.blocks[offsetBlock.id].transparent) continue;
                byte newLightLevel = (byte)(lightLevel - 1);
                if (offsetBlock.GetLightLevel() < newLightLevel) {
                    offsetBlock.SetLightLevel(newLightLevel);
                    SetBlockAndUpdateVertices(chunk, posoffset, offsetBlock);
                    if(newLightLevel > 1) lightSources.Enqueue(new (posoffset,newLightLevel));
                }
            }
        }
    }

    private static void PropageSunlightSource(Chunk chunk, Queue<LightNode> lightSources) {
        BlockFactory blockFactory = Chunk.blockFactory!;
        while (lightSources.TryDequeue(out LightNode lightSource)) {
            byte lightLevel = chunk.GetBlockData(lightSource.position).GetSkyLightLevel();
            if(lightLevel < 1 || lightLevel > lightSource.lightLevel) continue;
            foreach (Face face in FaceFlagUtils.FACES) {
                Vector3D<int> posoffset = lightSource.position + FaceOffset.GetOffsetOfFace(face);
                if(posoffset.Y < 0) continue;
                BlockData offsetBlock = chunk.GetBlockData(posoffset);
                byte nextLightLevel = (byte)(lightLevel - (face == Face.BOTTOM ? 0 : 1));
                if (offsetBlock.GetSkyLightLevel() < nextLightLevel) {
                    offsetBlock.SetSkyLightLevel(nextLightLevel);
                    SetBlockAndUpdateVertices(chunk, posoffset, offsetBlock);
                    if(nextLightLevel > 1 && blockFactory.blocks[offsetBlock.id].transparent ) lightSources.Enqueue(new (posoffset, nextLightLevel));
                }
            }
        }
    }
    private static void SetBlockAndUpdateVertices(Chunk chunk, in Vector3D<int> pos, in BlockData blockData) {
        if (BlockIsOutsideOfChunk(pos)) {
            int x = pos.X;
            int y = pos.Y;
            int z = pos.Z;
            FaceFlag faceFlag = FaceFlag.EMPTY;
            if (y < 0) {
                faceFlag |= FaceFlag.BOTTOM;
                y += (int)Chunk.CHUNK_SIZE;
            } else if (y >= Chunk.CHUNK_SIZE) {
                faceFlag |= FaceFlag.TOP;
                y -= (int)Chunk.CHUNK_SIZE;
            }
            if (x < 0) {
                faceFlag |= FaceFlag.LEFT;
                x += (int)Chunk.CHUNK_SIZE;
            } else if (x >= Chunk.CHUNK_SIZE) {
                faceFlag |= FaceFlag.RIGHT;
                x -= (int)Chunk.CHUNK_SIZE;
            }
            if (z < 0) {
                faceFlag |= FaceFlag.BACK;
                z += (int)Chunk.CHUNK_SIZE;
            } else if (z >= Chunk.CHUNK_SIZE) {
                faceFlag |= FaceFlag.FRONT;
                z -= (int)Chunk.CHUNK_SIZE;
            }
            FaceExtended faceExtended = (FaceExtended)FaceFlagUtils.GetFaceExtended(faceFlag)!;
            chunk.chunksNeighbors![(int)faceExtended].chunkData.SetBlock(blockData, x, y, z);
            chunk.chunksNeighbors![(int)faceExtended].chunkStrategy.UpdateChunkVertex();
            
        } else {
            chunk.chunkData.SetBlock(blockData,pos.X, pos.Y, pos.Z);
            FaceFlag faceFlag = FaceFlag.EMPTY;
            if (pos.Y == 0) {
                faceFlag |= FaceFlag.BOTTOM;
            } else if (pos.Y == Chunk.CHUNK_SIZE - 1) {
                faceFlag |= FaceFlag.TOP;
            }
            if (pos.X == 0) {
                faceFlag |= FaceFlag.LEFT;
            } else if (pos.X == Chunk.CHUNK_SIZE - 1) {
                faceFlag |= FaceFlag.RIGHT;
            }
            if (pos.Z == 0) {
                faceFlag |= FaceFlag.BACK;
            } else if (pos.Z == Chunk.CHUNK_SIZE - 1) {
                faceFlag |= FaceFlag.FRONT;
            }
            FaceExtended? faceExtended = FaceFlagUtils.GetFaceExtended(faceFlag);
            if(faceExtended is not null) chunk.chunksNeighbors![(int)faceExtended].chunkStrategy.UpdateChunkVertex();
        }
    }

    private static void GetLightSourcesOfChunk(Chunk chunk, Queue<LightNode> lightSources) {
        BlockFactory blockFactory = Chunk.blockFactory!;
        BlockData[,,] blocks = chunk.chunkData.GetBlocks();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    byte lightLevel = blockFactory.blocks[blocks[x, y, z].id].lightEmitting;
                    blocks[x,y,z].SetLightLevel(lightLevel); 
                    if(lightLevel > 1) {
                        lightSources.Enqueue(new(new Vector3D<int>(x,y,z), lightLevel));
                    }
                }
            }
        } 
    }

    private static void GetLightSourcesOfNeighbors(Chunk chunk, Queue<LightNode> lightSources) {
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++) {
                //Top
                byte lightLevelSource = chunk.chunksNeighbors![(int)Face.TOP].chunkData.GetBlock(i, 0, j).GetLightLevel();
                BlockData block = chunk.GetBlockData(new(i, 15, j));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.chunkData.SetBlock(block,i,15,j);
                    lightSources.Enqueue(new(new Vector3D<int>(i, 15, j), lightLevelSource));
                }
                
                //Bottom
                lightLevelSource = chunk.chunksNeighbors![(int)Face.BOTTOM].chunkData.GetBlock(i, 15, j).GetLightLevel();
                block = chunk.GetBlockData(new(i, 0, j));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.chunkData.SetBlock(block,i,0,j);
                    lightSources.Enqueue(new(new Vector3D<int>(i, 0, j), lightLevelSource));
                }
                
                //Left
                lightLevelSource = chunk.chunksNeighbors![(int)Face.LEFT].chunkData.GetBlock(15, i, j).GetLightLevel();
                block = chunk.GetBlockData(new(0, i, j));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.chunkData.SetBlock(block,0,i,j);
                    lightSources.Enqueue(new(new Vector3D<int>(0, i, j), lightLevelSource));
                }
                
                
                //Right
                lightLevelSource = chunk.chunksNeighbors![(int)Face.RIGHT].chunkData.GetBlock(0, i, j).GetLightLevel();
                block = chunk.GetBlockData(new(15, i, j));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.chunkData.SetBlock(block,15,i,j);
                    lightSources.Enqueue(new(new Vector3D<int>(15, i, j), lightLevelSource));
                }
                
                //Front
                lightLevelSource = chunk.chunksNeighbors![(int)Face.FRONT].chunkData.GetBlock(i, j, 0).GetLightLevel();
                block = chunk.GetBlockData(new(i, j, 15));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.chunkData.SetBlock(block,i,j,15);
                    lightSources.Enqueue(new(new Vector3D<int>(i, j, 15), lightLevelSource));
                }
                
                //Back
                lightLevelSource = chunk.chunksNeighbors![(int)Face.BACK].chunkData.GetBlock(i, j, 15).GetLightLevel();
                block = chunk.GetBlockData(new(i, j, 0));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.chunkData.SetBlock(block,i,j,0);
                    lightSources.Enqueue(new(new Vector3D<int>(i, j, 0), lightLevelSource));
                }
            }
        }
        
        
    }

    private static bool BlockIsOutsideOfChunk(Vector3D<int> pos) {
        return pos.X < 0 || pos.X >= Chunk.CHUNK_SIZE ||
               pos.Y < 0 || pos.Y >= Chunk.CHUNK_SIZE ||
               pos.Z < 0 || pos.Z >= Chunk.CHUNK_SIZE;
    }
    
    private struct LightNode(Vector3D<int> position, byte lightLevel)
    {
        public Vector3D<int> position = position;
        public byte lightLevel = lightLevel;
    }

    private struct LightRemovalNode(Vector3D<int> position, byte oldLightLevel)
    {
        public Vector3D<int> position = position;
        public byte oldLightLevel = oldLightLevel;
    }
    
}