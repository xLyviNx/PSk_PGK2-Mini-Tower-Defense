using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using System.Text.Json.Serialization;
using PGK2.Engine.Components;
namespace PGK2.Engine.SceneSystem
{
    [Serializable]
	public class Scene
	{
		public string SceneName = "Unnamed Scene";
		public List<GameObject> GameObjects { get; internal set; }
		[JsonIgnore] public List<CameraComponent> Cameras { get; internal set; }
		[JsonIgnore] public List<Renderer> Renderers { get; internal set; }
		[JsonIgnore] public List<UI_Renderer> UI_Renderers { get; internal set; }
		[JsonIgnore] public List<Light> Lights { get; internal set; }
		/*[JsonInclude]
		public List<Guid> CameraObjects
		{
			get
			{
				List<Guid> list = new();
				foreach(CameraComponent c in Cameras)
				{
					if (c != null && c.gameObject != null)
						list.Add(c.gameObject.Id);
				}
				return list;
			}
		}*/
		
		public Scene()
		{
			GameObjects = new List<GameObject>();
			Cameras = new();
			Renderers = new();
			UI_Renderers = new();
			Lights = new();
		}
		public static GameObject CreateObject(string ObjectName = "GameObject")
		{
			if(SceneManager.ActiveScene!=null)
			{
				SceneManager.ActiveScene.CreateSceneObject(ObjectName);
			}
			throw new Exception("Scene not set");
		}
		public GameObject CreateSceneObject(string ObjectName = "GameObject")
		{
			GameObject gameObject = new GameObject(ObjectName);
			gameObject.MyScene = this;

			return gameObject;
		}
		public T? FindObjectOfType<T>(bool onlyActive = true) where T : Component
		{
			foreach (var gameObject in GameObjects)
			{
				if (onlyActive && !gameObject.IsActive)
					continue;

				var component = gameObject.Components.Get<T>();
				if (component != null && component.Enabled)
					return component;
			}

			return null;
		}

		public List<T> FindAllObjectsOfType<T>(bool onlyActive = true) where T : Component
		{
			List<T> result = new List<T>();

			foreach (var gameObject in GameObjects)
			{
				if (onlyActive && !gameObject.IsActive)
					continue;

				var component = gameObject.Components.Get<T>();
				if (component != null)
					result.Add(component);
			}

			return result;
		}
	}
}
