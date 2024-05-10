using Assimp.Configs;
using Assimp;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace PGK2.Engine.Core
{
	public class Mesh
	{
		private static Dictionary<string, Mesh> LoadedMeshes = new();
		private Dictionary<int, List<MeshVertex>> MaterialParts = new Dictionary<int, List<MeshVertex>>();

		// Bufor OpenGL dla wierzchołków
		private int VertexBufferObject;
		private int VertexArrayObject;

		public List<Material> LoadedMaterials { get; private set; }
		public List<MeshVertex> Vertices { get; private set; }

		public Mesh()
		{
			Vertices = new List<MeshVertex>();
			VertexBufferObject = 0;
			LoadedMaterials = new();
		}
		public float[] ExtractVerts()
		{
			List<float> vertsList = new();
			foreach (var v in Vertices)
			{
				vertsList.Add(v.Position.X);
				vertsList.Add(v.Position.Y);
				vertsList.Add(v.Position.Z);
			};
			return vertsList.ToArray();
		}
		public void Unload()
		{
			if (VertexBufferObject != 0)
			{
				GL.DeleteBuffer(VertexBufferObject);
				VertexBufferObject = 0;
			}

			if (LoadedMaterials != null)
			{
				foreach (var material in LoadedMaterials)
				{
					//material?.Texture2D?.Dispose();
				}
			}
		}

		public static Mesh LoadFromFile(string filePath)
		{
			if (LoadedMeshes.ContainsKey(filePath))
				return LoadedMeshes[filePath];

			AssimpContext context = new AssimpContext();
			context.SetConfig(new NormalSmoothingAngleConfig(66.0f));

			Scene scene = context.ImportFile(filePath, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

			if (scene == null || scene.Meshes.Count == 0)
				throw new Exception("Failed to load mesh.");

			Mesh loadedMesh = new Mesh();

			foreach (var mesh in scene.Meshes)
			{

				// Przetwórz informacje o meshu, takie jak wierzchołki, indeksy itd.
				for (int i = 0; i < mesh.Vertices.Count; i++)
				{

					MeshVertex vertex = new MeshVertex(
						new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z),
						new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z),
						mesh.HasTextureCoords(0) ? new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y) : Vector2.Zero
					);

					loadedMesh.Vertices.Add(vertex);


					// Podziel wierzchołki na części w zależności od materiału
					int materialIndex = mesh.MaterialIndex;
					if (!loadedMesh.MaterialParts.ContainsKey(materialIndex))
						loadedMesh.MaterialParts[materialIndex] = new List<MeshVertex>();

					loadedMesh.MaterialParts[materialIndex].Add(vertex);
				}
			}

			// Przetwórz materiały z sceny, jeśli są dostępne.
			if (scene.Materials.Count > 0)
			{
				loadedMesh.LoadedMaterials = new();

				foreach (var material in scene.Materials)
				{
					loadedMesh.LoadedMaterials.Add(new Material(material));

				}
			}

			// Utwórz bufor OpenGL dla wierzchołków
			loadedMesh.VertexArrayObject = GL.GenVertexArray();
			loadedMesh.VertexBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, loadedMesh.VertexBufferObject);
			float[] verts = loadedMesh.ExtractVerts();
			GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

			GL.BindVertexArray(loadedMesh.VertexArrayObject);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
			GL.EnableVertexArrayAttrib(loadedMesh.VertexArrayObject, 0);


			LoadedMeshes[filePath] = loadedMesh;
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
			return loadedMesh;
		}
		
		public void Render(int materialIndex)
		{
			if (!MaterialParts.ContainsKey(materialIndex))
				return;
			GL.BindVertexArray(VertexArrayObject);
			/*// Ustawienia atrybutów dla wierzchołków (position, normal, texCoord)
			int positionLocation = 0; // Przyjmujemy, że atrybuty w shaderze są ustawione na lokalizacjach 0, 1 i 2
			GL.EnableVertexAttribArray(positionLocation);
			GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<MeshVertex>(), 0);

			int normalLocation = 1;
			GL.EnableVertexAttribArray(normalLocation);
			GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<MeshVertex>(), Vector3.SizeInBytes);

			/*int texCoordLocation = 2;
			*GL.EnableVertexAttribArray(texCoordLocation);
			GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<MeshVertex>(), Vector3.SizeInBytes * 2);
			*/
			// Renderowanie
			GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, Vertices.Count);

			// Wyłączenie atrybutów po renderowaniu
			//GL.DisableVertexAttribArray(positionLocation);
			//GL.DisableVertexAttribArray(normalLocation);
			//GL.DisableVertexAttribArray(texCoordLocation);
		}
	}
	public struct MeshVertex
	{
		public Vector3 Position { get; }
		public Vector3 Normal { get; }
		public Vector2 TexCoord { get; }

		public MeshVertex(Vector3 position, Vector3 normal, Vector2 texCoord)
		{
			Position = position;
			Normal = normal;
			TexCoord = texCoord;
		}
	}
}