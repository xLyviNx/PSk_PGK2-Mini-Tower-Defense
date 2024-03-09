using Game.Engine.Components;
using PGK2.Engine.SceneSystem;
using PGK2.Engine.Serialization.Converters;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Core
{
    [Serializable]
    public class GameObject
    {
        [JsonInclude] public Guid Id;
		[JsonIgnore] public string name;
        private bool _isdestroyed = false;
        [JsonIgnore] public bool isDestroyed { get => _isdestroyed; }
		public GameObjectComponents Components;
		public TransformComponent transform;
		public TagsContainer Tags { get; private set; }
		public bool IsActiveSelf { get; private set; } = true;
		[JsonIgnore] public bool IsActive
        {
            get
            {
                return IsActiveSelf && transform != null && (transform.Parent == null || transform.Parent.gameObject != null && transform.Parent.gameObject.IsActive);
            }
            set
            {
                IsActiveSelf = value;
            }
        }
        public GameObject() : this("GameObject") { }
        public GameObject(string name)
        {
            Console.WriteLine("MADE OBJECT");
            Components = new GameObjectComponents(this);
            transform = new TransformComponent(this);
            Tags = new();
            this.name = name;
			Id = Guid.NewGuid();
		}
        public void Update()
        {
            if (!IsActive) return;
            foreach(Component c in Components.All)
            {
                if (!c.Enabled) continue;
                c.Update();
            }
        }
		public void Destroy()
        {
            if (_isdestroyed || Components == null) return;
            foreach (var component in Components.All)
            {
                component.OnDestroy();
            }
            Components.All.Clear();
            if (SceneManager.ActiveScene!=null)
            {
                SceneManager.ActiveScene.GameObjects.Remove(this);
            }
            foreach(TransformComponent child in transform.children.AllObjects)
            {
                child.gameObject.Destroy();
            }
            _isdestroyed = true;
        }
    }
    [Serializable]
    public class GameObjectComponents
    {
		private GameObject gameObject;
		[JsonConverter(typeof(ComponentListConverter))]
		public List<Component> All { get; private set; }

        public GameObjectComponents(GameObject gObj)
        {
            gameObject = gObj;
            All = new List<Component>();
        }

        public T Add<T>() where T : Component, new()
        {
            Component.assigningComponentTo = gameObject;
            T newComponent = new T();
            All.Add(newComponent);
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
