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
			if(pass)
			{
				Render(camera);
			}
		}
		protected virtual void Render(CameraComponent camera)
		{
			
		}
	}
	[Serializable]
	public class MeshRenderer : Renderer
	{
		private Core.Mesh? _mesh;
		[JsonIgnore] public Core.Mesh? Mesh { get => _mesh; set
			{
				Console.WriteLine("1 Setting Mesh to " + (value != null ? value.ToString() : "NULL"));
				SetMesh(value);
			}
		}
		public List<Core.Material> Materials { get; set; }

		public MeshRenderer()
		{
			_mesh = null;
			Materials = new();
		}
		public void SetMesh(Core.Mesh? mesh)
		{
			Console.WriteLine("Setting Mesh to " + (mesh != null? mesh.ToString() : "NULL"));
			_mesh = mesh;
			Materials.Clear();
			if (mesh != null)
			{
				foreach (Core.Material mat in mesh.LoadedMaterials)
				{
					Console.WriteLine($"MATERIAL");
					mat.Shader = EngineWindow.shader;
					Materials.Add(mat);
				}
			}
		}

		protected override void Render(CameraComponent camera)
		{

			if (Mesh == null) return;
			if (Mesh.vertices.Count<=1) return;
			base.Render(camera);
			Matrix4 modelMatrix = gameObject.transform.GetModelMatrix();
			Matrix4 viewMatrix = camera.ViewMatrix;
			Matrix4 projectionMatrix = camera.ProjectionMatrix;


			for (int i = 0; i < Materials.Count; i++)
			{
				Core.Material material = Materials[i];

				material.Shader.SetMatrix4("model", modelMatrix);
				material.Shader.SetMatrix4("view", viewMatrix);
				material.Shader.SetMatrix4("projection", projectionMatrix);
				// Renderuj mesh z użyciem danego materiału
				material.Use();
				Mesh.Render(i);
				return;

				material.Unuse();
			}
		}
	}
}
