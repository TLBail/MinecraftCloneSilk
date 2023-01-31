using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Xml.Linq;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.UI;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.GameComponent;

public enum WorldMode
{
    EMPTY, // no chunks
    SIMPLE, //juste couple of chunk around 0
    DYNAMIC // chunks generated around player
}

public class World : GameObject, ChunkProvider
{
    private Player player;
    private const int RADIUS = 6;
    private readonly WorldUI worldUi;
    public WorldNaturalGeneration worldNaturalGeneration;
    public WorldMode worldMode { get; set; }
    public ChunkManager chunkManager;

    public World(Game game, WorldMode worldMode = WorldMode.EMPTY) : base(game) {
        game.drawables += Draw;
        this.worldMode = worldMode;
        worldUi = new WorldUI(this);
        worldNaturalGeneration = new WorldNaturalGeneration();
        chunkManager = new ChunkManager(RADIUS, worldNaturalGeneration);
    }


    protected override void start() {
        player = (Player)game.gameObjects[typeof(Player).FullName];
        if (worldMode == WorldMode.SIMPLE) {
            addExempleChunk();
        }
    }

    protected override void update(double deltaTime) {
        if (worldMode == WorldMode.DYNAMIC) {
            createChunkAroundPlayer();
        }

        foreach (var chunk in chunkManager.getChunks()) {
            chunk.Update(deltaTime);
        }
        chunkManager.update();
    }

    protected override void stop() {
        chunkManager.Dispose();
    }


    public void setBlock(string blockName, Vector3D<int> position) {
        if (blockName == null) {
            throw new GameException(this, "try to set a block with a name null");
        }
        var key = getChunkPosition(position);
        var chunk = chunkManager.getChunk(key);
        var localPosition = getLocalPosition(position);
        chunk.setBlock(localPosition.X, localPosition.Y, localPosition.Z, blockName);
    }

    public Block getBlock(Vector3D<int> position) {
        var key = getChunkPosition(position);
        var localPosition = getLocalPosition(position);
        var chunk = chunkManager.getChunk(key);
        return chunk.getBlock(localPosition);
    }


    public void setWorldMode(WorldMode worldMode) {
        this.worldMode = worldMode;
        switch (worldMode) {
            case WorldMode.EMPTY:
                if (chunkManager.count() > 0) {
                    chunkManager.clear();
                    GC.Collect();
                }

                break;
            case WorldMode.SIMPLE:
                if (chunkManager.count() == 0) {
                    addExempleChunk();
                }

                break;
            case WorldMode.DYNAMIC:
                createChunkAroundPlayer();
                break;
        }
    }

    public ImmutableDictionary<Vector3D<int>, Chunk> getWorldChunks() {
        return chunkManager.getImmutableDictionary();
    }

    public bool containChunkKey(Vector3D<int> key) {
        return chunkManager.ContainsKey(key);
    }

    public Chunk getChunk(Vector3D<int> position) {
        position = getChunkPosition(position);
        return chunkManager.getChunk(position);
    }


    public void Draw(GL gl, double deltaTime) {
            foreach (var chunk in chunkManager.getChunks()) chunk.Draw(gl, deltaTime);
    }
    
    public static Vector3D<int> getChunkPosition(Vector3D<int> blockPosition) {
        return new Vector3D<int>(
            (int)((int)MathF.Floor((float)blockPosition.X / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE),
            (int)((int)MathF.Floor((float)blockPosition.Y / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE),
            (int)((int)MathF.Floor((float)blockPosition.Z / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE)
        );
    }

    public static Vector3D<int> getLocalPosition(Vector3D<int> globalPosition) {
        var localPosition = new Vector3D<int>((int)(globalPosition.X % Chunk.CHUNK_SIZE),
            (int)(globalPosition.Y % Chunk.CHUNK_SIZE), (int)(globalPosition.Z % Chunk.CHUNK_SIZE));
        if (localPosition.X < 0) {
            localPosition.X = (int)(Chunk.CHUNK_SIZE + localPosition.X);
        }

        if (localPosition.Y < 0) {
            localPosition.Y = (int)(Chunk.CHUNK_SIZE + localPosition.Y);
        }

        if (localPosition.Z < 0) {
            localPosition.Z = (int)(Chunk.CHUNK_SIZE + localPosition.Z);
        }

        return localPosition;
    }


    private void createChunkAroundPlayer() {
        var chunkRelevant = new List<Vector3D<int>>();
        var centerChunk =
            getChunkPosition(new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z));
        var rootChunk = centerChunk + new Vector3D<int>((int)(-RADIUS * Chunk.CHUNK_SIZE));
        for (var x = 0; x < 2 * RADIUS; x++)
        for (var y = 0; y < 2 * RADIUS; y++)
        for (var z = 0; z < 2 * RADIUS; z++) {
            var key = rootChunk + new Vector3D<int>((int)(x * Chunk.CHUNK_SIZE), (int)(y * Chunk.CHUNK_SIZE),
                (int)(z * Chunk.CHUNK_SIZE));
            chunkRelevant.Add(key);
        }
        chunkManager.updateRelevantChunks(chunkRelevant);
    }


    

    private void addExempleChunk() {
        Vector3D<int>[] postions =
        {
            Vector3D<int>.Zero,
            new(0, (int)Chunk.CHUNK_SIZE, 0),
            new((int)Chunk.CHUNK_SIZE, 0, 0),
            new(0, 0, (int)Chunk.CHUNK_SIZE),
            new((int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            new(-(int)Chunk.CHUNK_SIZE, 0, 0),
            new(0, 0, -(int)Chunk.CHUNK_SIZE),
            new(-(int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            new((int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE),
        };
        foreach (var postion in postions) {
            var chunk = new Chunk(postion, this, worldNaturalGeneration);
            chunk.setWantedChunkState(ChunkState.DRAWABLE);
            chunkManager.addOrSet(chunk);
        }
    }

    public override void toImGui() {
        worldUi.drawUi();
    }
}