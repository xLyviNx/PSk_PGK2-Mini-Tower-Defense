using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using System.Numerics;

namespace PGK2.Engine.Components.Base
{
	public class UI_Renderer : Component
	{
		public Vector2 UI_Position => new Vector2(transform.Position.X, transform.Position.Y);
		public Vector4 Color = new Vector4(1, 1, 1, 1);
		public UI_Renderer()
		{
			OnSceneTransfer += SceneTransfer;
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
			if (Enabled)
				Draw();
		}
	}
}
