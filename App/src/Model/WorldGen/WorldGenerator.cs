﻿using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.WorldGen;

public interface IWorldGenerator
{
    public void GenerateTerrain(Vector3D<int> chunkPosition, BlockData[,,] blocks);
    bool HaveTreeOnThisCoord(int positionX, int positionZ);
}