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
		internal List<GameObject> AwaitingGameObjects { get; set; }
		internal List<GameObject> RemovingGameObjects { get; set; }
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
		internal void AddAwaitingObjects()
		{
			List<GameObject> toadd = new();
			foreach (GameObject obj in AwaitingGameObjects)
				if(!obj.isDestroyed && obj!=null)
					toadd.Add(obj);
			GameObjects.InsertRange(GameObjects.Count, toadd);
			AwaitingGameObjects.Clear();
		}	
		internal void RemoveAwaitingObjects()
		{
			foreach (GameObject obj in RemovingGameObjects)
				GameObjects.Remove(obj);
			RemovingGameObjects.Clear();
		}
		public Scene()
		{
			AwaitingGameObjects = new List<GameObject>();
			RemovingGameObjects = new List<GameObject>();
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
				if (component != null && component.EnabledInHierarchy)
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
