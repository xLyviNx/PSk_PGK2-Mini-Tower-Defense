using PGK2.Engine.Serialization.Converters;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Core
{
	[Serializable]
    public class GameObjectComponents
    {
		internal GameObject gameObject;
		[JsonConverter(typeof(ComponentListConverter))]
		public List<Component> All { get; internal set; }

		// Add a parameterless constructor
		public GameObjectComponents()
		{
			All = new List<Component>();
		}

		public GameObjectComponents(GameObject gObj)
		{
			gameObject = gObj;
			All = new List<Component>();
		}

		public T Add<T>() where T : Component, new()
        {
            //Console.WriteLine("Setting component container to '" + gameObject + "'");
            Component.assigningComponentTo = gameObject;
			T newComponent = new T();
			All.Add(newComponent);
            newComponent.OnSceneTransfer?.Invoke(null);
			EngineWindow.StartQueue.Enqueue(newComponent);
			return newComponent;
        }

        public T? Get<T>() where T : Component
        {
            foreach (var component in All)
            {
                if (component is T typedComponent)
                {
                    return typedComponent;
                }
            }
            return null;
        }
    }
}
