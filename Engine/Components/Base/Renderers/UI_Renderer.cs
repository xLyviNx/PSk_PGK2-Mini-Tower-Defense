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
	/// <summary>
	/// Klasa bazowa odpowiedzialna za renderowanie interfejsu użytkownika (UI).
	/// </summary>
	public class UI_Renderer : Component
	{
		/// <summary>
		/// Określa, czy najeżdżanie wykrywane jest na całe okno obiektu (a nie tylko np. tekst).
		/// </summary>
		public bool HoverOnlyWindow = false;
		/// <summary>
		/// Punkt odniesienia do pozycji.
		/// </summary>
		public Vector2 Pivot;
		/// <summary>
		/// Rozmiar elementu UI. Zwykle ustalany wewnętrznie przy rysowaniu.
		/// </summary>
		public Vector2 Size;
		// <summary>
		/// Margines wokół elementu UI.
		/// </summary>
		public Vector2 Padding = Vector2.One*10;
		/// <summary>
		/// Wyrównanie elementu UI.
		/// </summary>
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
		/// <summary>
		/// Określa wyrównanie elementu UI.
		/// </summary>
		public Alignment UI_Alignment;
		/// <summary>
		/// Pozycja obiektu po wyrównaniu.
		/// </summary>
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
		/// <summary>
		/// Pozycja do rysowania.
		/// </summary>
		internal Vector2 DrawPosition
		{
			get
			{
				var pos = AlignmentPosition + new Vector2(transform.Position.X, transform.Position.Y);
				pos -= Pivot * Size;
				return pos;
			}
		}
		/// <summary>
		/// Kolor elementu UI.
		/// </summary>
		public Vector4 Color = new Vector4(1, 1, 1, 1);
		/// <summary>
		/// Zdarzenie wywoływane po kliknięciu na element UI.
		/// </summary>
		public event Action OnClick = delegate { };
		/// <summary>
		/// Zdarzenie wywoływane po najechaniu myszką na element UI.
		/// </summary>
		public event Action OnHover = delegate { };
		/// <summary>
		/// Zdarzenie wywoływane po opuszczeniu elementu UI myszką.
		/// </summary>
		public event Action OnExit = delegate { };
		/// <summary>
		/// Określa, czy element UI był kliknięty.
		/// </summary>
		public bool wasClicked = false;
		/// <summary>
		/// Określa, czy myszka najechała na element UI.
		/// </summary>
		public bool wasHovered = false;
		/// <summary>
		/// Aktualnie najechany element UI.
		/// </summary>
		public static UI_Renderer CurrentHovered;
		/// <summary>
		/// Określa indeks warstwy renderowania.
		/// </summary>
		public short Z_Index;
		/// <summary>
		/// Inicjalizuje nową instancję klasy UI_Renderer.
		/// </summary>
		public UI_Renderer()
		{
			OnSceneTransfer += SceneTransfer;
		}
		/// <summary>
		/// Obsługuje zdarzenie kliknięcia na element UI.
		/// </summary>
		public virtual void Clicked()
		{
			wasClicked = true;
			OnClick.Invoke();
		}
		/// <summary>
		/// Obsługuje zdarzenie najechania myszką na element UI.
		/// </summary>
		public virtual void Hovered()
		{
			OnHover.Invoke();
		}
		/// <summary>
		/// Obsługuje zdarzenie opuszczenia elementu UI myszką.
		/// </summary>
		public virtual void Exited()
		{
			OnExit.Invoke();
		}
		/// <summary>
		/// Wywoływane podczas niszczenia komponentu.
		/// </summary>
		public override void OnDestroy()
		{
			if (OnSceneTransfer != null)
				OnSceneTransfer -= SceneTransfer;
			if (CurrentHovered == this)
				CurrentHovered = null;
			base.OnDestroy();
		}
		/// <summary>
		/// Przenosi komponent UI do nowej sceny.
		/// </summary>
		/// <param name="oldscene">Stara scena, z której komponent UI jest przenoszony.</param>
		private void SceneTransfer(Scene? oldscene)
		{
			if (oldscene != null)
				oldscene.UI_Renderers.Remove(this);
			if (CurrentHovered == this)
				CurrentHovered = null;
			if (MyScene.UI_Renderers.Contains(this)) return;
			MyScene.UI_Renderers.Add(this);
		}
		/// <summary>
		/// Metoda wywoływana do rysowania komponentu UI.
		/// </summary>
		internal virtual void Draw()
		{
		}
		/// <summary>
		/// Wywołuje metodę rysowania komponentu UI, jeśli jest włączony w hierarchii.
		/// </summary>
		internal void CallDraw()
		{
			if (EnabledInHierarchy)
				Draw();
		}
		/// <summary>
		/// Obsługuje stan najechania myszką na element UI.
		/// </summary>
		protected virtual void HandleHoverState()
		{
			if (CurrentHovered == this)
				CurrentHovered = null;
			bool hovered = HoverOnlyWindow ? ImGui.IsWindowHovered() : ImGui.IsItemHovered();
			if (hovered)
			{
				CurrentHovered = this;
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
