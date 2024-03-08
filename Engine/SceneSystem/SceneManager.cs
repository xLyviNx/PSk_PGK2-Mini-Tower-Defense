using PGK2.Engine.Serialization.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PGK2.Engine.SceneSystem
{
	public class SceneManager
	{
		private List<Scene> scenes = new List<Scene>();
		private Scene? activeScene;

		public Scene? ActiveScene
		{
			get { return activeScene; }
		}

		public void LoadScene(Scene scene)
		{
			if (!scenes.Contains(scene))
			{
				scenes.Add(scene);
			}

			SetActiveScene(scene);
		}

		public void UnloadScene(Scene scene)
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

		private void SetActiveScene(Scene? scene)
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
					JsonSerializerOptions options= new JsonSerializerOptions();
					options.Converters.Add(new Vector3Converter());					
					options.Converters.Add(new QuaternionConverter());
					options.Converters.Add(new ComponentListConverter());
					options.Converters.Add(new GameObjectListConverter());  // Dodaj nowy konwerter GameObjectList

					options.WriteIndented = true;
					options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
					options.IncludeFields = true;
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
					//XmlSerializer xmlSerializer = new XmlSerializer(typeof(Scene));
					//loadedScene = (Scene)xmlSerializer.Deserialize(fileStream);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to deserialize. Reason: " + e.Message + '\n' + e.StackTrace);
			}

			return loadedScene;
		}
	}
}
