using System.Diagnostics;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Lighting;

public class LightCalculator
{
    
    public float lightLevel = 1f;


    [Logger.Timer]
    public static void LightChunk(Chunk chunk) {
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
                BlockData? options = GetBlockDataUntilIsNotLoaded(chunk, posoffset);
                if(options is null) continue;
                BlockData offsetBlock = options.Value;
                if(!blockFactory.IsBlockTransparent(offsetBlock)) continue;
                if(offsetBlock.GetSkyLightLevel() < 1) continue;
                if (offsetBlock.GetSkyLightLevel() == lightSource.oldLightLevel - (face == Face.BOTTOM && lightSource.oldLightLevel == 15 ? 0 : 1) ) {
                    removalSunLightQueue.Enqueue(new(posoffset, offsetBlock.GetSkyLightLevel()));
                    offsetBlock.SetSkyLightLevel(0);
                    SetBlockAndUpdateVertices(chunk,posoffset, offsetBlock);
                } else {
                    sunLightSource.Enqueue(new(posoffset, offsetBlock.GetSkyLightLevel()));
                }
            }
        }

        while (sunLightSource.TryDequeue(out LightNode lightSource)) {
            byte lightLevel = GetBlockDataUntilIsNotLoaded(chunk, lightSource.position)!.Value.GetSkyLightLevel();
            if(lightLevel < 1 || lightLevel > lightSource.lightLevel) continue;
            foreach (Face face in FaceFlagUtils.FACES) {
                Vector3D<int> posoffset = lightSource.position + FaceOffset.GetOffsetOfFace(face);
                BlockData? options = GetBlockDataUntilIsNotLoaded(chunk, posoffset);
                if(options is null) continue;
                BlockData offsetBlock = options.Value;
                byte nextLightLevel = (byte)(lightLevel - (face == Face.BOTTOM && lightLevel == 15 ? 0 : 1));
                if (offsetBlock.GetSkyLightLevel() < nextLightLevel) {
                    offsetBlock.SetSkyLightLevel(nextLightLevel);
                    SetBlockAndUpdateVertices(chunk, posoffset, offsetBlock);
                    if(nextLightLevel > 1 && blockFactory.blocks[offsetBlock.id].transparent ) sunLightSource.Enqueue(new (posoffset, nextLightLevel));
                }
            }
        }
    }

    private static void PropageSunLight(Chunk chunk) {
        BlockData[,,] blocks = chunk.blocks;
        Span<BlockData> span = Chunk.GetBlockSpan(blocks);
        if ((chunk.chunkFace & ChunkFace.EMPTYCHUNK) != 0 && chunk.position.Y >= 0) {
            for (int i = 0; i < span.Length; i++) span[i].SetSkyLightLevel(15);
            return;
        }
        
        BlockFactory blockFactory = Chunk.blockFactory!;
        Queue<LightNode> sunLightSource = new Queue<LightNode>();
        Chunk topChunk = chunk.chunksNeighbors![(int)Face.TOP];
        
        
        //reset all skylight
        for (int i = 0; i < span.Length; i++) span[i].SetSkyLightLevel(0);

        bool[,] blocked = new bool[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
        if ((topChunk.chunkFace & ChunkFace.BOTTOMOPAQUE) == 0) {
            if ((topChunk.chunkFace & ChunkFace.EMPTYCHUNK) != 0 && topChunk.position.Y >= 0) {
                for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        if (!blockFactory.blocks[blocks[x, 15, z].id].transparent) {
                            blocked[x, z] = true;
                            continue;
                        }
                        blocks[x, 15, z].SetSkyLightLevel(15);
                    }
                }
            } else {
                Debug.Assert(topChunk.chunkState >= ChunkState.LIGHTING); //Todo has fail surement parce le chunkFace à changé et est passé à Bottom transparent
                BlockData[,,] topBlocks = topChunk.blocks;
                for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        BlockData topBlock = topBlocks[x, 0, z];
                        if (topBlock.GetSkyLightLevel() > 2) {
                            blocks[x, 15, z].SetSkyLightLevel((byte)(topBlock.GetSkyLightLevel() == 15 ? 15 : (byte)(topBlock.GetSkyLightLevel() - 1)));
                        } else {
                            blocked[x, z] = true;
                        }
                    }
                }
            }
        }
        
        
        for (int y = 15; y > 0; y--) {
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    if(blocked[x,z]) continue;
                    byte sunlightValue = blocks[x, y, z].GetSkyLightLevel();
                    if(sunlightValue <= 1) continue;
                    byte nextSunlightValue = (byte)(sunlightValue - 1);
                    
                    // check neighbors for light 
                    if (x > 0 && blockFactory.blocks[blocks[x - 1, y, z].id].transparent
                              && blocks[x - 1, y, z].GetSkyLightLevel() < nextSunlightValue) {
                        blocks[x - 1,y,z].SetSkyLightLevel(nextSunlightValue);
                        sunLightSource.Enqueue(new LightNode(new (x - 1,y,z), nextSunlightValue));
                    }
                    
                    if (x < Chunk.CHUNK_SIZE - 1 && blockFactory.blocks[blocks[x + 1, y, z].id].transparent && 
                        blocks[x + 1, y, z].GetSkyLightLevel() < nextSunlightValue) {
                        blocks[x + 1,y,z].SetSkyLightLevel(nextSunlightValue);
                        sunLightSource.Enqueue(new LightNode(new (x + 1,y,z), nextSunlightValue));
                    }
                    
                    if (z > 0 && blockFactory.blocks[blocks[x, y, z - 1].id].transparent && blocks[x, y, z - 1].GetSkyLightLevel() < nextSunlightValue) {
                        blocks[x,y,z - 1].SetSkyLightLevel(nextSunlightValue);
                        sunLightSource.Enqueue(new LightNode(new (x,y,z - 1), nextSunlightValue));
                    }
                    
                    if (z < Chunk.CHUNK_SIZE - 1 && blockFactory.blocks[blocks[x, y, z + 1].id].transparent &&
                        blocks[x, y, z + 1].GetSkyLightLevel() < nextSunlightValue) {
                        blocks[x,y,z + 1].SetSkyLightLevel(nextSunlightValue);
                        sunLightSource.Enqueue(new LightNode(new (x,y,z + 1), nextSunlightValue));
                    }
                    
                    if(!blockFactory.blocks[blocks[x, y - 1, z].id].transparent) {
                        blocked[x, z] = true;
                        continue;
                    }
                    blocks[x,y - 1,z].SetSkyLightLevel(sunlightValue == 15 ? sunlightValue : nextSunlightValue);
                }
            }
            
        }

        if (sunLightSource.Count > 0) {
            PropageSunlightSource(chunk, sunLightSource);
        }
        
    }

    public static bool IsChunkOkToGenerateLightBelow(Chunk chunk) {
        Debug.Assert(chunk.chunkState >= ChunkState.BLOCKGENERATED);
        return (chunk.chunkState >= ChunkState.LIGHTING) || 
               ((chunk.chunkFace & ChunkFace.BOTTOMOPAQUE) != 0) || (
                   (chunk.chunkFace & ChunkFace.EMPTYCHUNK) != 0 &&
                   chunk.position.Y >= 0
               );
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

    [Logger.Timer]
    private static void PropageSunlightSource(Chunk chunk, Queue<LightNode> lightSources) {
        BlockFactory blockFactory = Chunk.blockFactory!;
        while (lightSources.TryDequeue(out LightNode lightSource)) {
            byte lightLevel = chunk.GetBlockData(lightSource.position).GetSkyLightLevel();
            if(lightLevel < 1 || lightLevel > lightSource.lightLevel) continue;
            foreach (Face face in FaceFlagUtils.FACES) {
                Vector3D<int> posoffset = lightSource.position + FaceOffset.GetOffsetOfFace(face);
                if(posoffset.Y < 0) continue;
                BlockData? options = GetBlockDataUntilIsNotLoaded(chunk, posoffset);
                if(options is null) continue;
                BlockData offsetBlock = options.Value;
                byte nextLightLevel = (byte)(lightLevel - (face == Face.BOTTOM && lightLevel == 15 ? 0 : 1));
                if (offsetBlock.GetSkyLightLevel() < nextLightLevel) {
                    offsetBlock.SetSkyLightLevel(nextLightLevel);
                    SetBlockAndUpdateVertices(chunk, posoffset, offsetBlock);
                    if(nextLightLevel > 1 && blockFactory.blocks[offsetBlock.id].transparent ) lightSources.Enqueue(new (posoffset, nextLightLevel));
                }
            }
        }
    }

    private static BlockData? GetBlockDataUntilIsNotLoaded(Chunk chunk, Vector3D<int> position) {
        if (position.X < -16 || position.X >= 32 ||
            position.Y < -16 || position.Y >= 32 ||
            position.Z < -16 || position.Z >= 32) {
            Chunk farChunk = chunk.chunkManager.GetChunk(chunk.position + World.GetChunkPosition(position));
            if(farChunk.chunkState < ChunkState.LIGHTING) return null;
            return farChunk.GetBlockData(World.GetLocalPosition(position));
        } else {
            return chunk.GetBlockData(position);
        }
    }
    
    
    private static void SetBlockAndUpdateVertices(Chunk chunk, in Vector3D<int> pos, in BlockData blockData) {
        if (pos.X < -16 || pos.X >= 32 ||
            pos.Y < -16 || pos.Y >= 32 ||
            pos.Z < -16 || pos.Z >= 32) {
            Chunk farChunk = chunk.chunkManager.GetChunk(chunk.position + World.GetChunkPosition(pos));
            if (farChunk.chunkState < ChunkState.LIGHTING) {
                Console.WriteLine("error lighting farChunk");
                return;
            }
            Vector3D<int> localPos = World.GetLocalPosition(pos);
            farChunk.blocks[localPos.X, localPos.Y, localPos.Z] = blockData;
            return;
        }

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
            chunk.chunksNeighbors![(int)faceExtended].blocks[ x, y, z] = blockData;
            chunk.chunksNeighbors![(int)faceExtended].chunkStrategy.UpdateChunkVertex();
            
        } else {
            chunk.blocks[pos.X, pos.Y, pos.Z] = blockData;
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
            if(faceExtended is not null) chunk.chunksNeighbors![(int)faceExtended].UpdateChunkVertex();
        }
    }

    private static void GetLightSourcesOfChunk(Chunk chunk, Queue<LightNode> lightSources) {
        BlockFactory blockFactory = Chunk.blockFactory!;
        BlockData[,,] blocks = chunk.blocks;
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
                byte lightLevelSource = chunk.chunksNeighbors![(int)Face.TOP].blocks[i, 0, j].GetLightLevel();
                BlockData block = chunk.GetBlockData(new(i, 15, j));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.blocks[i,15,j] = block;
                    lightSources.Enqueue(new(new Vector3D<int>(i, 15, j), lightLevelSource));
                }
                
                //Bottom
                lightLevelSource = chunk.chunksNeighbors![(int)Face.BOTTOM].blocks[i, 15, j].GetLightLevel();
                block = chunk.GetBlockData(new(i, 0, j));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.blocks[i,0,j] = block;
                    lightSources.Enqueue(new(new Vector3D<int>(i, 0, j), lightLevelSource));
                }
                
                //Left
                lightLevelSource = chunk.chunksNeighbors![(int)Face.LEFT].blocks[15, i, j].GetLightLevel();
                block = chunk.GetBlockData(new(0, i, j));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.blocks[0,i,j] = block;
                    lightSources.Enqueue(new(new Vector3D<int>(0, i, j), lightLevelSource));
                }
                
                
                //Right
                lightLevelSource = chunk.chunksNeighbors![(int)Face.RIGHT].blocks[0, i, j].GetLightLevel();
                block = chunk.GetBlockData(new(15, i, j));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.blocks[15,i,j] = block;
                    lightSources.Enqueue(new(new Vector3D<int>(15, i, j), lightLevelSource));
                }
                
                //Front
                lightLevelSource = chunk.chunksNeighbors![(int)Face.FRONT].blocks[i, j, 0].GetLightLevel();
                block = chunk.GetBlockData(new(i, j, 15));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.blocks[i,j,15] = block;
                    lightSources.Enqueue(new(new Vector3D<int>(i, j, 15), lightLevelSource));
                }
                
                //Back
                lightLevelSource = chunk.chunksNeighbors![(int)Face.BACK].blocks[i, j, 15].GetLightLevel();
                block = chunk.GetBlockData(new(i, j, 0));
                if (lightLevelSource > 1 && block.GetLightLevel() < lightLevelSource - 1){
                    block.SetLightLevel((byte)(lightLevelSource - 1));
                    chunk.blocks[i,j,0] = block;
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