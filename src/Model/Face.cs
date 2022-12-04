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
		BACK = 5
		
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
		public static int nbFaces(FaceFlag facesFlag) {
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
		
		public static IEnumerator<Face> getFaces(FaceFlag facesFlag) {
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
	}
	
	
	public static class FaceOffset
	{
		public static Vector3D<int> getOffsetOfFace(Face face)
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
		public static Vector3D<int> getOffsetOfFace(FaceFlag face)
		{
			switch (face) {
				case FaceFlag.TOP:
					return new Vector3D<int>(0, 1, 0);
				case FaceFlag.BOTTOM:
					return new Vector3D<int>(0, -1, 0);
				case FaceFlag.LEFT:
					return new Vector3D<int>(-1, 0, 0);
				case FaceFlag.RIGHT:
					return new Vector3D<int>(1, 0, 0);
				case FaceFlag.FRONT:
					return new Vector3D<int>(0, 0, 1);
				case FaceFlag.BACK:
					return new Vector3D<int>(0, 0, -1);
				default:
					return Vector3D<int>.Zero;
			}
		} 
	}
	
	
	
}
