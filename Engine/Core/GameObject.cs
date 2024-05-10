using PGK2.Engine.Components.Base;
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
        [JsonIgnore] private SceneSystem.Scene? _myscene;
		[JsonIgnore]
        public SceneSystem.Scene? MyScene
        {
            get => _myscene;
            set
            {
                if(ReferenceEquals(_myscene, value) == false)
                {
                    if(_myscene!=null)
                    {
                        _myscene.GameObjects.Remove(this);
                    }
					OnSceneTransfer.Invoke(_myscene);
                    if (value != null)
                        if (!value.GameObjects.Contains(this))
                        {
                            value.GameObjects.Add(this);
                        }
					_myscene = value;
				}
			}
		}
		private bool _isdestroyed = false;
		[JsonIgnore] public Action<SceneSystem.Scene?> OnSceneTransfer = delegate { };
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
            transform = Components.Add<TransformComponent>();
            Tags = new();
            this.name = name;
			Id = Guid.NewGuid();
            if (MyScene == null)
                MyScene = SceneManager.ActiveScene;
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
                component.Enabled = false;
                component.OnDestroy();
            }
            Components.All.Clear();
            if (MyScene != null)
            {
				MyScene.GameObjects.Remove(this);
            }
            foreach(TransformComponent child in transform.Children.AllObjects)
            {
                child.gameObject.Destroy();
            }
            _isdestroyed = true;
        }
		public override string ToString()
		{
            if (this != null)
                return $"GameObject ({name} - {Id})";
            else if (this._isdestroyed)
                return "null (destroyed)";
            else
                return "null";
        }

		public static bool operator ==(GameObject? x, GameObject? y)
		{
			//Console.WriteLine($"EQUALS CHECK\n - X null? {ReferenceEquals(x, null)}\n - Y null? {ReferenceEquals(y, null)}");

            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null)) //x==null
            {
                if (!ReferenceEquals(y, null)) //y!=null
					return y._isdestroyed;
            }
            else if (ReferenceEquals(y, null))
                    return x._isdestroyed;

            if (ReferenceEquals(x, null) && !ReferenceEquals(y, null))
                return false;  
            if (ReferenceEquals(y, null) && !ReferenceEquals(x, null))
                return false;

			if (x.isDestroyed && y.isDestroyed)
				return true;

			return x.Id == y.Id;
		}

		public static bool operator !=(GameObject? x, GameObject? y)
		{
			return !(x == y);
		}
		public int GetHashCode(GameObject? obj)
		{
			if (ReferenceEquals(obj, null) || obj.isDestroyed)
				return 0;

			return obj.Id.GetHashCode();
		}

		public override bool Equals(object? obj) => this == obj;

		public override int GetHashCode()
		{
			if (this==null)
				return 0;

			return Id.GetHashCode();
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
            Console.WriteLine("Setting component container to '" + gameObject + "'");
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
