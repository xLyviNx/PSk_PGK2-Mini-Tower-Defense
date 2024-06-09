using PGK2.Engine.Components.Base;
using OpenTK;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using OpenTK.Mathematics;

namespace PGK2.Engine.Components
{
	[Serializable]
	public abstract class Renderer : Component
	{
		public Color4 OutlineColor = Color4.Transparent;
		protected Material _outlineMaterial;
		protected bool DrawOutline => OutlineColor != Color4.Transparent;
		protected Material OutlineMaterial
		{
			get
			{
				if (_outlineMaterial == null || _outlineMaterial.ColorValues["outlinecolor"] != OutlineColor)
				{
					_outlineMaterial = new Material(EngineWindow.OutlineShader);
					_outlineMaterial.ColorValues["outlinecolor"] = OutlineColor;
				}

				return _outlineMaterial;
			}
		}
		internal bool CanDraw(CameraComponent camera)
		{
			if (camera == null || !camera.EnabledInHierarchy)
				return false;

			if (!EnabledInHierarchy) return false;
			bool pass = (camera.IncludeTags.isEmpty && !camera.ExcludeTags.HasAny(gameObject.Tags)) || camera.IncludeTags.HasAny(gameObject.Tags);
			return pass;
		}
		protected Renderer()
		{
			OnSceneTransfer += SceneTransfer;
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
				oldscene.Renderers.Remove(this);

			if (MyScene.Renderers.Contains(this)) return;
			MyScene.Renderers.Add(this);
		}
		public void CallRender(CameraComponent camera, EngineInstance.RenderPass RenderPass)
		{
			var pass = CanDraw(camera);
			if (pass)
			{
				Render(camera, RenderPass);
			}
		}
		public void CallRenderOutline(CameraComponent camera)
		{
			if (camera == null || !camera.EnabledInHierarchy)
				return;

			if (!EnabledInHierarchy) return;
			bool pass = (camera.IncludeTags.isEmpty && !camera.ExcludeTags.HasAny(gameObject.Tags)) || camera.IncludeTags.HasAny(gameObject.Tags);
			if (pass)
			{
				RenderOutline(camera);
			}
		}
		protected virtual void Render(CameraComponent camera, EngineInstance.RenderPass RenderPass)
		{

		}	
		protected virtual void RenderOutline(CameraComponent camera)
		{

		}
	}
}
