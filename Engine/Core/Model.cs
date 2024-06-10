using Assimp;
using PGK2.Engine.Components.Base;
using Assimp.Configs;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Klasa reprezentująca model 3D.
	/// </summary>
	public class Model
	{
		/// <summary>
		/// Słownik zawierający załadowane modele, gdzie kluczem jest ścieżka do pliku, a wartością instancja klasy Model.
		/// </summary>
		public static Dictionary<string, Model> LoadedModels = new();

		/// <summary>
		/// Ścieżka do pliku z modelem.
		/// </summary>
		public string Path;

		/// <summary>
		/// Pole otaczające model (bounding box).
		/// </summary>
		public BoundingBox ModelBoundingBox;

		/// <summary>
		/// Metoda statyczna ładująca model z pliku.
		/// </summary>
		/// <param name="path">Ścieżka do pliku z modelem.</param>
		/// <returns>Instancja klasy Model reprezentująca załadowany model.</returns>
		public static Model LoadFromFile(string path)
		{
			if (LoadedModels.ContainsKey(path) && LoadedModels[path] != null)
				return LoadedModels[path];

			Model model = new Model(path);
			LoadedModels[path] = model;
			return model;
		}

		/// <summary>
		/// Konstruktor klasy Model inicjujący obiekt dla podanego pliku.
		/// </summary>
		/// <param name="path">Ścieżka do pliku z modelem.</param>
		public Model(string path)
		{
			loadModel(path);
		}

		/// <summary>
		/// Konstruktor prywatny - domyślny.
		/// </summary>
		internal Model()
		{
		}

		/// <summary>
		/// Metoda rysująca model.
		/// </summary>
		/// <param name="modelMatrix">Macierz modelu.</param>
		/// <param name="viewMatrix">Macierz widoku.</param>
		/// <param name="projectionMatrix">Macierz projekcji.</param>
		/// <param name="lights">Kolekcja świateł w scenie.</param>
		/// <param name="camera">Komponent kamery.</param>
		/// <param name="RenderPass">Rodzaj renderowania (np. nieprzezroczyste, przezroczyste).</param>
		/// <param name="overrideMaterial">Materiał zastępujący domyślny materiał modelu (opcjonalny).</param>
		/// <param name="RendererMaterials">Tablica materiałów zastępujących domyślne (opcjonalna).</param>
		public void Draw(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix, List<Light> lights, CameraComponent camera, EngineInstance.RenderPass RenderPass, Material? overrideMaterial = null, Material?[]? RendererMaterials = null)
		{
			int i = 0;
			foreach (var mesh in meshes)
			{
				bool overrided = (RendererMaterials != null && RendererMaterials.Length >= i && RendererMaterials[i] != null);
				Material? mat = overrided ? RendererMaterials[i] : mesh.Material;
				bool transparentmat = (mesh.hasTransparentTextures || mat.HasTransparency);
				bool Transparent = (transparentmat && RenderPass == EngineInstance.RenderPass.Transparent);
				bool Opaque = (RenderPass == EngineInstance.RenderPass.Opaque && !transparentmat);
				bool Outline = (RenderPass == EngineInstance.RenderPass.Outline);

				if (Transparent || Opaque || Outline)
				{
					mesh.Draw(modelMatrix, viewMatrix, projectionMatrix, lights, camera, overrideMaterial != null ? overrideMaterial : (overrided ? mat : null));
				}
				i++;
			}
		}

		/// <summary>
		/// Lista meshy (siatek) tworzących model.
		/// </summary>
		public List<Mesh> meshes = new();

		private string directory;

		/// <summary>
		/// Prywatna metoda ładująca model z pliku.
		/// </summary>
		/// <param name="path">Ścieżka do pliku z modelem.</param>
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

		/// <summary>
		/// Prywatna metoda przetwarzająca węzeł sceny z pliku Assimp.
		/// </summary>
		/// <param name="node">Węzeł sceny.</param>
		/// <param name="scene">Scena z pliku Assimp.</param>
		private void processNode(Assimp.Node node, in Assimp.Scene scene)
		{
			for (int i = 0; i < node.MeshCount; i++)
			{
				Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
				Mesh processedMesh = processMesh(mesh, scene);
				meshes.Add(processedMesh);

				// Update the model bounding box
				UpdateModelBoundingBox(processedMesh.CalculateBoundingBox());
			}

			// then do the same for each of its children
			for (int i = 0; i < node.ChildCount; i++)
			{
				processNode(node.Children[i], scene);
			}
		}

		/// <summary>
		/// Prywatna metoda aktualizująca pole otaczające model (bounding box).
		/// </summary>
		/// <param name="meshBoundingBox">Pole otaczające pojedynczego mesha.</param>
		private void UpdateModelBoundingBox(BoundingBox meshBoundingBox)
		{
			if (ModelBoundingBox == null)
				ModelBoundingBox = new BoundingBox(meshBoundingBox.Min, meshBoundingBox.Max);
			else
			{
				ModelBoundingBox.Min = Vector3.ComponentMin(ModelBoundingBox.Min, meshBoundingBox.Min);
				ModelBoundingBox.Max = Vector3.ComponentMax(ModelBoundingBox.Max, meshBoundingBox.Max);
			}
		}

		/// <summary>
		/// Prywatna metoda przetwarzająca mesh z pliku Assimp.
		/// </summary>
		/// <param name="mesh">Mesh z pliku Assimp.</param>
		/// <param name="scene">Scena z pliku Assimp.</param>
		/// <returns>Przetworzony obiekt klasy Mesh.</returns>
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

				mat = new Material(material);
			}

			return new Mesh(vertices, indices, textures, mat);
		}

		/// <summary>
		/// Prywatna metoda ładująca tekstury materiału.
		/// </summary>
		/// <param name="mat">Materiał z pliku Assimp.</param>
		/// <param name="type">Typ tekstury (np. diffuse, specular).</param>
		/// <param name="typeName">Nazwa typu tekstury używana do tworzenia ścieżki.</param>
		/// <returns>Lista załadowanych tekstur.</returns>
		private List<Texture> LoadMaterialTextures(Assimp.Material mat, TextureType type, string typeName)
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
