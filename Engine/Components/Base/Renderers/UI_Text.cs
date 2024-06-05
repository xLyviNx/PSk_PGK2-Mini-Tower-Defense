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
		}

		internal override void Draw()
		{
			// Calculate the text size
			float padding = 10.0f;
			Vector2 textSize = (ImGui.CalcTextSize(Text) * FontSize)/* + Vector2.One*padding*/;
			Size = textSize;
			// Add some padding to the text size
			Vector2 windowSize = new Vector2(textSize.X + padding * 2, textSize.Y + padding * 2);

			// Set the position and size of the window
			ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);
			ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

			// Set the window properties
			ImGui.PushStyleColor(ImGuiCol.Text, Color);
			ImGui.SetNextWindowBgAlpha(0.0f);
			ImGui.Begin($"Text_{gameObject.Id}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground);

			// Set the font scale and render the text
			ImGui.SetWindowFontScale(FontSize);

			// Center the text within the window
			float cursorX = (windowSize.X - textSize.X) * 0.5f;
			float cursorY = (windowSize.Y - textSize.Y) * 0.5f;
			ImGui.SetCursorPos(new Vector2(cursorX, cursorY));

			ImGui.TextWrapped(Text);

			// Handle click and hover events
			if (ImGui.IsItemClicked())
			{
				Console.WriteLine("CLICK");
				Clicked();
			}
			HandleHoverState();

			ImGui.End();
			ImGui.PopStyleColor();
		}
	}
}