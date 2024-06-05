using ImGuiNET;
using PGK2.Engine.Core;
using System;
using System.Numerics;

namespace PGK2.Engine.Components.Base.Renderers
{
	public class UI_Button : UI_Renderer
	{
		public string Text = "Button";
		public Vector4 NormalColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
		public Vector4 HoverColor = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
		public Vector4 ClickColor = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
		public float FontSize = 1.0f;
		public float FadeSpeed = 8.0f;

		private Vector4 currentColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
		private bool isHovered = false;
		private bool isClicked = false;

		public override void Update()
		{
			base.Update();

			// Fade the button color
			if (isHovered && !isClicked)
			{
				currentColor = Vector4.Lerp(currentColor, HoverColor, FadeSpeed * Time.deltaTime);
			}
			else if (isClicked)
			{
				currentColor = Vector4.Lerp(currentColor, ClickColor, FadeSpeed * Time.deltaTime);
			}
			else
			{
				currentColor = Vector4.Lerp(currentColor, NormalColor, FadeSpeed * Time.deltaTime);
			}
		}

		internal override void Draw()
		{
			// Calculate the text size
			float padding = 10.0f;
			Vector2 textSize = ImGui.CalcTextSize(Text) * FontSize;
			Size = textSize + new Vector2(padding * 2, padding * 2);

			// Set the position and size of the button
			ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);
			ImGui.SetNextWindowSize(Size, ImGuiCond.Always);

			// Set the button properties
			ImGui.PushStyleColor(ImGuiCol.Button, currentColor);
			ImGui.PushStyleColor(ImGuiCol.ButtonHovered, currentColor);
			ImGui.PushStyleColor(ImGuiCol.ButtonActive, currentColor);
			ImGui.SetNextWindowBgAlpha(1.0f);
			ImGui.SetWindowFontScale(FontSize);

			ImGui.Begin($"Button_{gameObject.Id}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse);

			// Render the button and handle click and hover events
			if (ImGui.Button(Text, Size))
			{
				Clicked();
			}

			isHovered = ImGui.IsItemHovered();
			isClicked = ImGui.IsItemActive();

			HandleHoverState();

			ImGui.End();

			ImGui.PopStyleColor(3);
		}
	}
}