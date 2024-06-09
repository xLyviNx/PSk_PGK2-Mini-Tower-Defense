using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using PGK2.Engine.Serialization.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PGK2.Engine.SceneSystem
{
	/// <summary>
	/// Klasa zarządzająca scenami w grze.
	/// </summary>
	public class SceneManager
	{
		/// <summary>
		/// Lista scen w grze.
		/// </summary>
		private static List<Scene> scenes = new List<Scene>();

		/// <summary>
		/// Aktualna aktywna scena.
		/// </summary>
		private static Scene? activeScene;

		/// <summary>
		/// Aktywna scena.
		/// </summary>
		public static Scene? ActiveScene => activeScene;

		/// <summary>
		/// Ładuje scenę jako aktywną.
		/// </summary>
		/// <param name="scene">Ładowana scena.</param>
		public static void LoadScene(Scene scene)
		{
			if (!scenes.Contains(scene))
			{
				scenes.Add(scene);
			}

			SetActiveScene(scene);
		}
		/// <summary>
		/// Asynchronicznie zmienia aktualną scenę.
		/// </summary>
		/// <param name="newScene">Nowa scena.</param>
		public async static void ChangeSceneAsync(Scene newScene)
		{
			if (activeScene != null)
			{
				await UnloadSceneAsync(activeScene);
			}

			if (!scenes.Contains(newScene))
			{
				scenes.Add(newScene);
			}

			SetActiveScene(newScene);
		}

		/// <summary>
		/// Asynchronicznie usuwa scenę i czeka na zakończenie klatki.
		/// </summary>
		/// <param name="scene">Scena do usunięcia.</param>
		private async static Task UnloadSceneAsync(Scene scene)
		{
			UnloadScene(scene);
			await EngineWindow.instance.WaitForEndOfFrame();
		}
		/// <summary>
		/// Usuwa scenę po zakończeniu klatki.
		/// </summary>
		/// <param name="scene">Scena do usunięcia.</param>
		public async static void UnloadScene(Scene scene)
		{
			await EngineWindow.instance.WaitForEndOfFrame();
			CameraComponent.activeCamera = null;
			scene.Cameras.Clear();
			scene.GameObjects.Clear();
			scene.Renderers.Clear();
			scene.UI_Renderers.Clear();
			scene.Lights.Clear();
			scene.RemovingGameObjects.Clear();
			if (scenes.Contains(scene))
			{
				scenes.Remove(scene);
			}

			if (activeScene == scene)
			{
				activeScene = null;
			}
		}
		/// <summary>
		/// Ustawia aktywną scenę.
		/// </summary>
		/// <param name="scene">Scena do ustawienia jako aktywna.</param>
		private static void SetActiveScene(Scene? scene)
		{
			if (scenes.Contains(scene) || scene == null)
			{
				activeScene = scene;
			}
		}
		/// <summary>
		/// Zapisuje scenę do pliku.
		/// </summary>
		/// <param name="scene">Zapisywana scena.</param>
		/// <param name="filePath">Ścieżka do pliku.</param>
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
					new GameObjectListConverter(),
					new SceneConverter(),
					new GameObjectComponentsConverter(),
					new ChildrenContainerConverter(),
					new TransformComponentConverter(),
					new TagsContainerConverter(),

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

		/// <summary>
		/// Ładuje scenę z pliku.
		/// </summary>
		/// <param name="filePath">Ścieżka do pliku.</param>
		/// <returns>Ładowana scena.</returns>
		public static Scene? LoadSceneFromFile(string filePath)
		{
			Scene? loadedScene = null;
			try
			{
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
				{
					DeserializeContext.CurrentContext = new();
					JsonSerializerOptions options = new JsonSerializerOptions
					{
						Converters =
				{
					new Vector3Converter(),
					new QuaternionConverter(),
					new ComponentListConverter(),
					new GameObjectConverter(),
					new GameObjectListConverter(),
					new SceneConverter(),
					new GameObjectComponentsConverter(),
					new ChildrenContainerConverter(),
					new TransformComponentConverter(),
					new TagsContainerConverter(),

				},
						WriteIndented = true,
						DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
						IncludeFields = true,
					};
					Console.WriteLine("Starting deserialization...");
					loadedScene = JsonSerializer.Deserialize<Scene>(fileStream, options);
					Console.WriteLine("Deserialization completed.");
					// Restore parent-child relationships
					loadedScene.AddAwaitingObjects();

					RestoreHierarchy(loadedScene);
					LoadAllModels(loadedScene);
					string GameObjectList = "";
					string RenderersList = "";
					foreach (var go in loadedScene.GameObjects)
						GameObjectList += $"   - {go.name}\n";	
					foreach (var go in loadedScene.AwaitingGameObjects)
						GameObjectList += $"   * {go.name}\n";

					foreach (var go in loadedScene.Renderers)
					{
						RenderersList += $"   - {go.gameObject.name}\n";
						var model = go as ModelRenderer;
						if (model != null)
						{
							RenderersList += "       TAGS:\n";
							foreach(var rt in model.gameObject.Tags.All )
							{
								RenderersList += $"         - {rt}";
							}
						}
					}

					Console.WriteLine($"Scene Loaded.\n" +
									  $" Scene Name: {loadedScene.SceneName}\n" +
									  $" GameObjects: {loadedScene.GameObjects.Count}\n" +
									  GameObjectList +
									  $" Renderers: {loadedScene.Renderers.Count}\n" +
									  RenderersList +
									  $" UI Renderers: {loadedScene.UI_Renderers.Count}\n" +
									  $" Lights: {loadedScene.Lights.Count}\n");
					DeserializeContext.CurrentContext = null;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to deserialize. Reason: " + e.Message + '\n' + e.StackTrace);
			}
			return loadedScene;
		}
		/// <summary>
		/// Ładuje modele dla wszystkich rendererów w scenie.
		/// </summary>
		/// <param name="loadedScene">Ładowana scena.</param>
		private static void LoadAllModels(Scene loadedScene)
		{
			Console.WriteLine("\n== SCENE MODEL LOADING STARTED ==");
			foreach(var rend in loadedScene.Renderers)
			{
				ModelRenderer? mrend = rend as ModelRenderer;
				if(mrend !=null)
				{
					Console.WriteLine($"Found Model Renderer with MODEL PATH: {mrend._loadedModelPath}");
					mrend.Model = Model.LoadFromFile(mrend._loadedModelPath);
				}
			}

			Console.WriteLine("== SCENE MODEL LOADING ENDED ==\n");
		}
		/// <summary>
		/// Przywraca hierarchię obiektów w scenie.
		/// </summary>
		/// <param name="scene">Scena, dla której przywracana jest hierarchia.</param>
		internal static void RestoreHierarchy(Scene scene)
		{
			Console.WriteLine("\n== STARTING RESTORING SCENE HIERARCHY ==");
			foreach (var gameObject in scene.GameObjects)
			{
				if (gameObject.transform == null)
					gameObject.transform = gameObject.GetComponent<TransformComponent>();
				foreach (var childId in gameObject.transform.Children._loaded)
				{
					Console.WriteLine($"  {gameObject.name}'s CHILD ID: {childId}");
					var child = scene.GameObjects.Find(obj => obj.Id == childId);
					if (child != null)
					{
						if(child.transform==null)
							child.transform = child.GetComponent<TransformComponent>();
						Console.WriteLine($"   CHILD FOUND. APPLYING.");
						child.transform.Parent = gameObject.transform;
					}
				}
			}

			Console.WriteLine("== RESTORING ENDED ==\n");
		}
	}
}
