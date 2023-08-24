using MinecraftCloneSilk.Collision;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class PlayerInteractionToWorld
{
    private readonly World world;
    private readonly Player player;
    private Block? block;
    private Chunk? chunk;
    private Face? face;

    private bool haveUpdated;

    public PlayerInteractionToWorld(World world, Player player) {
        this.world = world;
        this.player = player;
        Game.GetInstance().updatables += Update;
    }

    private void Update(double deltatime) {
        haveUpdated = false;
    }


    public void UpdateBlock() {
        if (haveUpdated) {
            return;
        }

        haveUpdated = true;


        chunk = null;
        block = null;
        face = null;
        var playerPositionf = new Vector3D<float>(player.position.X, player.position.Y, player.position.Z);
        var ray = new Ray(playerPositionf, player.GetDirection3D());

        var bestHitDistance = float.MaxValue;


        const int maxDistance = 16;
        AABBCube aabbCube = new AABBCube(Vector3D<float>.Zero, Vector3D<float>.Zero);
        for (var x = -maxDistance; x < maxDistance; x++)
        for (var y = -maxDistance; y < maxDistance; y++)
        for (var z = -maxDistance; z < maxDistance; z++) {
            var blockPosition = new Vector3D<int>(
                (int)(x + Math.Round(playerPositionf.X)),
                (int)(y + Math.Round(playerPositionf.Y)),
                (int)(z + Math.Round(playerPositionf.Z))
            );
            aabbCube.bounds[0] = new Vector3D<float>(
                -0.5f + blockPosition.X,
                -0.5f + blockPosition.Y,
                -0.5f + blockPosition.Z
            );
            aabbCube.bounds[1] = new Vector3D<float>(
                0.5f + blockPosition.X,
                0.5f + blockPosition.Y,
                0.5f + blockPosition.Z
            );
            var hit = aabbCube.Intersect(ray);
            if (hit.haveHited) {
                if(!world.ContainChunkKey(World.GetChunkPosition(blockPosition))) continue;
                var chunkTested = world.GetChunk(blockPosition);
                if(chunkTested.chunkState < ChunkState.BLOCKGENERATED) continue;
                var testedBlock = chunkTested.GetBlock(World.GetLocalPosition(blockPosition));
                if (!testedBlock.airBlock) {
                    if (block == null || bestHitDistance > Math.Abs(hit.fNorm)) {
                        chunk = chunkTested;
                        block = testedBlock;
                        bestHitDistance = hit.fNorm;
                    }
                }
            }
        }


        if (HaveHitedBlock()) {
            UpdateFace(chunk!, ray);
        }
    }


    private void UpdateFace(Chunk chunk, Ray ray) {
        //z-
        var rblock = block!;
        var position = rblock.position.As<float>() + chunk.position.As<float>();
        var collidingPlaneBackFace = new Square(
            new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y - 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y + 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X, position.Y, position.Z - 0.5f)
        );
        if (collidingPlaneBackFace.Intersect(ray)) {
            face = Face.BACK;
            return;
        }

        //z+
        var collidingPlaneFrontFace = new Square(
            new Vector3D<float>(position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y - 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X, position.Y, position.Z + 0.5f)
        );
        if (collidingPlaneFrontFace.Intersect(ray)) {
            face = Face.FRONT;
            return;
        }

        //x-
        var collidingPlaneLeftFace = new Square(
            new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y, position.Z)
        );
        if (collidingPlaneLeftFace.Intersect(ray)) {
            face = Face.LEFT;
            return;
        }

        //x+
        var collidingPlaneRightFace = new Square(
            new Vector3D<float>(position.X + 0.5f, position.Y - 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y + 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y - 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y, position.Z)
        );
        if (collidingPlaneRightFace.Intersect(ray)) {
            face = Face.RIGHT;
            return;
        }

        //y-
        var collidingPlaneBottomFace = new Square(
            new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y - 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y - 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X, position.Y - 0.5f, position.Z)
        );
        if (collidingPlaneBottomFace.Intersect(ray)) {
            face = Face.BOTTOM;
            return;
        }

        //y+
        var collidingPlaneTopFace = new Square(
            new Vector3D<float>(position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y + 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f),
            new Vector3D<float>(position.X, position.Y + 0.5f, position.Z)
        );
        if (collidingPlaneTopFace.Intersect(ray)) {
            face = Face.TOP;
        }
    }


    public bool HaveHitedBlock() {
        UpdateBlock();
        return block != null;
    }

    public Block? GetBlock() {
        UpdateBlock();
        return block;
    }

    public Chunk? GetChunk() {
        UpdateBlock();
        return chunk;
    }


    public Face? GetFace() {
        UpdateBlock();
        return face;
    }
}