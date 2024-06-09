using PGK2.Engine.Components.Base;
using OpenTK;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using OpenTK.Mathematics;

namespace PGK2.Engine.Components
{
	/// <summary>
	/// Abstrakcyjna klasa bazowa dla komponentów renderujących (nie UI).
	/// </summary>
	[Serializable]
	public abstract class Renderer : Component
	{
		/// <summary>
		/// Kolor obrysu.
		/// </summary>
		public Color4 OutlineColor = Color4.Transparent;
		protected Material _outlineMaterial;
		/// <summary>
		/// Określa, czy należy rysować obrys.
		/// </summary>
		protected bool DrawOutline => OutlineColor != Color4.Transparent;
		/// <summary>
		/// Materiał używany do rysowania obrysu.
		/// </summary>
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
		/// <summary>
		/// Sprawdza, czy komponent może być renderowany dla danej kamery.
		/// </summary>
		internal bool CanDraw(CameraComponent camera)
		{
			if (camera == null || !camera.EnabledInHierarchy)
				return false;

			if (!EnabledInHierarchy) return false;
			bool pass = (camera.IncludeTags.isEmpty && !camera.ExcludeTags.HasAny(gameObject.Tags)) || camera.IncludeTags.HasAny(gameObject.Tags);
			return pass;
		}
		/// <summary>
		/// Konstruktor Renderer.
		/// </summary>
		protected Renderer()
		{
			OnSceneTransfer += SceneTransfer;
		}
		/// <summary>
		/// Metoda wywoływana podczas zniszczenia komponentu.
		/// </summary>
		public override void OnDestroy()
		{
			if (OnSceneTransfer != null)
				OnSceneTransfer -= SceneTransfer;
			base.OnDestroy();
		}
		/// <summary>
		/// Metoda przenosząca komponent Renderer do nowej sceny.
		/// </summary>
		private void SceneTransfer(Scene? oldscene)
		{
			if (oldscene != null)
				oldscene.Renderers.Remove(this);

			if (MyScene.Renderers.Contains(this)) return;
			MyScene.Renderers.Add(this);
		}
		/// <summary>
		/// Wywołuje renderowanie komponentu Renderer.
		/// </summary>
		public void CallRender(CameraComponent camera, EngineInstance.RenderPass RenderPass)
		{
			var pass = CanDraw(camera);
			if (pass)
			{
				Render(camera, RenderPass);
			}
		}

		/// <summary>
		/// Wywołuje renderowanie obrysu komponentu Renderer.
		/// </summary>
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

		/// <summary>
		/// Metoda do implementacji, która definiuje sposób renderowania komponentu Renderer.
		/// </summary>
		protected virtual void Render(CameraComponent camera, EngineInstance.RenderPass RenderPass)
		{

		}
		/// <summary>
		/// Metoda do implementacji, która definiuje sposób renderowania obrysu komponentu Renderer.
		/// </summary>
		protected virtual void RenderOutline(CameraComponent camera)
		{

		}
	}
}
