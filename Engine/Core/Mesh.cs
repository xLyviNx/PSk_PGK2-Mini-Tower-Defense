using System.Runtime.InteropServices;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PGK2.Engine.Components.Base;

namespace PGK2.Engine.Core
{
	public class Mesh
	{
		private int VAO, VBO, EBO;
		public List<MeshVertex> vertices;
		public List<uint> indices;
		public List<Texture> textures;
		public Material Material;
		public Mesh(List<MeshVertex> vertices, List<uint> indices, List<Texture> textures, Material mat)
		{
			this.vertices = vertices;
			this.indices = indices;
			this.textures = textures;
			Material = mat;
			Console.WriteLine($"INIT MESH: {vertices.Count}, {indices.Count}, {textures.Count}");
			setupMesh();
		}
		public void Draw(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix)
		{
			Console.WriteLine($"MESH {VAO}, {VBO}, {EBO}, VERTS: {vertices.Count}, INDICES: {indices.Count}");
			Material.Shader.SetMatrix4("model", modelMatrix);
			Material.Shader.SetMatrix4("view", viewMatrix);
			Material.Shader.SetMatrix4("projection", projectionMatrix);
			Material.Use();
			GL.BindVertexArray(VAO);
			GL.DrawElements(BeginMode.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
			GL.BindVertexArray(0);
			//Material.Unuse();
		}
		void ExtractMeshArrays(out float[] pos, out float[] norm, out float[] texcoords)
		{
			var P = new List<float>();
			var N = new List<float>();
			var TC = new List<float>();

			foreach(var vert in vertices)
			{
				P.Add(vert.Position.X);
				P.Add(vert.Position.Y);
				P.Add(vert.Position.Z);

				N.Add(vert.Normal.X);
				N.Add(vert.Normal.Y);
				N.Add(vert.Normal.Z);


				TC.Add(vert.TexCoords.X);
				TC.Add(vert.TexCoords.Y);

			}
			pos = P.ToArray();
			norm=N.ToArray();
			texcoords = TC.ToArray();
		}
		void setupMesh()
		{
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			EBO = GL.GenBuffer();

			GL.BindVertexArray(VAO);

			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * MeshVertex.SizeInBytes, vertices.ToArray(), BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

			// vertex positions
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, MeshVertex.SizeInBytes, 0);

			// vertex normals
			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, MeshVertex.SizeInBytes, Vector3.SizeInBytes);

			// vertex texture coords
			GL.EnableVertexAttribArray(2);
			GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, MeshVertex.SizeInBytes, Vector3.SizeInBytes * 2);

			GL.BindVertexArray(0);
		}
	};
}