using ImGuiNET;
using System;
using System.Numerics;

namespace PGK2.Engine.Components.Base.Renderers
{
	/// <summary>
	/// Klasa reprezentująca tekstowy element interfejsu użytkownika.
	/// </summary>
	public class UI_Text : UI_Renderer
	{
		/// <summary>
		/// Tekst do wyświetlenia.
		/// </summary>
		public string Text = "New Text";

		/// <summary>
		/// Rozmiar tekstu.
		/// </summary>
		public float FontSize = 1;
		/// <summary>
		/// Metoda rysująca tekst.
		/// </summary>
		internal override void Draw()
		{
			Vector2 textSize = (ImGui.CalcTextSize(Text) * FontSize)/* + Vector2.One*padding*/;
			Vector2 windowSize = textSize + Padding * 2;
			Size = windowSize;

			ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);
			ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

			ImGui.PushStyleColor(ImGuiCol.Text, Color);
			ImGui.SetNextWindowBgAlpha(0.0f);
			ImGui.Begin($"Text_{gameObject.Id}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground);

			ImGui.SetWindowFontScale(FontSize);

			float cursorX = (windowSize.X - textSize.X) * 0.5f;
			float cursorY = (windowSize.Y - textSize.Y) * 0.5f;
			ImGui.SetCursorPos(new Vector2(cursorX, cursorY));

			ImGui.TextWrapped(Text);

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