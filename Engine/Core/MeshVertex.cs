using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace PGK2.Engine.Core
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MeshVertex
	{
		public Vector3 Position { get; set;  }

		public Vector3 Normal { get; set; }
		public Vector2 TexCoords { get; set; }

		public static int SizeInBytes => Marshal.SizeOf<MeshVertex>();

		public MeshVertex(Vector3 position, Vector3 normal, Vector2 texCoord)
		{
			Position = position;
			Normal = normal;
			TexCoords = texCoord;
		}
	}
}