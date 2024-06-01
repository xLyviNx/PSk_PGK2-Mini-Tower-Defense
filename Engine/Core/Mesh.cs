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
		public bool hasTransparency
		{
			get
			{
				if (Material.FloatValues.ContainsKey("material.transparency") && Material.FloatValues["material.transparency"] <1f)
				{
					return true;
				}

				foreach(Texture tex in textures)
				{
					if (tex.transparency)
						return true;
				}
				foreach(var t in Material.Textures.Values)
				{
					if (t.transparency)
						return true;
				}

				return false;
			}
		}
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
		public void Draw(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix, List<Light> Lights, CameraComponent camera, Material? overrideMaterial = null)
		{
			//Console.WriteLine($"MESH {VAO}, {VBO}, {EBO}, VERTS: {vertices.Count}, INDICES: {indices.Count}");
			Material mat = Material;
			if(overrideMaterial != null)
			{
				mat = overrideMaterial;
			}
			mat.Shader.SetMatrix4("model", modelMatrix);
			mat.Shader.SetMatrix4("view", viewMatrix);
			mat.Shader.SetMatrix4("projection", projectionMatrix);
			if (overrideMaterial==null)
			{
				if (Material.Shader != EngineWindow.lightShader)
				{
					int lightsnum = (int)MathF.Min(8, Lights.Count);
					Console.WriteLine($"LIGHTS: {lightsnum}");
					for (int i = 0; i < lightsnum; i++)
					{
						Light l = Lights[i];
						Material.Shader.SetVector3($"lights[{i}].position", l.Position);
						Material.Shader.SetVector3($"lights[{i}].ambient", l.Ambient);
						Material.Shader.SetVector3($"lights[{i}].diffuse", l.Diffuse);
						Material.Shader.SetVector3($"lights[{i}].specular", l.Specular);
					}
					Material.Shader.SetInt("numLights", lightsnum);
					Material.Shader.SetVector3($"viewPos", camera.transform.Position);
				}
				Material.Use();
			}
			GL.BindVertexArray(VAO);
			GL.DrawElements(BeginMode.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
			GL.BindVertexArray(0);
			if(overrideMaterial==null)
				Material.Unuse();
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