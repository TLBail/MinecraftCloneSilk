using System.Numerics;
using MinecraftCloneSilk.Collision;
using Silk.NET.Maths;
using Plane = MinecraftCloneSilk.Collision.Plane;

namespace MinecraftCloneSilk.GameComponent;

public class PlayerInteractionToWorld
{
    private World world;
    private Player player;
    private Block block;
    private Chunk chunk;
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
        Ray ray = new Ray(playerPositionf, player.getDirection3D());

        const int maxDistance = 16;
        for (int distance = 0; distance < maxDistance; distance++) {
             Vector3D<int> position = ray.projectToBlock(distance);
             Chunk chunkTested = world.getChunk(position);
             if(chunkTested == null) continue;
             Block testedBlock =  chunkTested.getBlock(World.getLocalPosition(position));
             if (!testedBlock.airBlock) {
                 chunk = chunkTested;
                 block = testedBlock;
                 break;
             }
        }

        
        if(haveHitedBlock()) updateFace(ray);


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
        float zmNormal = 0;
        collidingPlaneBackFace.intersect(ray, ref zmNormal);

            //z+
        Plane collidingPlaneFrontFace = new Plane(
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X, rblock.position.Y, rblock.position.Z + 0.5f)
        );
        float zpNormal = 0;
        collidingPlaneFrontFace.intersect(ray, ref zpNormal);

        //x-
        Plane collidingPlaneLeftFace = new Plane(
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y, rblock.position.Z)
        );
        float xmNormal = 0;
        collidingPlaneLeftFace.intersect(ray, ref xmNormal);
        
        //x+
        Plane collidingPlaneRightFace = new Plane(
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y, rblock.position.Z)
        );
        float xpNormal = 0;
        collidingPlaneRightFace.intersect(ray, ref xpNormal);
        
        //y-
        Plane collidingPlaneBottomFace = new Plane(
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X, rblock.position.Y - 0.5f, rblock.position.Z)
        );
        float ymNormal = 0;
        collidingPlaneBottomFace.intersect(ray, ref ymNormal);
        
        //y+
        Plane collidingPlaneTopFace = new Plane(
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X + 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
            new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z + 0.5f),
            new Vector3D<float>(rblock.position.X, rblock.position.Y + 0.5f, rblock.position.Z)
        );
        float ypNormal = 0;
        collidingPlaneTopFace.intersect(ray, ref ypNormal);

        
        //z-
        if (zmNormal > 0 && zmNormal > zpNormal && zmNormal > xmNormal && zmNormal > xpNormal && zmNormal > ymNormal && zmNormal > ypNormal) {
            face = Face.BACK;
        }

        //z+
        if (zpNormal > 0 && zpNormal > zmNormal && zpNormal > xmNormal && zpNormal > xpNormal && zpNormal > ymNormal && zpNormal > ypNormal) {
            face = Face.FRONT;
        }

        //x-
        if (xmNormal > 0 && xmNormal > xpNormal && xmNormal > ymNormal && xmNormal > ypNormal && xmNormal > zmNormal && xmNormal > zpNormal) {
            face = Face.LEFT;
        }

        //x+
        if (xpNormal > 0 && xpNormal > xmNormal && xpNormal > ymNormal && xpNormal > ypNormal && xpNormal > zmNormal && xpNormal > zpNormal) {
            face = Face.RIGHT;
        }

        //y-
        if (ymNormal > 0 && ymNormal > ypNormal && ymNormal > xmNormal && ymNormal > xpNormal && ymNormal > zmNormal && ymNormal > zpNormal) {
            face = Face.BOTTOM;
        }

        //y+
        if (ypNormal > 0 && ypNormal > ymNormal && ypNormal > xmNormal && ypNormal > xpNormal && ypNormal > zmNormal && ypNormal > zpNormal) {
            face = Face.TOP;
        }
    }


    public bool haveHitedBlock()
    {
        updateBlock();
        return block != null;
    }
    
    public Block getBlock()
    {
        updateBlock();
        return block;
    }

    public Chunk getChunk()
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