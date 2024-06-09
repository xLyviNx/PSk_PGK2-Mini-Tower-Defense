using ImGuiNET;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using System.Numerics;
namespace PGK2.Engine.Components.Base.Renderers
{
	/// <summary>
	/// Klasa reprezentująca przycisk w interfejsie użytkownika.
	/// </summary>
	public class UI_Button : UI_Renderer
	{
		/// <summary>
		/// Tekst wyświetlany na przycisku.
		/// </summary>
		public string Text = "Button";

		/// <summary>
		/// Rozmiar czcionki przycisku.
		/// </summary>
		public float FontSize = 1;

		/// <summary>
		/// Kolor przycisku w stanie normalnym.
		/// </summary>
		public Vector4 NormalColor = new Vector4(0.1f, 0.1f, 0.1f, 0.2f);

		/// <summary>
		/// Kolor przycisku podczas najechania kursorem.
		/// </summary>
		public Vector4 HoverColor = new Vector4(0.2f, 0.2f, 0.2f, 0.5f);

		/// <summary>
		/// Kolor przycisku podczas kliknięcia.
		/// </summary>
		public Vector4 ClickColor = new Vector4(0.8f, 0.8f, 0.8f, 0.9f);

		/// <summary>
		/// Szybkość płynnego przechodzenia kolorów.
		/// </summary>
		public float FadeSpeed = 15f;

		Vector4 currentColor = Vector4.Zero;
		float clickedTimer = 0f;

		/// <summary>
		/// Metoda wywoływana podczas aktualizacji.
		/// </summary>
		public override void Update()
		{
			base.Update();
			if (clickedTimer > 0f)
				clickedTimer -= Time.deltaTime;
		}

		/// <summary>
		/// Metoda wywoływana podczas uruchomienia komponentu.
		/// </summary>
		public override void Awake()
		{
			currentColor = NormalColor;
			base.Awake();
			HoverOnlyWindow = true;
		}
		/// <summary>
		/// Metoda obliczająca kolor przycisku w zależności od jego stanu.
		/// </summary>
		private Vector4 ButtonColor()
		{
			if (clickedTimer > 0f)
			{
				return Vector4.Lerp(currentColor, ClickColor, FadeSpeed * Time.deltaTime);
			}
			else if (wasHovered)
			{
				return Vector4.Lerp(currentColor, HoverColor, FadeSpeed * Time.deltaTime);
			}
			else
			{
				return Vector4.Lerp(currentColor, NormalColor, FadeSpeed * Time.deltaTime);
			}
		}
		/// <summary>
		/// Metoda rysująca tło przycisku.
		/// </summary>
		private void BackgroundDraw(Vector2 windowSize)
		{
			ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);
			ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

			ImGui.PushStyleColor(ImGuiCol.WindowBg, ButtonColor());

			ImGui.Begin($"btn_{gameObject.Id}_bg", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse);

			if (wasClicked)
			{
				Clicked();
				clickedTimer = 0.15f;
			}
			HandleHoverState();
			wasClicked = wasHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

			ImGui.End();
			ImGui.PopStyleColor();
		}
		/// <summary>
		/// Metoda rysująca tekst przycisku.
		/// </summary>
		private void TextDraw(Vector2 windowSize, Vector2 textSize)
		{
			ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);
			ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

			ImGui.PushStyleColor(ImGuiCol.Text, Color);
			ImGui.SetNextWindowBgAlpha(0.0f);
			ImGui.Begin($"btn_{gameObject.Id}_text", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground);

			ImGui.SetWindowFontScale(FontSize);

			float cursorX = (windowSize.X - textSize.X) * 0.5f;
			float cursorY = (windowSize.Y - textSize.Y) * 0.5f;
			ImGui.SetCursorPos(new Vector2(cursorX, cursorY));

			ImGui.TextWrapped(Text);

			ImGui.End();
			ImGui.PopStyleColor();
		}

		/// <summary>
		/// Metoda do przesłonięcia, która definiuje sposób rysowania komponentu.
		/// </summary>
		internal override void Draw()
		{
			Vector2 textSize = ImGui.CalcTextSize(Text) * FontSize;

			// Add some padding to the text size
			Vector2 windowSize = textSize + Padding * 2;
			Size = windowSize;
			TextDraw(windowSize, textSize);
			BackgroundDraw(windowSize);
		}
	}
}