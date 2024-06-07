using Assimp;
using PGK2.Engine.Components.Base;
using Assimp.Configs;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace PGK2.Engine.Core
{
	public class Model
	{
		public static Dictionary<string, Model> LoadedModels = new();
		public string Path;
		
		public static Model LoadFromFile(string path)
		{
			if (LoadedModels.ContainsKey(path) && LoadedModels[path] != null)
				return LoadedModels[path];
			Model model = new Model(path);
			LoadedModels[path] = model;
			return model;
		}

		public Model(string path)
		{
			loadModel(path);
		}
		internal Model()
		{
		}
		public void Draw(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix, List<Light> lights, CameraComponent camera, EngineInstance.RenderPass RenderPass, Material? overrideMaterial = null, Material?[]? RendererMaterials = null)
		{
			int i = 0;
			foreach (var mesh in meshes)
			{
				Material? mat = RendererMaterials != null && RendererMaterials.Length>=i && RendererMaterials[i] != null ? RendererMaterials[i] : mesh.Material;
				mat = mesh.Material;
				string matstring = "";
				try
				{
					matstring = $"{mat.Vector3Values["material.diffuse"]}";
				}
				catch(Exception ex)
				{
					matstring = ex.Message;
				}
				Console.WriteLine($"MAT:{(RendererMaterials != null && RendererMaterials.Length >= i && RendererMaterials[i] != null ? "OVERRIDED" : "NORMAL")}, {matstring}");
				bool Transparent = ((mesh.hasTransparentTextures || mat.HasTransparency) && RenderPass == EngineInstance.RenderPass.Transparent);
				bool Opaque = (RenderPass == EngineInstance.RenderPass.Opaque && !mesh.hasTransparency);
				bool Outline = (RenderPass == EngineInstance.RenderPass.Outline);

				//Console.WriteLine($"{Transparent}, {Opaque}");
				if (Transparent || Opaque || Outline)
				{

					mesh.Draw(modelMatrix, viewMatrix, projectionMatrix, lights, camera, overrideMaterial != null? overrideMaterial : mat);
				}
				//Console.WriteLine($"DRAWN");
				i++;
			}
		}

		public List<Mesh> meshes = new();
		private string directory;

		private void loadModel(string path)
		{
			Console.WriteLine($"Loading MODEL: '{path}'");
			AssimpContext context = new AssimpContext();
			context.SetConfig(new NormalSmoothingAngleConfig(66.0f));

			Assimp.Scene scene = context.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

			if (scene == null || scene.Meshes.Count == 0)
				throw new Exception("Failed to load mesh.");

			directory = path.Substring(0, path.LastIndexOf('/'));
			processNode(scene.RootNode, scene);
			Path = path;
			
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
			List<uint> indices = new();
			List<Texture> textures = new();

			for (int i = 0; i < mesh.VertexCount; i++)
			{
				MeshVertex vertex = new();

				// Transformacja z Blender do OpenGL
				Vector3 vector;
				vector.X = mesh.Vertices[i].X;
				vector.Y = mesh.Vertices[i].Z; // Z Blender -> Y OpenGL
				vector.Z = -mesh.Vertices[i].Y; // Y Blender -> -Z OpenGL
				vertex.Position = vector;

				// Transformacja normalnych
				vector.X = mesh.Normals[i].X;
				vector.Y = mesh.Normals[i].Z; // Z Blender -> Y OpenGL
				vector.Z = -mesh.Normals[i].Y; // Y Blender -> -Z OpenGL
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

				vertices.Add(vertex);
			}

			for (int i = 0; i < mesh.FaceCount; i++)
			{
				Assimp.Face face = mesh.Faces[i];
				for (int j = 0; j < face.IndexCount; j++)
					indices.Add((uint)face.Indices[j]);
			}

			// Process material
			Material mat = new(EngineWindow.shader);
			if (mesh.MaterialIndex >= 0)
			{
				Assimp.Material material = scene.Materials[mesh.MaterialIndex];
				List<Texture> diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse");
				textures.AddRange(diffuseMaps);
				List<Texture> specularMaps = LoadMaterialTextures(material, TextureType.Specular, "texture_specular");
				textures.AddRange(specularMaps);

				mat = new(material);
			}

			return new Mesh(vertices, indices, textures, mat);
		}
		List<Texture> LoadMaterialTextures(Assimp.Material mat, TextureType type, string typeName)
		{
			List<Texture> textures = new List<Texture>();
			for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
			{
				TextureSlot textureSlot;
				mat.GetMaterialTexture(type, i, out textureSlot);
				Texture texture = Texture.LoadFromFile(textureSlot.FilePath, directory);
				textures.Add(texture);
			}
			return textures;
		}
	}
}
