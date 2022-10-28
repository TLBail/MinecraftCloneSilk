using System.Numerics;
using MinecraftCloneSilk.Collision;
using Silk.NET.Maths;
using Plane = MinecraftCloneSilk.Collision.Plane;

namespace MinecraftCloneSilk.GameComponent;

public class PlayerInteractionToWorld
{
    private World world;
    private Player player;
    private Block? block;
    private Chunk? chunk;
    private Face? face;
    
    
    private static readonly Vector3D<float> minChunk = new Vector3D<float>(-0.5f, -0.5f, -0.5f);

    private static readonly Vector3D<float> maxChunk =
        new Vector3D<float>(0.5f + Chunk.CHUNK_SIZE, 0.5f + Chunk.CHUNK_SIZE, 0.5f + Chunk.CHUNK_SIZE);

    private static readonly Vector3D<float> minBlock = new Vector3D<float>(-0.5f, -0.5f, -0.5f);
    private static readonly Vector3D<float> maxBlock = new Vector3D<float>(0.5f, 0.5f, 0.5f);

    private bool haveUpdated = false;
    
    public PlayerInteractionToWorld(World world, Player player)
    {
        this.world = world;
        this.player = player;
        Game.getInstance().updatables += Updatables;
    }

    private void Updatables(double deltatime)
    {
        haveUpdated = false;
    }


    public void updateBlock()
    {
        if(haveUpdated) return;
        haveUpdated = true;
        
        
        chunk = null;
        block = null;
        face = null;
        Vector3D<float> playerPositionf = new Vector3D<float>(player.position.X, player.position.Y, player.position.Z);
        Vector3D<int> playerPosition = new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z);
        Ray ray = new Ray(playerPositionf, player.getDirection3D());
        List<Vector3D<float>> hitedChunks = new List<Vector3D<float>>();

        Vector3D<int> chunkToTestPosition =
            World.getChunkPosition(playerPosition) + new Vector3D<int>((int)-Chunk.CHUNK_SIZE, (int)-Chunk.CHUNK_SIZE, (int)-Chunk.CHUNK_SIZE);

        const int RADIUS = 3;
        for (int x = 0; x < RADIUS; x++) {
            for (int y = 0; y <  RADIUS; y++) {
                for (int z = 0; z < RADIUS; z++) {
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
            
            //TODO prendre que les blocs qui sont proche de player position
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        if (block.HasValue) {
                            if (MathF.Abs( Vector3D.Distance(playerPositionf, hitedChunkPosition + new Vector3D<float>(x, y, z)) )>
                                MathF.Abs(Vector3D.Distance(playerPositionf, hitedChunkPosition + new Vector3D<float>(((Block)block).position.X,
                                    ((Block)block).position.Y, ((Block)block).position.Z)))) {
                                continue;
                            }
                        }
                        AABBCube cube = new AABBCube(hitedChunkPosition +  minBlock + new Vector3D<float>(x, y, z),
                            hitedChunkPosition + maxBlock + new Vector3D<float>(x, y, z));
                        if (cube.intersect(ray, Chunk.CHUNK_SIZE * 2)) {
                            Chunk hitedChunk = world.getChunk(new Vector3D<int>((int)hitedChunkPosition.X, (int)hitedChunkPosition.Y, (int)hitedChunkPosition.Z));
                            if (!hitedChunk.getBlock(x, y, z).airBlock) {
                                chunk = hitedChunk;
                                block = hitedChunk.getBlock(x, y, z);
                            }
                        }
                    }
                }
            }
        }

        if(block.HasValue) updateFace(ray);


    }


    private void updateFace(Ray ray)
    {
        //z-
        Block rblock = (Block)block;
        Plane collidingPlaneBackFace = new Plane(
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X, rblock.position.Y, rblock.position.Z - 0.5f)
        );
        if (collidingPlaneBackFace.intersect(ray, 16.0f)) {
            face = Face.BACK;
            return;
        }
        
        
        //z+
        Plane collidingPlaneFrontFace = new Plane(
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X, rblock.position.Y, rblock.position.Z + 0.5f)
        );
        if (collidingPlaneFrontFace.intersect(ray, 16.0f)) {
            face = Face.FRONT;
            return;
        }


        //x-
        Plane collidingPlaneLeftFace = new Plane(
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y, rblock.position.Z)
        );
        if (collidingPlaneLeftFace.intersect(ray, 16.0f)) {
            face = Face.LEFT;
            return;
        }

        //x+
        Plane collidingPlaneRightFace = new Plane(
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y, rblock.position.Z)
        );
        if (collidingPlaneRightFace.intersect(ray, 16.0f)) {
            face = Face.RIGHT;
            return;
        }
        
        //y-
        Plane collidingPlaneBottomFace = new Plane(
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X, rblock.position.Y - 0.5f, rblock.position.Z)
        );
        if (collidingPlaneBottomFace.intersect(ray, 16.0f)) {
            face = Face.BOTTOM;
            return;
        }

        //y+
        Plane collidingPlaneTopFace = new Plane(
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X, rblock.position.Y + 0.5f, rblock.position.Z)
        );
        if (collidingPlaneTopFace.intersect(ray, 16.0f)) {
            face = Face.TOP;
            return;
        }


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


    public Face? getFace()
    {
        updateBlock();
        return face;
    }
}