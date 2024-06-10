using System.Runtime.InteropServices;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PGK2.Engine.Components.Base;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Klasa reprezentująca pojedynczy mesh dla modelu.
	/// </summary>
	public class Mesh
	{
		/// <summary>
		/// Identyfikator VAO (Vertex Array Object).
		/// </summary>
		private int VAO;
		/// <summary>
		/// Identyfikator VBO (Vertex Buffer Object).
		/// </summary>
		private int VBO;
		/// <summary>
		/// Identyfikator EBO.
		/// </summary>
		private int EBO;

		/// <summary>
		/// Lista wierzchołków mesha.
		/// </summary>
		public List<MeshVertex> vertices;
		/// <summary>
		/// Lista indeksów tworzących trójkąty w meshu.
		/// </summary>
		public List<uint> indices;
		/// <summary>
		/// Lista tekstur przypisanych do mesha.
		/// </summary>
		public List<Texture> textures;
		/// <summary>
		/// Lista shaderów wykluczonych z uzywania swiatel, material.use dla tego mesha (np. shader obrysu).
		/// </summary>
		public static List<Shader> Exclude = new List<Shader>() { EngineWindow.OutlineShader, EngineWindow.lightShader, EngineWindow.GridShader };
		/// <summary>
		/// Właściwość informująca czy mesh posiada tekstury z przezroczystością.
		/// </summary>
		public bool hasTransparentTextures
		{
			get
			{
				foreach (Texture tex in textures)
				{
					if (tex.transparency)
						return true;
				}
				foreach (var t in Material.Textures.Values)
				{
					if (t.transparency)
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Właściwość informująca czy mesh posiada przezroczystość (materiał lub tekstury).
		/// </summary>
		public bool hasTransparency
		{
			get
			{
				return Material.HasTransparency || hasTransparentTextures;
			}
		}
		/// <summary>
		/// Domyślny materiał przypisany do mesha.
		/// </summary>
		public Material Material;
		/// <summary>
		/// Pole otaczające mesh (bounding box).
		/// </summary>
		public BoundingBox BoundingBox;
		/// <summary>
		/// Konstruktor inicjujący nowy mesh.
		/// </summary>
		/// <param name="vertices">Lista wierzchołków mesha.</param>
		/// <param name="indices">Lista indeksów tworzących trójkąty w meshu.</param>
		/// <param name="textures">Lista tekstur przypisanych do mesha.</param>
		/// <param name="mat">Materiał przypisany do mesha.</param>
		public Mesh(List<MeshVertex> vertices, List<uint> indices, List<Texture> textures, Material mat)
		{
			this.vertices = vertices;
			this.indices = indices;
			this.textures = textures;
			Material = mat;
			setupMesh();
			BoundingBox = CalculateBoundingBox();
		}
		/// <summary>
		/// Metoda obliczająca pole otaczające mesh (bounding box).
		/// </summary>
		/// <returns>Pole otaczające mesh (bounding box).</returns>
		public BoundingBox CalculateBoundingBox()
		{
			if (vertices.Count == 0)
				return null;

			Vector3 min = vertices[0].Position;
			Vector3 max = vertices[0].Position;

			foreach (var vertex in vertices)
			{
				min = Vector3.ComponentMin(min, vertex.Position);
				max = Vector3.ComponentMax(max, vertex.Position);
			}

			return new BoundingBox(min, max);
		}
		/// <summary>
		/// Metoda rysująca mesha.
		/// </summary>
		/// <param name="modelMatrix">Macierz modelu.</param>
		/// <param name="viewMatrix">Macierz widoku.</param>
		/// <param name="projectionMatrix">Macierz projekcji.</param>
		/// <param name="Lights">Kolekcja świateł w scenie.</param>
		/// <param name="camera">Komponent kamery.</param>
		/// <param name="overrideMaterial">Materiał zastępujący domyślny materiał mesha (opcjonalny).</param>
		public void Draw(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix, List<Light> Lights, CameraComponent camera, Material? overrideMaterial)
		{
			Material mat = overrideMaterial != null? overrideMaterial : Material;
			mat.Shader.SetMatrix4("model", modelMatrix);
			mat.Shader.SetMatrix4("view", viewMatrix);
			mat.Shader.SetMatrix4("projection", projectionMatrix);
			bool usednormal = false;
			if (!Exclude.Contains(mat.Shader))
			{
				usednormal = true;
				int lightsnum = (int)MathF.Min(8, Lights.Count);
				for (int i = 0; i < lightsnum; i++)
				{
					Light l = Lights[i];
					mat.Shader.SetVector3($"lights[{i}].position", l.Position);
					mat.Shader.SetVector3($"lights[{i}].ambient", l.Ambient);
					mat.Shader.SetVector3($"lights[{i}].diffuse", l.Diffuse);
					mat.Shader.SetVector3($"lights[{i}].specular", l.Specular);
				}
				mat.Shader.SetInt("numLights", lightsnum);
				mat.Shader.SetVector3($"viewPos", camera.transform.Position);
			}
			if (mat.Shader != Exclude[0])
			{
				mat.Use();
				usednormal = true;
			}
			GL.BindVertexArray(VAO);
			GL.DrawElements(BeginMode.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
			GL.BindVertexArray(0);
			if(usednormal)
				mat.Unuse();
		}
		/// <summary>
		/// Metoda inicjująca VAO, VBO i EBO dla mesha.
		/// </summary>
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