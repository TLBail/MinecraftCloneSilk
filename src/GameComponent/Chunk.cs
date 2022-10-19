using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace MinecraftCloneSilk.src.GameComponent
{
    public class Chunk
    {
        private Vector3 position;
        private Block[,,] blocks;

        private static readonly uint CHUNK_SIZE = 16;


        public Chunk(Vector3 position)
        {
            this.position = position;
            blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
            generateTerrain();
            fillChunkWithAirBlock();
            initBlocks();
        }

        private void fillChunkWithAirBlock()
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        if(blocks[x,y,z].Equals(default(Block))) blocks[x, y, z] = new Block(new Vector3(x, y, z));
                    }
                }
            }

        }

        private void generateTerrain()
        {

            Vector3[] positions = new[]
            {
                new Vector3(0, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(4, 0, 0),
                new Vector3(0, 2, 0),
                new Vector3(0, 4, 0),
                new Vector3(0, 0, 2),
                new Vector3(0, 0, 4),
                new Vector3(2, 0, 2),
                new Vector3(4, 0, 4)
            };
            foreach (var position in positions)
            {
                blocks[(int)position.X, (int)position.Y, (int)position.Z] = new Block(position, "dirt", false);
            }
        }

        private void initBlocks()
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        if(!blocks[x, y, z].airBlock) initCubes(x, y, z);
                    }
                }
            }
        }

        private void initCubes(int x, int y, int z) {
            if(getFaces(blocks[x,y,z]).Length == 0) return;
            blocks[x, y, z].cube = new Cube(Game.getInstance().getGL(), blocks[x,y,z].name, getFaces(blocks[x, y, z]));
            
        }

        public void addBlock(int x, int y , int z, string name)
        {
            blocks[x, y, z] = new Block(new Vector3(x, y, z), name, false);
            //only if the cube is visible => faces not empty
            initCubes(x, y, z);
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
                    block.cube?.Draw(gl, deltaTime, block.position);
                }
            }
        }

    }
}
