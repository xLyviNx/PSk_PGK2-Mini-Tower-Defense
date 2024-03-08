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
					loadedMesh.Vertices.Add(new MeshVertex
					{
						Position = new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z),
						Normal = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z),
						TexCoord = mesh.HasTextureCoords(0) ? new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y) : Vector2.Zero
					});
				}
			}

			// Przetwórz materiały z sceny, jeśli są dostępne.
			if (scene.Materials.Count > 0)
			{
				loadedMesh.LoadedMaterials = new Material[scene.Materials.Count];
				for (int i = 0; i < scene.Materials.Count; i++)
				{
					// Przetwórz informacje o materiale, takie jak kolor, tekstury itd.
					// Utwórz obiekty Material i dodaj je do loadedMesh.LoadedMaterials.
				}
			}

			// Utwórz bufor OpenGL dla wierzchołków
			GL.GenBuffers(1, out loadedMesh.VertexBufferObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, loadedMesh.VertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, loadedMesh.Vertices.Count * Marshal.SizeOf(typeof(MeshVertex)), loadedMesh.Vertices.ToArray(), BufferUsageHint.StaticDraw);

			LoadedMeshes[filePath] = loadedMesh;

			return loadedMesh;
		}
	}

	public class MeshVertex
	{
		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }
		public Vector2 TexCoord { get; set; }
	}
}
