using System;
using System.Collections.Generic;
using Assimp;
using Assimp.Configs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace PGK2.Engine.Core
{
	public class Mesh
	{
		private static Dictionary<string, Mesh> LoadedMeshes = new();

		// Bufor OpenGL dla wierzchołków
		private int VertexBufferObject;

		public List<Material> LoadedMaterials { get; private set; }
		public List<MeshVertex> Vertices { get; private set; } 

		public Mesh()
		{
			Vertices = new List<MeshVertex>();
			VertexBufferObject = 0;
			LoadedMaterials = new();
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
					loadedMesh.Vertices.Add(new MeshVertex(

						new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z),
						new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z),
						mesh.HasTextureCoords(0) ? new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y) : Vector2.Zero
					));
				}
			}

			// Przetwórz materiały z sceny, jeśli są dostępne.
			if (scene.Materials.Count > 0)
			{
				loadedMesh.LoadedMaterials = new();
				foreach(var material in scene.Materials)
				{
					loadedMesh.LoadedMaterials.Add(new Material(material));
				}
			}

			// Utwórz bufor OpenGL dla wierzchołków
			GL.GenBuffers(1, out loadedMesh.VertexBufferObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, loadedMesh.VertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, loadedMesh.Vertices.Count * Marshal.SizeOf(typeof(MeshVertex)), loadedMesh.Vertices.ToArray(), BufferUsageHint.StaticDraw);

			LoadedMeshes[filePath] = loadedMesh;

			return loadedMesh;
		}
		public void Render()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

			// Ustawienia atrybutów dla wierzchołków (position, normal, texCoord)
			int positionLocation = 0; // Przyjmujemy, że atrybuty w shaderze są ustawione na lokalizacjach 0, 1 i 2
			GL.EnableVertexAttribArray(positionLocation);
			GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<MeshVertex>(), 0);

			int normalLocation = 1;
			GL.EnableVertexAttribArray(normalLocation);
			GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<MeshVertex>(), Vector3.SizeInBytes);

			int texCoordLocation = 2;
			GL.EnableVertexAttribArray(texCoordLocation);
			GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<MeshVertex>(), Vector3.SizeInBytes * 2);

			// Renderowanie
			GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, Vertices.Count);

			// Wyłączenie atrybutów po renderowaniu
			GL.DisableVertexAttribArray(positionLocation);
			GL.DisableVertexAttribArray(normalLocation);
			GL.DisableVertexAttribArray(texCoordLocation);
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
