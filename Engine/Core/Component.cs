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
        [JsonIgnore] public static GameObject? assigningComponentTo;
		[JsonIgnore] public GameObject gameObject;
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
        public Component()
        {
            if (assigningComponentTo != null)
                gameObject = assigningComponentTo;
            else
                gameObject = null;
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
