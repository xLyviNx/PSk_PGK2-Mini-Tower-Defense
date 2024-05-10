
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using PGK2.Engine.Components.Base;

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
	public struct Texture
	{
		uint id;
		string type;
	};
	public class Mesh
	{
		private uint VAO, VBO, EBO;
		public List<MeshVertex> vertices;
		public List<uint> indices;
		public List<Texture> textures;
		public Mesh(List<MeshVertex> vertices, List<uint> indices, List<Texture> textures)
		{
			this.vertices = vertices;
			this.indices = indices;
			this.textures = textures;
			setupMesh();
		}
		public void Draw(ref CameraComponent camera)
		{
			GL.BindVertexArray(VAO);
			GL.DrawElements(BeginMode.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
			GL.BindVertexArray(0);
		}

		void setupMesh()
		{
			GL.GenVertexArrays(1, out VAO);
			GL.GenBuffers(1, out VBO);
			GL.GenBuffers(1, out EBO);

			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Marshal.SizeOf<MeshVertex>(), vertices.ToArray(), BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.ArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<MeshVertex>(), 0);

			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<MeshVertex>(), Marshal.OffsetOf<MeshVertex>("Normal"));

			GL.EnableVertexAttribArray(2);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<MeshVertex>(), Marshal.OffsetOf<MeshVertex>("TexCoords"));

			GL.BindVertexArray(0);
		}
	};
}