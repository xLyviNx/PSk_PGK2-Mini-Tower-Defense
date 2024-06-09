using ImGuiNET;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using System.Numerics;
namespace PGK2.Engine.Components.Base.Renderers
{
	/// <summary>
	/// Klasa reprezentująca panel w interfejsie użytkownika.
	/// </summary>
	public class UI_Panel : UI_Renderer
	{
		/// <summary>
		/// Metoda wywoływana podczas uruchomienia komponentu.
		/// </summary>
		public override void Awake()
		{
			base.Awake();
			HoverOnlyWindow = true;
		}
		/// <summary>
		/// Metoda, która rysuje panel.
		/// </summary>
		internal override void Draw()
		{
			ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);
			ImGui.SetNextWindowSize(Size, ImGuiCond.Always);

			ImGui.PushStyleColor(ImGuiCol.WindowBg, Color);

			ImGui.Begin($"panel_{gameObject.Id}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse);

			// Handle click and hover events
			if (wasClicked)
			{
				Clicked();
			}
			HandleHoverState();
			wasClicked = wasHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

			ImGui.End();
			ImGui.PopStyleColor(); // Pop the background color	}
		}
	}
}