using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;
using Plane = Silk.NET.Maths.Plane;

namespace MinecraftCloneSilk.Model;

public static class Brush
{
    public static void Line(World world, Player player, int length, string blockName) {
        Vector3 position = player.position;
        for (int i = 0; i < length; i++) {
            world.SetBlock(blockName, new Vector3D<int>((int)position.X, (int)position.Y, (int)position.Z));
            position += player.GetDirection3D();
        }
    }

    public static void Bomb(World world, Vector3D<int> origin, int size) {
        for (int x = -size; x < size; x++) {
            for (int y = -size; y < size; y++) {
                for (int z = -size; z < size; z++) {
                    Vector3D<int> positionS = origin + new Vector3D<int>(x, y, z);
                    if (Vector3D.Distance(positionS, origin) < size)
                        world.SetBlock("air", positionS);
                }
            }
        }
    }


    public static void Wall(World world, Player player, int size, string blockName) {
        Vector3 position = player.position + player.GetDirection3D() * size;
        Vector3D<int> centerWall = new Vector3D<int>(
            (int)position.X,
            (int)position.Y,
            (int)position.Z);
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                world.SetBlock(blockName,
                    centerWall + (new Vector3D<int>(x, y, 0) - new Vector3D<int>(size / 2, size / 2, 0)));
            }
        }
    }

    /**
     * Crée une spirale à partir de la position du joueur
     * @param nbTurns Nombre de tours de la spirale
     * @param blockName Nom du bloc à utiliser pour la spirale
     * @param heightIncrement Incrément de hauteur après chaque tour complet
     */
    public static void Spiral(World world,Vector3 origin, int nbTurns, float heightIncrement, string blockName) {
        float angle = 0;
        float radius = 0;
        float height = 0;
        // Détermine le nombre de blocs en fonction du nombre de tours et de la précision
        int totalBlocks = (int)(nbTurns * 360 * 1.5f);

        for (int i = 0; i < totalBlocks; i++) {
            // Convertit l'angle et le rayon en coordonnées X et Z
            float x = radius * MathF.Cos(MathHelper.DegreesToRadians(angle));
            float z = radius * MathF.Sin(MathHelper.DegreesToRadians(angle));

            // Place le bloc
            world.SetBlock(blockName,
                new Vector3D<int>((int)(origin.X + x), (int)(origin.Y + height), (int)(origin.Z + z)));

            // Augmente l'angle, le rayon et la hauteur
            angle += 0.4f; // Plus cette valeur est petite, plus la spirale sera serrée
            radius += 0.005f; // Augmente le rayon progressivement
            height += heightIncrement / 360; // Augmente la hauteur après chaque tour complet

            if (angle >= 360 * nbTurns) {
                break; // Sort de la boucle une fois le nombre de tours atteint
            }
        }
    }

    public static void Sierpinski(World world,Vector3 position, int level, int size, string blockName) {
        if (level == 0)
        {
            // Dessine un triangle simple à la position donnée
            Triangle(world,position, size, blockName);
        }
        else
        {
            int newSize = size / 2;
            // Dessine 3 triangles de Sierpinski de niveau inférieur
            Sierpinski(world,position, level - 1, newSize, blockName);
            Sierpinski(world,new Vector3(position.X + newSize, position.Y, position.Z), level - 1, newSize, blockName);
            Sierpinski(world,new Vector3(position.X + newSize / 2, position.Y, position.Z + (int)(newSize * Math.Sqrt(3) / 2)), level - 1, newSize, blockName);
        }
        
    }
    public static void Triangle(World world, Vector3 position, int size, string blockName)
    {
        for (int y = 0; y <= size; y++)
        {
            for (int x = 0; x <= y; x++)
            {
                world.SetBlock(blockName, new Vector3D<int>((int)position.X + x, (int)position.Y, (int)position.Z + y));
                world.SetBlock(blockName, new Vector3D<int>((int)position.X - x, (int)position.Y, (int)position.Z + y));
            }
        }
    }

    public static void MengerSponge(World world, Vector3 position, int level, int size, string blockName) {
        if (level == 0)
        {
            // Dessiner un cube plein à la position donnée
            Cube(world,position, size, blockName);
        }
        else
        {
            int newSize = size / 3;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        if (x != 1 || y != 1 && z != 1) // Ne pas dessiner le cube central et les cubes centraux de chaque face
                        {
                            Vector3 newPosition = new Vector3(position.X + x * newSize, position.Y + y * newSize, position.Z + z * newSize);
                            MengerSponge(world,newPosition, level - 1, newSize, blockName);
                        }
                    }
                }
            }
        }
    }
    public static void Cube(World world,Vector3 position, int size, string blockName)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    world.SetBlock(blockName, new Vector3D<int>((int)position.X + x, (int)position.Y + y, (int)position.Z + z));
                }
            }
        }
    }

}