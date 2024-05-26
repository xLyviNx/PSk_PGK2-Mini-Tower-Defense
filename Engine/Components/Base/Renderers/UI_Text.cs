using ImGuiNET;
using System.Numerics;

namespace PGK2.Engine.Components.Base.Renderers
{
	public class UI_Text : UI_Renderer
	{
		public string Text = "New Text";
		public float FontSize = 1;

		internal override void Draw()
		{
			ImGui.SetNextWindowPos(UI_Position, ImGuiCond.Always);
			ImGui.PushStyleColor(ImGuiCol.Text, Color);
			ImGui.SetNextWindowBgAlpha(0.0f); // Transparent background
			ImGui.Begin("Text", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground);
			ImGui.SetWindowFontScale(FontSize); 
			ImGui.Text(Text);
			ImGui.End();
			ImGui.PopStyleColor();
		}
	}
}
