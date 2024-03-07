using Game.Engine.Components;
using PGK2.Engine.SceneSystem;

namespace PGK2.Engine.Core
{
    public class GameObject
    {
		[SerializeField] public TagsContainer RenderTags { get; private set; }
		[SerializeField] public string name;
        private bool _isdestroyed = false;
        public bool isDestroyed { get => _isdestroyed; }
		[SerializeField] public GameObjectComponents Components;
		[SerializeField] public TransformComponent transform;
		[SerializeField] public TagsContainer Tags { get; private set; }
		[SerializeField] public bool IsActiveSelf { get; private set; } = true;
        public bool IsActive
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
            Components = new GameObjectComponents(this);
            transform = new TransformComponent();
            Tags = new();
            RenderTags = new();
            this.name = name;
        }
        public void Destroy()
        {
            if (_isdestroyed || Components == null) return;
            foreach (var component in Components.All)
            {
                component.gameObject = null;
            }
            _isdestroyed = true;
        }
    }
    public class GameObjectComponents
    {
		[SerializeField] public GameObject gameObject;
		[SerializeField] public List<Component> All { get; private set; }

        public GameObjectComponents(GameObject gObj)
        {
            gameObject = gObj;
            All = new List<Component>();
        }

        public T Add<T>() where T : Component, new()
        {
            T newComponent = new T();
            newComponent.gameObject = gameObject;
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
