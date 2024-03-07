using Game.Engine.Components;
using PGK2.Engine.Main;

namespace PGK2.Engine.Core
{
    public class Scene
	{
		public List<GameObject> GameObjects = new();

		public T? FindObjectOfType<T>(bool onlyActive = true) where T : Component
		{
			foreach (var gameObject in GameObjects)
			{
				if (onlyActive && !gameObject.isActive)
					continue;

				var component = gameObject.components.Get<T>();
				if (component != null && component.enabled)
					return component;
			}

			return null;
		}

		public List<T> FindAllObjectsOfType<T>(bool onlyActive = true) where T : Component
		{
			List<T> result = new List<T>();

			foreach (var gameObject in GameObjects)
			{
				if (onlyActive && !gameObject.isActive)
					continue;

				var component = gameObject.components.Get<T>();
				if (component != null)
					result.Add(component);
			}

			return result;
		}
	}
}