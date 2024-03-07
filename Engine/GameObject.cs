using Game.Engine.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine
{
	public class GameObject
	{
		public TagsContainer RenderTags { get; private set; }
		public string name;
		private bool _isdestroyed = false;
		public bool isDestroyed { get => _isdestroyed; }
		public GameObjectComponents components;
		public TransformComponent transform;
		public TagsContainer Tags { get; private set; }
		public bool isActiveSelf { get; private set; } = true;
		public bool isActive
		{
			get
			{
				return isActiveSelf && transform != null && (transform.Parent == null || ( transform.Parent.gameObject != null && transform.Parent.gameObject.isActive));
			}
			set
			{
				isActiveSelf = value;
			}
		}
		public GameObject() : this("GameObject") { }
		public GameObject(string name)
		{
			components = new GameObjectComponents(this);
			transform = new TransformComponent();
			Tags = new();
			RenderTags = new();
			this.name = name;
		}
		public void Destroy()
		{
			if (_isdestroyed||components==null) return;
			foreach (var component in components.All)
			{
				component.gameObject = null;
			}
			_isdestroyed = true;
		}
	}
	public class GameObjectComponents
	{
		public GameObject gameObject;
		public List<Component> All { get; private set; }

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
	public class TagsContainer
	{
		private List<string> All;
		public TagsContainer()
		{
			All = new();
		}
		public bool Has(string name)
		{ return All.Contains(name); }

		public bool Add(string tag)
		{
			if (Has(tag)) return false;
			All.Add(tag);
			return true;
		}
	}
}
