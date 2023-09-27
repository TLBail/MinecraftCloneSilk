using System.Collections.ObjectModel;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public enum FaceExtended
{
    TOP = 0,
    BOTTOM = 1,
    LEFT = 2,
    RIGHT = 3,
    FRONT = 4,
    BACK = 5,
		
    LEFTTOP = 6,
    RIGHTTOP = 7,
    TOPFRONT = 8,
    TOPBACK = 9,
		
    LEFTBOTTOM = 10,
    RIGHTBOTTOM = 11,
    BOTTOMFRONT = 12,
    BOTTOMBACK = 13,
		
    LEFTTOPFRONT = 14,
    RIGHTTOPFRONT = 15,
    LEFTTOPBACK = 16,
    RIGHTTOPBACK = 17,

    LEFTBOTTOMFRONT = 18,
    RIGHTBOTTOMFRONT = 19,
    LEFTBOTTOMBACK = 20,
    RIGHTBOTTOMBACK = 21,
		
    LEFTFRONT = 22,
    RIGHTFRONT = 23,
    LEFTBACK = 24,
    RIGHTBACK = 25
};

public static class FaceExtendedConst
{
	public static readonly ReadOnlyCollection<FaceExtended> FACES = new ReadOnlyCollection<FaceExtended>(Enum.GetValues<FaceExtended>());
}

public static class FaceExtendedOffset
{
	public static Vector3D<int> GetOffsetOfFace(FaceExtended face)
	{
		return _faceOffsets[(int) face];
	}
	
	
	private static readonly Vector3D<int>[] _faceOffsets = new Vector3D<int>[]
	{
		new Vector3D<int>(0, 1, 0),
    	new Vector3D<int>(0, -1, 0),
    	new Vector3D<int>(-1, 0, 0),
    	new Vector3D<int>(1, 0, 0),
    	new Vector3D<int>(0, 0, 1),
    	new Vector3D<int>(0, 0, -1),
    	new Vector3D<int>(-1, 1, 0),
    	new Vector3D<int>(1, 1, 0),
    	new Vector3D<int>(0, 1, 1),
    	new Vector3D<int>(0, 1, -1),
    	new Vector3D<int>(-1, -1, 0),
    	new Vector3D<int>(1, -1, 0),
    	new Vector3D<int>(0, -1, 1),
    	new Vector3D<int>(0, -1, -1),
    	new Vector3D<int>(-1, 1, 1),
    	new Vector3D<int>(1, 1, 1),
    	new Vector3D<int>(-1, 1, -1),
    	new Vector3D<int>(1, 1, -1),
    	new Vector3D<int>(-1, -1, 1),
    	new Vector3D<int>(1, -1, 1),
    	new Vector3D<int>(-1, -1, -1),
    	new Vector3D<int>(1, -1, -1),
    	new Vector3D<int>(-1, 0, 1),
    	new Vector3D<int>(1, 0, 1),
    	new Vector3D<int>(-1, 0, -1),
    	new Vector3D<int>(1, 0, -1),
	};
}