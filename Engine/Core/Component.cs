using PGK2.Engine.SceneSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Engine.Core
{
    [Serializable]
	public abstract class Component
    {
		public GameObject gameObject;
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
