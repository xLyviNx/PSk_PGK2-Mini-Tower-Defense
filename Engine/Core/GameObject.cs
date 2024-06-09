using PGK2.Engine.Components.Base;
using PGK2.Engine.SceneSystem;
using System.Text.Json.Serialization;
using System.Xml.Linq;

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
                    Scene? oldScene = _myscene;
                    if(_myscene!=null)
                    {
                        _myscene.GameObjects.Remove(this);
                    }
                    if (value != null)
                        if (!value.GameObjects.Contains(this) && !value.AwaitingGameObjects.Contains(this))
                        {
                            value.AwaitingGameObjects.Add(this);
                        }
					_myscene = value;
					OnSceneTransfer.Invoke(oldScene);
				}
			}
		}
		private bool _isdestroyed = false;
		[JsonIgnore] public Action<SceneSystem.Scene?> OnSceneTransfer = delegate { };
		[JsonIgnore] public bool isDestroyed { get => _isdestroyed; }
		public GameObjectComponents Components;
		public TransformComponent transform;
		public TagsContainer Tags { get; internal set; }
		public bool IsActiveSelf { get; internal set; } = true;
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
		public T? GetComponent<T>() where T : Component
		{
			return Components.Get<T>();
		}
		public T? AddComponent<T>() where T : Component, new()
		{
			return Components.Add<T>();
		}
		public GameObject() : this("GameObject") { }
        public GameObject(string name) : this(name, Guid.NewGuid()) { }
        public GameObject(string name, Guid GUID)
        {
            //Console.WriteLine("MADE OBJECT");
            Components = new GameObjectComponents(this);
            transform = Components.Add<TransformComponent>();
            Tags = new();
            this.name = name;
			Id = GUID;
            if (MyScene == null)
                MyScene = SceneManager.ActiveScene;
		}
        public void Update()
        {
            if (!IsActive) return;
            foreach(Component c in Components.All)
            {
                if (!c.EnabledInHierarchy) continue;
                if (isDestroyed) return;
                c.Update();
				if (isDestroyed) return;
			}
		}
		public void Destroy()
        {
            if (_isdestroyed || Components == null) return;
            foreach (var component in Components.All)
            {
                component.EnabledSelf = false;
                component.OnDestroy();
            }
            Components.All.Clear();
            if (MyScene != null)
            {
				MyScene.RemovingGameObjects.Add(this);
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
}
