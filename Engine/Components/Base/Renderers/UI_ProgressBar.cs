using ImGuiNET;
using System;
using System.Numerics;

namespace PGK2.Engine.Components.Base.Renderers
{
	public class UI_ProgressBar : UI_Renderer
	{
		public float Value = 0.0f;
		public float MaxValue = 1.0f;
		public Vector4 BarColor = new Vector4(0, 1, 0, 1);
		public Vector4 BackgroundColor => Color;
		public float BarWidth = 100.0f;
		public bool ShowPercentage = true;

		public override void Update()
		{
			base.Update();
			// You can add additional update logic here
		}

		internal override void Draw()
		{
			ImGui.SetNextWindowPos(UI_Position, ImGuiCond.Always);

			float fraction = Math.Clamp(Value / MaxValue, 0.0f, 1.0f);

			// Push style variables to remove the border and background
			ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
			ImGui.PushStyleColor(ImGuiCol.ChildBg, Vector4.Zero);
			ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);

			ImGui.SetNextWindowBgAlpha(0.0f); // Make the window background transparent
			ImGui.Begin("ProgressBar", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings);

			ImGui.PushStyleColor(ImGuiCol.PlotHistogram, BarColor);
			ImGui.PushStyleColor(ImGuiCol.FrameBg, BackgroundColor); // Set the progress bar background color

			string overlay = ShowPercentage ? $"{(int)(fraction * 100)}%" : string.Empty;
			ImGui.ProgressBar(fraction, new Vector2(BarWidth, 0.0f), overlay);

			ImGui.PopStyleColor(2); // Restore BarColor and FrameBg colors
			ImGui.End();

			ImGui.PopStyleVar(2); // Restore style variables
			ImGui.PopStyleColor(2); // Restore ChildBg and WindowBg colors
		}
	}
}