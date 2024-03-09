using Game.Engine.Components;
using OpenTK;
using OpenTK.Mathematics;
using PGK2.Engine.Core;

namespace PGK2.Engine.Components
{
	[Serializable]
    public abstract class Renderer : Component
	{
		public TagsContainer RenderTags { get; private set; }
		protected Renderer()
		{
			RenderTags = new();
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
		public Mesh Mesh { get; set; }
		public List<Material> Materials { get; set; }

		public MeshRenderer(Mesh mesh, Material[] materials)
		{
			Mesh = mesh;
			Materials = new();
		}

		protected override void Render(CameraComponent camera)
		{
			if (Mesh == null) return;
			if (Mesh.Vertices.Count<=1) return;
			base.Render(camera);
			Matrix4 modelMatrix = gameObject.transform.GetModelMatrix();
			Matrix4 viewMatrix = camera.ViewMatrix;
			Matrix4 projectionMatrix = camera.ProjectionMatrix;

			foreach (Material material in Materials)
			{
				// Ustaw macierze w materiale
				material.Shader.SetMatrix4("model", modelMatrix);
				material.Shader.SetMatrix4("view", viewMatrix);
				material.Shader.SetMatrix4("projection", projectionMatrix);

				// Renderuj mesh z użyciem danego materiału
				material.Use();
				Mesh.Render();
				material.Unuse();
			}
		}
	}
}
