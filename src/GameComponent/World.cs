using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.Serialization.Json;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent
{
    public enum WorldMode
    {
        EMPTY,   // no chunks
        SIMPLE,  //juste couple of chunk around 0
        DYNAMIC // chunks generated around player
    }
    
    public class World
    {

        private Player player;
        private Game game;
        private const int RADIUS = 3;


        private Dictionary<Vector3D<int>, Chunk> worldChunks;

        public WorldMode worldMode { get; set; }
        
        public World(Player player, WorldMode worldMode)
        {
            this.player = player;
            game = Game.getInstance();
            game.updatables += Update;
            game.drawables += Draw;
            this.worldMode = worldMode;

            worldChunks = new Dictionary<Vector3D<int>, Chunk>((RADIUS + 1) * (RADIUS + 1)* (RADIUS + 1));
        }


        public void addBlock(string blockName, Vector3D<int> position)
        {
            if (blockName.Equals("airBlock")) {
                removeBlock(position);
                return;
            }
            Vector3D<int> key = getChunkPosition(position);
            if (worldChunks.ContainsKey(key)) {
                Vector3D<int> localPosition = getLocalPosition(position);
                worldChunks[key].addBlock(localPosition.X, localPosition.Y, localPosition.Z, blockName);
            }
        }

        public void removeBlock(Vector3D<int> position)
        {
            Vector3D<int> key = getChunkPosition(position);
            if (worldChunks.ContainsKey(key)) {
                Vector3D<int> localPosition = getLocalPosition(position);
                worldChunks[key].removeBlock(localPosition);
            }
        }

        public Block getBlock(Vector3D<int> position)
        {
            Vector3D<int> key = getChunkPosition(position);
            Vector3D<int> localPosition = getLocalPosition(position);
            if (worldChunks.ContainsKey(key)) {
                return worldChunks[key].getBlock(localPosition);
            } else {
                Chunk chunk = new Chunk(key, this);
                worldChunks.Add(key, chunk);
                return chunk.getBlock(localPosition);
            }
            
        }

        public void setWorldMode(WorldMode worldMode)
        {
            this.worldMode =  worldMode;
            switch (worldMode) {
                case WorldMode.EMPTY:
                    if(worldChunks.Count() > 0) worldChunks.Clear();
                    break;
                case WorldMode.SIMPLE:
                    if (worldChunks.Count() == 0) addExempleChunk();
                    break;
                case WorldMode.DYNAMIC:
                    createChunkAroundPlayer();
                    break;
            }
        }

        public ImmutableDictionary<Vector3D<int>, Chunk> getWorldChunks()
        {
            return worldChunks.ToImmutableDictionary();
        }

        public Chunk getChunk(Vector3D<int> position)
        {
            position = getChunkPosition(position);
            if (worldChunks.ContainsKey(position)) {
                return worldChunks[position];
            }
            return null;
        }
        
        public void Update(double deltaTime)
        {
            if(worldMode == WorldMode.DYNAMIC) createChunkAroundPlayer();   
            
            foreach (var chunk in worldChunks.Values)
            {
                chunk.Update(deltaTime);
            }
            
            
        }

        public void Draw(GL gl, double deltaTime)
        {
            foreach (var chunk in worldChunks.Values)
            {
                chunk.Draw(gl, deltaTime);
            }
        }

        public void updateChunkVertex(Vector3D<int> chunkPosition)
        {
            if (worldChunks.ContainsKey(chunkPosition)) {
                worldChunks[chunkPosition].updateChunkVertex();
            }
        }
        
        public static Vector3D<int> getChunkPosition(Vector3D<int> blockPosition) {
            return new Vector3D<int>(
                (int)((int)(MathF.Floor((float)blockPosition.X / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
                (int)((int)(MathF.Floor((float)blockPosition.Y/ Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
                (int)((int)(MathF.Floor((float)blockPosition.Z / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE)
            );

        }

        public static Vector3D<int> getLocalPosition(Vector3D<int> globalPosition) {
            Vector3D<int> localPosition  = new Vector3D<int>((int)(globalPosition.X % Chunk.CHUNK_SIZE), (int)(globalPosition.Y % Chunk.CHUNK_SIZE), (int)(globalPosition.Z % Chunk.CHUNK_SIZE));
            if (localPosition.X < 0) localPosition.X = (int)(Chunk.CHUNK_SIZE  + localPosition.X);
            if (localPosition.Y < 0) localPosition.Y = (int)(Chunk.CHUNK_SIZE + localPosition.Y);
            if (localPosition.Z < 0) localPosition.Z = (int)(Chunk.CHUNK_SIZE + localPosition.Z);
            return localPosition;
        }


        private void createChunkAroundPlayer()
        {
            Vector3D<int> centerChunk = getChunkPosition(new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z));
            Vector3D<int> rootChunk = centerChunk + new Vector3D<int>((int)(-RADIUS * Chunk.CHUNK_SIZE));
            List<Chunk> chunkRelevant = new List<Chunk>();
            for (int x = 0; x < 2 *RADIUS; x++) {
                for (int y = 0; y < 2 * RADIUS; y++) {
                    for (int z = 0; z < 2 * RADIUS; z++) {
                        Vector3D<int> key = rootChunk + new Vector3D<int>((int)(x * Chunk.CHUNK_SIZE), (int)(y * Chunk.CHUNK_SIZE),
                            (int)(z * Chunk.CHUNK_SIZE));
                        if (!worldChunks.ContainsKey(key)) {
                            worldChunks.Add(key, new Chunk(key, this));
                        }
                        chunkRelevant.Add(worldChunks[key]);
                    }
                }
            }

            var chunksToDelete = worldChunks.Values.Except(chunkRelevant);

            foreach (var chunkToDelete in chunksToDelete) {
                worldChunks.Remove(chunkToDelete.getPosition());
            }

            foreach (var chunkReleva in chunkRelevant) {
                chunkReleva.displayChunk();
            }
        }

        
        private void addExempleChunk()
        {
            Vector3D<int>[] postions = new[]
            {
                Vector3D<int>.Zero, 
                new Vector3D<int>(0, (int)Chunk.CHUNK_SIZE, 0),
                new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0),

            };
            foreach (var postion in postions) {
                worldChunks.Add(postion, new Chunk(postion, this));
            }

            foreach (var postion in postions) {
                worldChunks[postion].displayChunk();
            }
            
        }

    }
}
