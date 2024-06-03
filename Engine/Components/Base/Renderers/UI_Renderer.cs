using Assimp.Configs;
using ImGuiNET;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Components.Base
{
	public class UI_Renderer : Component
	{
		public enum Alignment
		{
			LeftUp,
			CenterUp,
			RightUp,
			Left,
			Center,
			Right,
			DownLeft,
			DownCenter,
			DownRight
		}
		public Alignment UI_Alignment;
		internal Vector2 AlignmentPosition
		{
			get
			{
				Debug.Assert(EngineWindow.instance != null);

				var size=EngineWindow.instance.ClientSize;
				switch (UI_Alignment)
				{
					case Alignment.LeftUp:
						return Vector2.Zero;
					case Alignment.CenterUp:
						return new Vector2(size.X / 2, 0);
					case Alignment.RightUp:
						return new Vector2(size.X, 0);
					case Alignment.Left:
						return new Vector2(0, size.Y / 2);
					case Alignment.Center:
						return new Vector2(size.X / 2, size.Y / 2);
					case Alignment.Right:
						return new Vector2(size.X, size.Y / 2);
					case Alignment.DownLeft:
						return new Vector2(0, size.Y);
					case Alignment.DownCenter:
						return new Vector2(size.X / 2, size.Y);
					case Alignment.DownRight:
						return new Vector2(size.X, size.Y);
					default:
						return Vector2.Zero;
				}
			}
		}
		internal Vector2 DrawPosition
		{
			get
			{
				return AlignmentPosition + new Vector2(transform.Position.X, transform.Position.Y);
			}
		}

		public Vector4 Color = new Vector4(1, 1, 1, 1);

		public event Action OnClick = delegate { };
		public event Action OnHover = delegate { };
		public event Action OnExit = delegate { };

		public bool wasHovered = false;

		public UI_Renderer()
		{
			OnSceneTransfer += SceneTransfer;
		}

		public virtual void Clicked()
		{
			OnClick.Invoke();
		}

		public virtual void Hovered()
		{
			OnHover.Invoke();
		}

		public virtual void Exited()
		{
			OnExit.Invoke();
		}

		public override void OnDestroy()
		{
			if (OnSceneTransfer != null)
				OnSceneTransfer -= SceneTransfer;
			base.OnDestroy();
		}

		private void SceneTransfer(Scene? oldscene)
		{
			if (oldscene != null)
				oldscene.UI_Renderers.Remove(this);

			if (MyScene.UI_Renderers.Contains(this)) return;
			MyScene.UI_Renderers.Add(this);
		}

		internal virtual void Draw()
		{
		}

		internal void CallDraw()
		{
			if (EnabledInHierarchy)
				Draw();
		}

		protected void HandleHoverState()
		{
			if (ImGui.IsItemHovered())
			{
				if (!wasHovered)
				{
					Hovered();
					wasHovered = true;
				}
			}
			else
			{
				if (wasHovered)
				{
					Exited();
					wasHovered = false;
				}
			}
		}
	}
}
