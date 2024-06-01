using PGK2.Engine.Serialization.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PGK2.Engine.SceneSystem
{
	public class SceneManager
	{
		private static List<Scene> scenes = new List<Scene>();
		private static Scene? activeScene;

		public static Scene? ActiveScene
		{
			get { return activeScene; }
		}

		public static void LoadScene(Scene scene)
		{
			if (!scenes.Contains(scene))
			{
				scenes.Add(scene);
			}

			SetActiveScene(scene);
		}

		public static void UnloadScene(Scene scene)
		{
			if (scenes.Contains(scene))
			{
				scenes.Remove(scene);
			}

			if (activeScene == scene)
			{
				activeScene = null;
			}
		}

		private static void SetActiveScene(Scene? scene)
		{
			if (scenes.Contains(scene) || scene == null)
			{
				activeScene = scene;
			}
		}

		public static void SaveSceneToFile(Scene scene, string filePath)
		{
			try
			{
				using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
				{
					JsonSerializerOptions options = new JsonSerializerOptions
					{
						Converters =
				{
					new Vector3Converter(),
					new QuaternionConverter(),
					new ComponentListConverter(),
					new GameObjectConverter(),
					new GameObjectListConverter()
				},
						WriteIndented = true,
						DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
						IncludeFields = true
					};
					JsonSerializer.Serialize(fileStream, scene, options);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to serialize. Reason: " + e.Message);
			}
		}

		public static Scene? LoadSceneFromFile(string filePath)
		{
			Scene? loadedScene = null;

			try
			{
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
				{
					JsonSerializerOptions options = new JsonSerializerOptions
					{
						Converters =
				{
					new Vector3Converter(),
					new QuaternionConverter(),
					new ComponentListConverter(),
					new GameObjectConverter(),
					new GameObjectListConverter()
				}
					};

					loadedScene = JsonSerializer.Deserialize<Scene>(fileStream, options);

					// Restore parent-child relationships
					RestoreParentChildRelationships(loadedScene);
					Console.WriteLine($"Scene Loaded.\n" +
									  $" Scene Name: {loadedScene.SceneName}\n" +
									  $" GameObjects: {loadedScene.GameObjects.Count}\n" +
									  $" Renderers: {loadedScene.Renderers.Count}\n" +
									  $" UI Renderers: {loadedScene.UI_Renderers.Count}\n" +
									  $" Lights: {loadedScene.Lights.Count}\n");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to deserialize. Reason: " + e.Message + '\n' + e.StackTrace);
			}

			return loadedScene;
		}

		private static void RestoreParentChildRelationships(Scene scene)
		{
			foreach (var gameObject in scene.GameObjects)
			{
				foreach (var childId in gameObject.transform.Children.All)
				{
					var child = scene.GameObjects.Find(obj => obj.Id == childId);
					if (child != null)
					{
						child.transform.Parent = gameObject.transform;
					}
				}
			}
		}
	}
}
