using Newtonsoft.Json;
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
			using (StreamWriter sw = new StreamWriter(filePath))
			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				JsonSerializer serializer = new JsonSerializer
				{
					ContractResolver = new SceneContractResolver(),
					Formatting = Formatting.Indented,
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore
				};

				serializer.Serialize(writer, scene);
			}
		}


	public static Scene? LoadSceneFromFile(string filePath)
		{
			string json = File.ReadAllText(filePath);
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ContractResolver = new SceneContractResolver()
			};

			return JsonConvert.DeserializeObject<Scene>(json, settings);
		}
	}
}
