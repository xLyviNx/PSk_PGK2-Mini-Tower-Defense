using ImGuiNET;
using System;
using System.Numerics;

namespace PGK2.Engine.Components.Base.Renderers
{
	public class UI_Text : UI_Renderer
	{
		public string Text = "New Text";
		public float FontSize = 1;

		public override void Update()
		{
			base.Update();
			if (wasHovered)
				Color = new(1, 0, 0, 1);
			else
				Color = new(0, 1, 1, 1);
		}
		internal override void Draw()
		{
			ImGui.SetNextWindowPos(UI_Position, ImGuiCond.Always);
			ImGui.PushStyleColor(ImGuiCol.Text, Color);
			ImGui.SetNextWindowBgAlpha(0.0f);
			ImGui.Begin("Text", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground);

			ImGui.SetWindowFontScale(FontSize);

			Vector2 textSize = ImGui.CalcTextSize(Text);

			ImGui.InvisibleButton("##invisible_button", textSize);

			if (ImGui.IsItemClicked())
			{
				Console.WriteLine("CLICK");
				Clicked();
			}

			HandleHoverState();

			ImGui.SetCursorPos(ImGui.GetCursorPos() - new Vector2(0, textSize.Y)); 
			ImGui.Text(Text);

			ImGui.End();
			ImGui.PopStyleColor();
		}
	}
}
