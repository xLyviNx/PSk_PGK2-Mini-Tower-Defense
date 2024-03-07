using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Components
{
	public abstract class Component
	{
		public GameObject gameObject;
		private bool _enabledSelf = true;
		public bool enabled
		{
			get
			{
				return _enabledSelf && gameObject != null && gameObject.isActive;
			}
			set { _enabledSelf = value; }
		}
		public Component()
		{
			
		}
	}
}
