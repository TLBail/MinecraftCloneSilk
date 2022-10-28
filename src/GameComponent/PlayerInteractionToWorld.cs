using System.Numerics;
using MinecraftCloneSilk.Collision;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent;

public class PlayerInteractionToWorld
{
    private World world;
    private Player player;
    private Game game;
    private Block? block;
    private Chunk? chunk;
    
    private static readonly Vector3D<float> minChunk = new Vector3D<float>(-0.5f, -0.5f, -0.5f);

    private static readonly Vector3D<float> maxChunk =
        new Vector3D<float>(0.5f + Chunk.CHUNK_SIZE, 0.5f + Chunk.CHUNK_SIZE, 0.5f + Chunk.CHUNK_SIZE);

    private static readonly Vector3D<float> minBlock = new Vector3D<float>(-0.5f, -0.5f, -0.5f);
    private static readonly Vector3D<float> maxBlock = new Vector3D<float>(0.5f, 0.5f, 0.5f);

    
    public PlayerInteractionToWorld(World world, Player player, Game game)
    {
        this.world = world;
        this.player = player;
        this.game = game;
    }



    public void updateBlock()
    {
        Vector3D<float> playerPositionf = new Vector3D<float>(player.position.X, player.position.Y, player.position.Z);
        Vector3D<int> playerPosition = new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z);
        Ray ray = new Ray(playerPositionf, player.getDirection3D());
        List<Vector3D<float>> hitedChunks = new List<Vector3D<float>>();

        Vector3D<int> chunkToTestPosition =
            World.getChunkPosition(playerPosition) + new Vector3D<int>((int)-Chunk.CHUNK_SIZE, (int)-Chunk.CHUNK_SIZE, (int)-Chunk.CHUNK_SIZE);

        const int RADIUS = 1;
        for (int x = 0; x < 2 *RADIUS; x++) {
            for (int y = 0; y < 2 * RADIUS; y++) {
                for (int z = 0; z < 2 * RADIUS; z++) {
                    Vector3D<int> key = chunkToTestPosition + new Vector3D<int>((int)(x * Chunk.CHUNK_SIZE), (int)(y * Chunk.CHUNK_SIZE),
                        (int)(z * Chunk.CHUNK_SIZE));
                    Vector3D<float> positionf =   new Vector3D<float>(chunkToTestPosition.X +  (x * Chunk.CHUNK_SIZE),
                        chunkToTestPosition.Y + (y * Chunk.CHUNK_SIZE),
                        chunkToTestPosition.Z + (z * Chunk.CHUNK_SIZE));
                    AABBCube cube = new AABBCube(minChunk + positionf, maxChunk + positionf);
                    if (cube.intersect(ray, Chunk.CHUNK_SIZE)) {
                        Chunk chunk = world.getChunk(new Vector3D<int>((int)positionf.X,
                            (int)positionf.Y, (int)positionf.Z));
                        if(chunk != null) hitedChunks.Add(positionf);
                    }        
                }
            }
        }
        

        foreach (Vector3D<float> hitedChunkPosition in hitedChunks) {
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        ray = new Ray(playerPositionf, player.getDirection3D());
                        AABBCube cube = new AABBCube(hitedChunkPosition +  minBlock + new Vector3D<float>(x, y, z),
                            hitedChunkPosition + maxBlock + new Vector3D<float>(x, y, z));
                        if (cube.intersect(ray, Chunk.CHUNK_SIZE * 2)) {
                            Chunk hitedChunk = world.getChunk(new Vector3D<int>((int)hitedChunkPosition.X, (int)hitedChunkPosition.Y, (int)hitedChunkPosition.Z));
                            if (!hitedChunk.getBlock(x, y, z).airBlock) {
                                chunk = hitedChunk;
                                block = hitedChunk.getBlock(x, y, z);
                                return;   
                            }
                        }
                    }
                }
            }
        }

        chunk = null;
        block = null;

    }


    public Block? getBlock()
    {
        updateBlock();
        return block;
    }

    public Chunk? getChunk()
    {
        updateBlock();
        return chunk;
    }
}