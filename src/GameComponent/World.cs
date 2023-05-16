using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using System.Xml.Linq;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.UI;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Console = MinecraftCloneSilk.UI.Console;

namespace MinecraftCloneSilk.GameComponent;

public enum WorldMode
{
    EMPTY, // no chunks
    SIMPLE, //juste couple of chunk around 0
    DYNAMIC // chunks generated around player
}

public class World : GameObject
{
    private Player player;
    public int radius { get; set; } = 16;
    private readonly WorldUI worldUi;
    public WorldNaturalGeneration worldNaturalGeneration;
    public WorldMode worldMode { get; set; }
    public ChunkManager chunkManager;

    private Vector3D<int> lastPlayerChunkPosition = new Vector3D<int>(-1);

    private ChunkStorage chunkStorage;

    public World(Game game, WorldMode worldMode = WorldMode.EMPTY) : base(game) {
        this.worldMode = worldMode;
        worldUi = new WorldUI(this);
        worldNaturalGeneration = new WorldNaturalGeneration();
        chunkStorage = new ChunkStorage("./Worlds/newWorld");
        chunkManager = new ChunkManager(radius, worldNaturalGeneration, chunkStorage);
    }


    protected override void start() {
        player = (Player)game.gameObjects[typeof(Player).FullName];
        if (worldMode == WorldMode.SIMPLE) {
            addExempleChunk();
        }

        addCommand();
    }


    protected override void update(double deltaTime) {
        if (worldMode == WorldMode.DYNAMIC) {
            createChunkAroundPlayer();
        }

        chunkManager.update(deltaTime);
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

    public List<Chunk> getWorldChunks() {
        return chunkManager.getChunksList();
    }

    public bool containChunkKey(Vector3D<int> key) {
        return chunkManager.ContainsKey(key);
    }

    public Chunk getChunk(Vector3D<int> position) {
        position = getChunkPosition(position);
        return chunkManager.getChunk(position);
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

    private void addCommand() {
        Console console = (Console)game.gameObjects[typeof(Console).FullName];
        console.addCommand("/addChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> newPosition = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                chunkManager.addChunkToLoad(newPosition);
                console.log("chunk at " + newPosition + " added succefuly"
                );
            }
        });

        console.addCommand("/rmChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.log("chunk at " + position + " unloaded " + (
                        chunkManager.tryToUnloadChunk(position) ? "succefuly" : "failed")
                );
            }
        });

        console.addCommand("/setBlock", (commandParams) =>
        {
            if (commandParams.Length >= 4) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                setBlock(commandParams[3], position);
                console.log("block " + commandParams[3] + " at " + position + " added succefuly"
                );
            }
        });

        console.addCommand("/getBlock", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.log("block at " + position + " is " + getBlock(position).name
                );
            }
        });

        console.addCommand("/getChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.log("chunk at " + position + " is \n" + getChunk(position).ToString()
                );
            }
        });
    }

    private void createChunkAroundPlayer() {
        var centerChunk =
            getChunkPosition(new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z));
        if (lastPlayerChunkPosition == centerChunk) return;
        lastPlayerChunkPosition = centerChunk;
        var chunkRelevant = new List<Vector3D<int>>();
        var rootChunk = centerChunk + new Vector3D<int>((int)(-radius * Chunk.CHUNK_SIZE));
        for (var x = 0; x < 2 * radius; x++)
        for (var y = 0; y < 2 * radius; y++)
        for (var z = 0; z < 2 * radius; z++) {
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

            new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE),
            new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, 0),
            new Vector3D<int>(0, 0, -(int)Chunk.CHUNK_SIZE),
            new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0),
            new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE),
            new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, 0),
            new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE)
        };
        foreach (var position in postions) {
            chunkManager.addChunkToLoad(position);
        }
    }

    public override void toImGui() {
        worldUi.drawUi();
    }
}