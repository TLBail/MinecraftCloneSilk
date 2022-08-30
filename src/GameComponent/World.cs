using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using System.Numerics;

namespace MinecraftCloneSilk.src.GameComponent
{
    public class World
    {

        private Player player;
        private Game game;

        private Dictionary<Vector3, Chunk> worldChunks;

        public World(Player player)
        {
            this.player = player;
            game = Game.getInstance();
            game.updatables += Update;
            game.drawables += Draw;

            worldChunks = new Dictionary<Vector3, Chunk>();
            worldChunks.Add(Vector3.Zero, new Chunk(Vector3.Zero));

        }

        public void Update(double deltaTime)
        {
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


    }
}
