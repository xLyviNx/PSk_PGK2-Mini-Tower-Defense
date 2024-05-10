using Assimp;
using PGK2.Engine.Components.Base;
using OpenTK;
using OpenTK.Mathematics;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Components
{
	[Serializable]
	public abstract class Renderer : Component
	{
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
		public void CallRender(CameraComponent camera)
		{
			if (camera == null || !camera.Enabled)
				return;

			if (!Enabled) return;
			bool pass = camera.RenderTags.isEmpty || camera.RenderTags.HasAny(RenderTags);
			if (pass)
			{
				Render(camera);
			}
		}
		protected virtual void Render(CameraComponent camera)
		{

		}
	}
	[Serializable]
	public class ModelRenderer : Renderer
	{
		private Core.Model? _model;
		[JsonIgnore]
		public Core.Model? Model
		{
			get => _model; set
			{
				SetModel(value);
			}
		}
		public ModelRenderer()
		{
			_model = null;
		}
		public void SetModel(Core.Model? model)
		{
			Console.WriteLine("Setting MODEL to " + (model != null ? model.ToString() : "NULL"));
			_model = model;
		}

		protected override void Render(CameraComponent camera)
		{
			if (Model == null) return;
			base.Render(camera);
			Matrix4 modelMatrix = gameObject.transform.GetModelMatrix();
			Matrix4 viewMatrix = camera.ViewMatrix;
			Matrix4 projectionMatrix = camera.ProjectionMatrix;
			Model.Draw(modelMatrix, viewMatrix, projectionMatrix);
		}
	}
}
