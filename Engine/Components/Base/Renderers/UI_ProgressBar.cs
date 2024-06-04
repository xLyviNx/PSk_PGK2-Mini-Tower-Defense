using ImGuiNET;
using PGK2.Engine.Core;
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
		public float BarHeight = 15f;
		public bool ShowPercentage = true;

		public override void Update()
		{
			base.Update();
		}

		internal override void Draw()
		{
			Size = new(BarWidth, BarHeight);
			ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);

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
			ImGui.ProgressBar(fraction, new Vector2(BarWidth,BarHeight), overlay);

			ImGui.PopStyleColor(2);
			ImGui.End();

			ImGui.PopStyleVar(2);
			ImGui.PopStyleColor(2);
		}
	}
}