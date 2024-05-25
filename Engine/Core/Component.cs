using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.SceneSystem;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Core
{
    [Serializable]
	[JsonDerivedType(typeof(CameraComponent))]
	[JsonDerivedType(typeof(SpinTest))]

	public abstract class Component
    {
        [JsonIgnore] internal bool CalledAwake = false;
        [JsonIgnore] public static GameObject? assigningComponentTo;
		[JsonIgnore] public GameObject gameObject;
        [JsonIgnore] public SceneSystem.Scene MyScene => gameObject.MyScene;
		[JsonIgnore] public GameObjectComponents Components => gameObject.Components;
		[JsonIgnore] public TransformComponent transform => gameObject.transform;
		private bool _enabledSelf = true;
        public bool Enabled
        {
            get
            {
                return _enabledSelf && gameObject != null && gameObject.IsActive;
            }
            set { _enabledSelf = value; }
        }
        [JsonIgnore] public Action<SceneSystem.Scene?> OnSceneTransfer = delegate { };

		public T? GetComponent<T>() where T : Component
		{
			return gameObject.Components.Get<T>();
		}

		public Component()
        {
            Console.WriteLine($"OBJ: '{assigningComponentTo}'");
            if (assigningComponentTo == null)
                Console.WriteLine("IS NULL");
			if (assigningComponentTo != null)
                gameObject = assigningComponentTo;
            else
                gameObject = null;
			gameObject.OnSceneTransfer += OnSceneTransfer;
			assigningComponentTo = null;
        }
        public virtual void Update()
        {

        }
        public virtual void Awake()
        {

        }
        public virtual void Start()
        {

        }
        public virtual void OnDestroy()
        {

        }
        public virtual void OnEnable()
        {

        }
        public virtual void OnDisable()
        {

        }
    }
}
