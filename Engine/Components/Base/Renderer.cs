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
		public TagsContainer RenderTags { get; private set; }
		protected Renderer()
		{
			RenderTags = new();
			if (SceneManager.ActiveScene != null)
			{
				SceneManager.ActiveScene.Renderers.Add(this);
			}
		}
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (SceneManager.ActiveScene != null)
			{
				SceneManager.ActiveScene.Renderers.Remove(this);
			}
		}
		public void CallRender(CameraComponent camera, EngineInstance.RenderPass RenderPass)
		{
			if (camera == null || !camera.Enabled)
				return;

			if (!Enabled) return;
			bool pass = camera.RenderTags.isEmpty || camera.RenderTags.HasAny(RenderTags);
			if (pass)
			{
				Render(camera, RenderPass);
			}
		}
		public void CallRenderOutline(CameraComponent camera)
		{
			if (camera == null || !camera.Enabled)
				return;

			if (!Enabled) return;
			bool pass = camera.RenderTags.isEmpty || camera.RenderTags.HasAny(RenderTags);
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
