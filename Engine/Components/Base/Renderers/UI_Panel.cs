using ImGuiNET;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using System.Numerics;

public class UI_Panel : UI_Renderer
{
	public override void Awake()
	{
		base.Awake();
		HoverOnlyWindow = true;
	}
	internal override void Draw()
	{
		ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);
		ImGui.SetNextWindowSize(Size, ImGuiCond.Always);

		ImGui.PushStyleColor(ImGuiCol.WindowBg, Color);

		ImGui.Begin($"panel_{gameObject.Id}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse);

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