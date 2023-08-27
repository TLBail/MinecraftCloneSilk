using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using MinecraftCloneSilk.UI;
using Silk.NET.Maths;
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
    private Player player = null!;
    public int radius { get; set; } = 6;
    private readonly WorldUi worldUi;
    public WorldNaturalGeneration worldNaturalGeneration;
    public WorldMode worldMode { get; set; }
    public ChunkManager chunkManager;

    private Vector3D<int> lastPlayerChunkPosition = new Vector3D<int>(-1);

    private RegionStorage regionStorage;

    public World(Game game, WorldMode worldMode = WorldMode.EMPTY) : base(game) {
        this.worldMode = worldMode;
        worldUi = new WorldUi(this);
        worldNaturalGeneration = new WorldNaturalGeneration();
        regionStorage = new RegionStorage("./Worlds/newWorld");
        chunkManager = new ChunkManager(radius, worldNaturalGeneration, regionStorage);
    }


    protected override void Start() {
        player = (Player)game.gameObjects[typeof(Player).FullName!];
        if (worldMode == WorldMode.SIMPLE) {
            AddExempleChunk();
        }

        AddCommand();
    }


    [Logger.Timer]
    protected override void Update(double deltaTime) {
        if (worldMode == WorldMode.DYNAMIC) {
            CreateChunkAroundPlayer();
        }
        chunkManager.Update(deltaTime);
    }

    protected override void Stop() {
        chunkManager.Dispose();
    }


    public void SetBlock(string blockName, Vector3D<int> position) {
        if (blockName == null) {
            throw new GameException(this, "try to set a block with a name null");
        }

        var key = GetChunkPosition(position);
        var chunk = chunkManager.GetChunk(key);
        var localPosition = GetLocalPosition(position);
        chunk.SetBlock(localPosition.X, localPosition.Y, localPosition.Z, blockName);
    }

    public Block GetBlock(Vector3D<int> position) {
        var key = GetChunkPosition(position);
        var localPosition = GetLocalPosition(position);
        var chunk = chunkManager.GetChunk(key);
        return chunk.GetBlock(localPosition);
    }


    public void SetWorldMode(WorldMode worldMode) {
        this.worldMode = worldMode;
        switch (worldMode) {
            case WorldMode.EMPTY:
                if (chunkManager.Count() > 0) {
                    chunkManager.Clear();
                }

                break;
            case WorldMode.SIMPLE:
                if (chunkManager.Count() == 0) {
                    AddExempleChunk();
                }

                break;
            case WorldMode.DYNAMIC:
                CreateChunkAroundPlayer();
                break;
        }
    }

    public List<Chunk> GetWorldChunks() {
        return chunkManager.GetChunksList();
    }

    public bool ContainChunkKey(Vector3D<int> key) {
        return chunkManager.ContainsKey(key);
    }

    public Chunk GetChunk(Vector3D<int> position) {
        position = GetChunkPosition(position);
        return chunkManager.GetChunk(position);
    }


    public static Vector3D<int> GetChunkPosition(Vector3D<int> blockPosition) {
        return new Vector3D<int>(
            (int)((int)MathF.Floor((float)blockPosition.X / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE),
            (int)((int)MathF.Floor((float)blockPosition.Y / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE),
            (int)((int)MathF.Floor((float)blockPosition.Z / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE)
        );
    }

    public static Vector3D<int> GetLocalPosition(Vector3D<int> globalPosition) {
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

    private void AddCommand() {
        Console console = (Console)game.gameObjects[typeof(Console).FullName!];
        console.AddCommand("/addChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> newPosition = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                chunkManager.AddChunkToLoad(newPosition);
                console.Log("chunk at " + newPosition + " added succefuly"
                );
            }
        });

        console.AddCommand("/rmChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.Log("chunk at " + position + " unloaded " + (
                        chunkManager.TryToUnloadChunk(position) ? "succefuly" : "failed")
                );
            }
        });

        console.AddCommand("/setBlock", (commandParams) =>
        {
            if (commandParams.Length >= 4) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                SetBlock(commandParams[3], position);
                console.Log("block " + commandParams[3] + " at " + position + " added succefuly"
                );
            }
        });

        console.AddCommand("/getBlock", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.Log("block at " + position + " is " + GetBlock(position).name
                );
            }
        });

        console.AddCommand("/getChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.Log("chunk at " + position + " is \n" + GetChunk(position).ToString()
                );
            }
        });
    }

    private void CreateChunkAroundPlayer() {
        var centerChunk =
            GetChunkPosition(new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z));
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

        chunkManager.UpdateRelevantChunks(chunkRelevant);
    }


    private void AddExempleChunk() {
        Vector3D<int>[] postions =
        {
            Vector3D<int>.Zero,

            // new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            // new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE),
            // new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, 0),
            // new Vector3D<int>(0, 0, -(int)Chunk.CHUNK_SIZE),
            // new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0),
            // new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            // new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE),
            // new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, 0),
            // new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            // new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE)
        };
        foreach (var position in postions) {
            chunkManager.AddChunkToLoad(position);
        }
    }

    public override void ToImGui() {
        worldUi.DrawUi();
    }
}