using ImGuiNET;
using PGK2.Engine.Core;
using System;
using System.Numerics;

namespace PGK2.Engine.Components.Base.Renderers
{
	/// <summary>
	/// Klasa reprezentująca pasek postępu w interfejsie użytkownika.
	/// </summary>
	public class UI_ProgressBar : UI_Renderer
	{

		/// <summary>
		/// Wartość paska postępu.
		/// </summary>
		public float Value = 0.0f;

		/// <summary>
		/// Maksymalna wartość paska postępu.
		/// </summary>
		public float MaxValue = 1.0f;

		/// <summary>
		/// Kolor paska postępu.
		/// </summary>
		public Vector4 BarColor = new Vector4(0, 1, 0, 1);

		/// <summary>
		/// Kolor tła paska postępu.
		/// </summary>
		public Vector4 BackgroundColor => Color;

		/// <summary>
		/// Szerokość paska postępu.
		/// </summary>
		public float BarWidth = 100.0f;

		/// <summary>
		/// Wysokość paska postępu.
		/// </summary>
		public float BarHeight = 15f;

		/// <summary>
		/// Określa, czy wyświetlać procenty na pasku postępu.
		/// </summary>
		public bool ShowPercentage = true;
		/// <summary>
		/// Metoda rysująca progress bar.
		/// </summary>
		internal override void Draw()
		{
			Size = new(BarWidth, BarHeight);
			ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);

			float fraction = Math.Clamp(Value / MaxValue, 0.0f, 1.0f);

			ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
			ImGui.PushStyleColor(ImGuiCol.ChildBg, Vector4.Zero);
			ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);

			ImGui.SetNextWindowBgAlpha(0.0f);
			ImGui.Begin("ProgressBar", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings);

			ImGui.PushStyleColor(ImGuiCol.PlotHistogram, BarColor);
			ImGui.PushStyleColor(ImGuiCol.FrameBg, BackgroundColor); 

			string overlay = ShowPercentage ? $"{(int)(fraction * 100)}%" : string.Empty;
			ImGui.ProgressBar(fraction, new Vector2(BarWidth,BarHeight), overlay);

			ImGui.PopStyleColor(2);
			ImGui.End();

			ImGui.PopStyleVar(2);
			ImGui.PopStyleColor(2);
		}
	}
}