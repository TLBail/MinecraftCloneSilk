using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.src.GameComponent
{
    public class Chunk
    {
        private Vector3 position;
        private Block[,,] blocks;


        public Chunk(Vector3 position)
        {
            this.position = position;
            blocks = new Block[16, 16, 16];
            addBlock(0, 0, 0, "dirt");
        }

        public void addBlock(int x, int y , int z, string name)
        {
            blocks[x, y, z] = new Block(new Vector3(x, y, z), name, false);
            blocks[x, y, z].cube = new Cube(Game.getInstance().getGL(), blocks[x,y,z].name, getFaces(blocks[x, y, z]));
            
        }

        private Face[] getFaces(Block block)
        {
            Face[] faces = { Face.TOP,Face.BOTTOM, Face.LEFT,Face.RIGHT, Face.FRONT,Face.BACK};
            return faces;
        }



        public void Update(double deltaTime)
        {

        }

        public void Draw(GL gl, double deltaTime)
        {
            foreach (Block block in blocks)
            {
                if (!block.airBlock) {
                    block.cube?.Draw(gl, deltaTime);
                }
            }
        }

    }
}
