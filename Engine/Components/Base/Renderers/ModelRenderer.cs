using PGK2.Engine.Components.Base;
using OpenTK.Mathematics;
using PGK2.Engine.Core;
using System.Text.Json.Serialization;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;

namespace PGK2.Engine.Components
{
	[Serializable]
	public class ModelRenderer : Renderer
	{
		private Core.Model? _model;

		[JsonIgnore]
		public Core.Model? Model
		{
			get => _model; 
			set
			{
				SetModel(value);
			}
		}
		[JsonInclude]
		public string? ModelPath
		{
			get => _model?.Path;
			internal set
			{
				_loadedModelPath = value;
			}
		}
		public Material?[] OverrideMaterials;
		[JsonIgnore]
		internal string? _loadedModelPath;
		public ModelRenderer()
		{
			_model = null;
		}
		public void SetModel(Core.Model? model)
		{
			//Console.WriteLine("Setting MODEL to " + (model != null ? model.ToString() : "NULL"));
			_model = model;
			int mats = 0;
			foreach(Mesh m in model.meshes)
			{
				mats += 1; // jesli kiedys bedzie wiele materialow na mesh, to tu bedzie dodawane tyle ile materialow czy cos
			}
			OverrideMaterials = new Material?[mats];
		}
		public void InstantiateAllMaterials()
		{
			foreach(Mesh m in Model.meshes)
			{
				InstantiateMaterial(m);
			}
		}
		public void InstantiateMaterial(Mesh mesh)
		{
			int index = Model.meshes.IndexOf(mesh);
			OverrideMaterials[index] = mesh.Material.Instantiate();
		}
		protected override void Render(CameraComponent camera, EngineInstance.RenderPass RenderPass)
		{
			if (Model == null) return;
			base.Render(camera, RenderPass);
			Matrix4 modelMatrix = gameObject.transform.GetModelMatrix();
			Matrix4 viewMatrix = camera.ViewMatrix;
			Matrix4 projectionMatrix = camera.ProjectionMatrix;
			if (DrawOutline)
			{
				GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
				GL.StencilMask(0xFF);
			}
			else
			{
				GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
				GL.StencilMask(0x00);
			}
			Model.Draw(modelMatrix, viewMatrix, projectionMatrix, gameObject.MyScene.Lights, camera, RenderPass, null, OverrideMaterials);			
		}
		public virtual BoundingBox GetBoundingBox()
		{
			if (Model == null || Model.meshes == null || Model.meshes.Count == 0)
			{
				float size = 0.1f;
				Vector3 center = gameObject.transform.Position;
				Vector3 halfExtents = new Vector3(size / 2, size / 2, size / 2);

				Vector3 minx = center - halfExtents;
				Vector3 maxx = center + halfExtents;

				return new BoundingBox(minx, maxx);
			}
			Vector3 min = Vector3.TransformPosition(Model.ModelBoundingBox.Min, gameObject.transform.GetModelMatrix());
			Vector3 max = Vector3.TransformPosition(Model.ModelBoundingBox.Max, gameObject.transform.GetModelMatrix());

			return new BoundingBox(min, max);
		}
		protected override void RenderOutline(CameraComponent camera)
		{
			if (Model == null) return;
			Matrix4 viewMatrix = camera.ViewMatrix;
			Matrix4 projectionMatrix = camera.ProjectionMatrix;
			base.RenderOutline(camera);
			if (DrawOutline)
			{
				GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
				GL.StencilMask(0x00);

				// Disable depth testing
				GL.Disable(EnableCap.DepthTest);

				Vector3 Scale = transform.Scale;
				transform.LocalScale = Scale * 1.05f;
				OutlineMaterial.Use();
				Model.Draw(transform.GetModelMatrix(), viewMatrix, projectionMatrix, new List<Light>(), camera, EngineInstance.RenderPass.Outline, OutlineMaterial, null);
				transform.LocalScale = Scale;
				OutlineMaterial.Unuse();

				GL.StencilMask(0xFF);
				GL.StencilFunc(StencilFunction.Always, 1, 0xFF);

				// Re-enable depth testing
				GL.Enable(EnableCap.DepthTest);
			}
		}
	}
}
