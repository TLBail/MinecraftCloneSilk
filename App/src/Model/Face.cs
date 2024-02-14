using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model
{
	public enum Face
	{
		TOP = 0,
		BOTTOM = 1,
		LEFT = 2,
		RIGHT = 3,
		FRONT = 4,
		BACK = 5,
	};

	[Flags]
	public enum FaceFlag
	{
		EMPTY = 0,
		TOP = 1,
		BOTTOM = 2,
		LEFT = 4,
		RIGHT = 8,
		FRONT = 16,
		BACK = 32
	}
	

	public static class FaceFlagUtils
	{
		public static readonly Face[] FACES = [Face.TOP, Face.BOTTOM, Face.LEFT, Face.RIGHT, Face.FRONT, Face.BACK];
		
		public static int NbFaces(FaceFlag facesFlag) {
			int total = 0;
			if ((facesFlag & FaceFlag.TOP) == FaceFlag.TOP) {
				total++;
			}
			if ((facesFlag & FaceFlag.BOTTOM) == FaceFlag.BOTTOM) {
				total++;
			}
			if ((facesFlag & FaceFlag.LEFT) == FaceFlag.LEFT) {
				total++;
			}
			if ((facesFlag & FaceFlag.RIGHT) == FaceFlag.RIGHT) {
				total++;
			}
			if ((facesFlag & FaceFlag.FRONT) == FaceFlag.FRONT) {
				total++;
			}
			if ((facesFlag & FaceFlag.BACK) == FaceFlag.BACK) {
				total++;
			}
			return total;
		}
		
		public static IEnumerable<Face> GetFaces(FaceFlag facesFlag) {
			if ((facesFlag & FaceFlag.TOP) == FaceFlag.TOP) {
				yield return Face.TOP;
			}
			if ((facesFlag & FaceFlag.BOTTOM) == FaceFlag.BOTTOM) {
				yield return Face.BOTTOM;
			}
			if ((facesFlag & FaceFlag.LEFT) == FaceFlag.LEFT) {
				yield return Face.LEFT;
			}
			if ((facesFlag & FaceFlag.RIGHT) == FaceFlag.RIGHT) {
				yield return Face.RIGHT;
			}
			if ((facesFlag & FaceFlag.FRONT) == FaceFlag.FRONT) {
				yield return Face.FRONT;
			}
			if ((facesFlag & FaceFlag.BACK) == FaceFlag.BACK) {
				yield return Face.BACK;
			}
		}

		public static FaceExtended? GetFaceExtended(FaceFlag faceFlag) {
			switch (faceFlag) {
				case FaceFlag.EMPTY:
					return null;
				case FaceFlag.TOP:
					return FaceExtended.TOP;
				case FaceFlag.BOTTOM:
					return FaceExtended.BOTTOM;
				case FaceFlag.BACK:
					return FaceExtended.BACK;
				case FaceFlag.FRONT:
					return FaceExtended.FRONT;
				case FaceFlag.LEFT:
					return FaceExtended.LEFT;
				case FaceFlag.RIGHT:
					return FaceExtended.RIGHT;
				
				case FaceFlag.TOP | FaceFlag.LEFT:
					return FaceExtended.LEFTTOP;
				case FaceFlag.TOP | FaceFlag.RIGHT:
					return FaceExtended.RIGHTTOP;
				case FaceFlag.TOP | FaceFlag.FRONT:
					return FaceExtended.TOPFRONT;
				case FaceFlag.TOP | FaceFlag.BACK:
					return FaceExtended.TOPBACK;
				
				case FaceFlag.BOTTOM | FaceFlag.LEFT:
					return FaceExtended.LEFTBOTTOM;
				case FaceFlag.BOTTOM | FaceFlag.RIGHT:
					return FaceExtended.RIGHTBOTTOM;
				case FaceFlag.BOTTOM | FaceFlag.FRONT:
					return FaceExtended.BOTTOMFRONT;
				case FaceFlag.BOTTOM | FaceFlag.BACK:
					return FaceExtended.BOTTOMBACK;
				
				case FaceFlag.LEFT | FaceFlag.TOP | FaceFlag.FRONT:
					return FaceExtended.LEFTTOPFRONT;
				case FaceFlag.RIGHT | FaceFlag.TOP | FaceFlag.FRONT:
					return FaceExtended.RIGHTTOPFRONT;
				case FaceFlag.LEFT | FaceFlag.TOP | FaceFlag.BACK:
					return FaceExtended.LEFTTOPBACK;
				case FaceFlag.RIGHT | FaceFlag.TOP | FaceFlag.BACK:
					return FaceExtended.RIGHTTOPBACK;
				
				case FaceFlag.LEFT | FaceFlag.BOTTOM | FaceFlag.FRONT:
					return FaceExtended.LEFTBOTTOMFRONT;
				case FaceFlag.RIGHT | FaceFlag.BOTTOM | FaceFlag.FRONT:
					return FaceExtended.RIGHTBOTTOMFRONT;
				case FaceFlag.LEFT | FaceFlag.BOTTOM | FaceFlag.BACK:
					return FaceExtended.LEFTBOTTOMBACK;
				case FaceFlag.RIGHT | FaceFlag.BOTTOM | FaceFlag.BACK:
					return FaceExtended.RIGHTBOTTOMBACK;

				case FaceFlag.LEFT | FaceFlag.FRONT:
					return FaceExtended.LEFTFRONT;
				case FaceFlag.RIGHT | FaceFlag.FRONT:
					return FaceExtended.RIGHTFRONT;
				case FaceFlag.LEFT | FaceFlag.BACK:
					return FaceExtended.LEFTBACK;
				case FaceFlag.RIGHT | FaceFlag.BACK:
					return FaceExtended.RIGHTBACK;
				default:
					throw new Exception("FaceFlag not found");

			}
			
		}
	}
	
	
	public static class FaceOffset
	{
		public static Vector3D<int> GetOffsetOfFace(Face face)
		{
			switch (face) {
				case Face.TOP:
					return new Vector3D<int>(0, 1, 0);
				case Face.BOTTOM:
					return new Vector3D<int>(0, -1, 0);
				case Face.LEFT:
					return new Vector3D<int>(-1, 0, 0);
				case Face.RIGHT:
					return new Vector3D<int>(1, 0, 0);
				case Face.FRONT:
					return new Vector3D<int>(0, 0, 1);
				case Face.BACK:
					return new Vector3D<int>(0, 0, -1);
				default:
					return Vector3D<int>.Zero;
			}
		}
	}
	
	
	
}
