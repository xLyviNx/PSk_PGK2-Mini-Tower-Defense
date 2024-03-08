using PGK2.Engine.Core;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PGK2.Engine.SceneSystem
{
	[Serializable]
	public class Scene
	{
		public string SceneName = "Unnamed Scene";
		public List<GameObject> GameObjects { get;  set; }
		
		public Scene()
		{
			GameObjects = new List<GameObject>();
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
