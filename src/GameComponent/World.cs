﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Xml.Linq;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.Chunk;
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

public class World : GameObject
{
    private Player player;
    private const int RADIUS = 6;
    private readonly WorldUI worldUi;
    public WorldGeneration worldGeneration;

    private readonly object worldChunksLock = new object();
    public readonly ConcurrentDictionary<Vector3D<int>, Chunk> worldChunks;

    public WorldMode worldMode { get; set; }

    public World(Game game, WorldMode worldMode = WorldMode.EMPTY) : base(game) {
        game.drawables += Draw;
        this.worldMode = worldMode;
        worldChunks = new ConcurrentDictionary<Vector3D<int>, Chunk>(Environment.ProcessorCount * 2, (RADIUS + 1) * (RADIUS + 1) * (RADIUS + 1));
        worldUi = new WorldUI(this);
        worldGeneration = new WorldGeneration();
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

        lock (worldChunksLock) {
            foreach (var chunk in worldChunks.Values.ToList()) chunk.Update(deltaTime);
        }
    }


    public async Task setBlock(string blockName, Vector3D<int> position) {
        if (blockName == null) {
            throw new GameException(this, "try to set a block with a name null");
        }

        var key = getChunkPosition(position);
        var localPosition = getLocalPosition(position);
        if (worldChunks.ContainsKey(key)) {
            worldChunks[key].setBlock(localPosition.X, localPosition.Y, localPosition.Z, blockName);
        } else {
            var chunk = new Chunk(key, this);
            await chunk.setWantedChunkState(ChunkState.GENERATEDTERRAINANDSTRUCTURES);
            worldChunks.TryAdd(key, chunk);
            worldChunks[key].setBlock(localPosition.X, localPosition.Y, localPosition.Z, blockName);
        }
    }

    public async Task<Block> getBlock(Vector3D<int> position) {
        var key = getChunkPosition(position);
        var localPosition = getLocalPosition(position);
        if (worldChunks.ContainsKey(key)) {
            return await worldChunks[key].getBlock(localPosition);
        }

        var chunk = new Chunk(key, this);
        await chunk.setWantedChunkState(ChunkState.GENERATEDTERRAINANDSTRUCTURES);
        worldChunks.TryAdd(key, chunk);
        return await chunk.getBlock(localPosition);
    }


    public async Task<BlockData> getBlockData(Vector3D<int> position) {
        var key = getChunkPosition(position);
        var localPosition = getLocalPosition(position);
        if (worldChunks.ContainsKey(key)) {
            return await worldChunks[key].getBlockData(localPosition);
        }

        var chunk = new Chunk(key, this);
        await chunk.setWantedChunkState(ChunkState.GENERATEDTERRAINANDSTRUCTURES);
        worldChunks.TryAdd(key, chunk);
        return await chunk.getBlockData(localPosition);
    }


    public void setWorldMode(WorldMode worldMode) {
        this.worldMode = worldMode;
        switch (worldMode) {
            case WorldMode.EMPTY:
                if (worldChunks.Count() > 0) {
                    worldChunks.Clear();
                    GC.Collect();
                }

                break;
            case WorldMode.SIMPLE:
                if (worldChunks.Count() == 0) {
                    addExempleChunk();
                }

                break;
            case WorldMode.DYNAMIC:
                createChunkAroundPlayer();
                break;
        }
    }

    public ImmutableDictionary<Vector3D<int>, Chunk> getWorldChunks() {
        return worldChunks.ToImmutableDictionary();
    }

    public bool containChunkKey(Vector3D<int> key) {
        return worldChunks.ContainsKey(key);
    }

    public Chunk getChunk(Vector3D<int> position) {
        position = getChunkPosition(position);
        if (worldChunks.ContainsKey(position)) {
            return worldChunks[position];
        }

        var chunk = new Chunk(position, this);
        worldChunks.TryAdd(position, chunk);
        return chunk;
    }


    public void Draw(GL gl, double deltaTime) {
        lock (worldChunksLock) {
            foreach (var chunk in worldChunks.Values.ToList()) chunk.Draw(gl, deltaTime);
        }
    }

    public void updateChunkVertex(Vector3D<int> chunkPosition) {
        if (worldChunks.ContainsKey(chunkPosition)) {
            worldChunks[chunkPosition].updateChunkVertex();
        }
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
        var chunkRelevant = new List<Chunk>();
        lock (worldChunksLock) {
            var centerChunk =
                getChunkPosition(new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z));
            var rootChunk = centerChunk + new Vector3D<int>((int)(-RADIUS * Chunk.CHUNK_SIZE));
            for (var x = 0; x < 2 * RADIUS; x++)
            for (var y = 0; y < 2 * RADIUS; y++)
            for (var z = 0; z < 2 * RADIUS; z++) {
                var key = rootChunk + new Vector3D<int>((int)(x * Chunk.CHUNK_SIZE), (int)(y * Chunk.CHUNK_SIZE),
                    (int)(z * Chunk.CHUNK_SIZE));
                if (!worldChunks.ContainsKey(key)) {
                    worldChunks.TryAdd(key, new Chunk(key, this));
                }

                chunkRelevant.Add(worldChunks[key]);
            }

            var chunksToDelete = worldChunks.Values.Except(chunkRelevant).ToList();

            foreach (var chunkToDelete in chunksToDelete) removeChunk(chunkToDelete);
        }
        
        Parallel.ForEachAsync(chunkRelevant, async (chunk, token) =>
        {
            await chunk.setWantedChunkState(ChunkState.DRAWABLE);
        });
    }


    private void removeChunk(Chunk chunk) {
        chunk.Dispose();
        worldChunks.TryRemove(chunk.position, out chunk);
    }

    private void addExempleChunk() {
        Vector3D<int>[] postions =
        {
            Vector3D<int>.Zero,
            new(0, (int)Chunk.CHUNK_SIZE, 0),
            new((int)Chunk.CHUNK_SIZE, 0, 0)
        };
        foreach (var postion in postions) {
            var chunk = new Chunk(postion, this);
            chunk.setWantedChunkState(ChunkState.DRAWABLE);
            worldChunks.TryAdd(postion, chunk);
        }
    }

    public override void toImGui() {
        worldUi.drawUi();
    }
}