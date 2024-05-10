using OpenTK.Mathematics;

namespace PGK2.Engine.Core
{
	public struct MeshVertex
	{
		public Vector3 Position { get; set;  }
		public Vector3 Normal { get; set; }
		public Vector2 TexCoords { get; set; }

		public MeshVertex(Vector3 position, Vector3 normal, Vector2 texCoord)
		{
			Position = position;
			Normal = normal;
			TexCoords = texCoord;
		}
	}
}