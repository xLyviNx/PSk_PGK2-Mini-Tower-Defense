using Assimp;
using PGK2.Engine.Components.Base;
using Assimp.Configs;
using OpenTK.Mathematics;
using Assimp.Unmanaged;
using System.Numerics;

namespace PGK2.Engine.Core
{
	public class Model
	{

		public Model(string path)
		{
			loadModel(path);
		}
		public void Draw(ref CameraComponent camera)
		{
			foreach(var mesh in meshes)
				mesh.Draw(ref camera);
		}

		private List<Mesh> meshes;
		private string directory;

		private void loadModel(string path)
		{
			AssimpContext context = new AssimpContext();
			context.SetConfig(new NormalSmoothingAngleConfig(66.0f));

			Assimp.Scene scene = context.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

			if (scene == null || scene.Meshes.Count == 0)
				throw new Exception("Failed to load mesh.");

			directory = path.Substring(0, path.LastIndexOf('/'));
			processNode(scene.RootNode, scene);
		}
		private void processNode(Assimp.Node node, in Assimp.Scene scene)
		{
			for (int i = 0; i < node.MeshCount; i++)
			{
				Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
				meshes.Add(processMesh(mesh, scene));
			}
			// then do the same for each of its children
			for (int i = 0; i < node.ChildCount; i++)
			{
				processNode(node.Children[i], scene);
			}
		}

		private Mesh processMesh(Assimp.Mesh mesh, in Assimp.Scene scene)
		{
			List<MeshVertex> vertices = new();
			List<uint> indices=new();
			List<Texture> textures=new();

			for (int i = 0; i < mesh.VertexCount; i++)
			{
				MeshVertex vertex = new();

				Vector3 vector;
				vector.X = mesh.Vertices[i].X;
				vector.Y = mesh.Vertices[i].Y;
				vector.Z = mesh.Vertices[i].Z;
				vertex.Position = vector;

				vertices.Add(vertex);

				vector.X = mesh.Normals[i].X;
				vector.Y = mesh.Normals[i].Y;
				vector.Z = mesh.Normals[i].Z;
				vertex.Normal = vector;

				if (mesh.HasTextureCoords(0))
				{
					Vector2 vec;
					vec.X = mesh.TextureCoordinateChannels[0][i].X;
					vec.Y = mesh.TextureCoordinateChannels[0][i].Y;
					vertex.TexCoords = vec;
				}
				else
					vertex.TexCoords = new Vector2(0.0f, 0.0f);
			}
			for (int i = 0; i < mesh.FaceCount; i++)
			{
				Assimp.Face face = mesh.Faces[i];
				for (int j = 0; j < face.IndexCount; j++)
					indices.Add((uint)face.Indices[j]);
			}
			// process material
			if (mesh.MaterialIndex >= 0)
			{
				Assimp.Material material = scene.Materials[mesh.MaterialIndex];
				List<Texture> diffuseMaps = LoadMaterialTextures(material,
													TextureType.Diffuse, "texture_diffuse");
				textures.AddRange(diffuseMaps);
				List<Texture> specularMaps = LoadMaterialTextures(material,
													TextureType.Specular, "texture_specular");
				textures.AddRange(specularMaps);
			}
			return new Mesh(vertices, indices, textures);
		}
		List<Texture> LoadMaterialTextures(Assimp.Material mat, TextureType type, string typeName)
		{
			List<Texture> textures = new List<Texture>();
			for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
			{
				TextureSlot textureSlot;
				mat.GetMaterialTexture(type, i, out textureSlot);
				Texture texture = new Texture();
				texture.Id = TextureFromFile(textureSlot.FilePath, directory);
				texture.Type = typeName;
				texture.Path = textureSlot.FilePath;
				textures.Add(texture);
			}
			return textures;
		}
	}
