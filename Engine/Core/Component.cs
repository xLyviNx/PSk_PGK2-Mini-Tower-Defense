using Game.Engine.Components;
using PGK2.Engine.Components;
using PGK2.Engine.SceneSystem;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Core
{
    [Serializable]
	[JsonDerivedType(typeof(CameraComponent))]

	public abstract class Component
    {
		[JsonIgnore] public GameObject gameObject;
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

        }
    }
}
