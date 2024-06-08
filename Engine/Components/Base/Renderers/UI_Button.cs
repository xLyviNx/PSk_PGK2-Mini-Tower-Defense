using ImGuiNET;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using System.Numerics;

public class UI_Button : UI_Renderer
{
	public string Text = "Button";
	public float FontSize = 1;
	public Vector4 NormalColor = new Vector4(0.1f, 0.1f, 0.1f, 0.2f);
	public Vector4 HoverColor = new Vector4(0.2f, 0.2f, 0.2f, 0.5f);
	public Vector4 ClickColor = new Vector4(0.8f, 0.8f, 0.8f, 0.9f);
	public float FadeSpeed = 15f;
	Vector4 currentColor = Vector4.Zero;
	float clickedTimer = 0f;
	public override void Update()
	{
		base.Update();
		if (clickedTimer > 0f)
			clickedTimer -= Time.deltaTime;
	}

	public override void Awake()
	{
		currentColor = NormalColor;
		base.Awake();
		HoverOnlyWindow = true;
	}
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
	private void BackgroundDraw(Vector2 windowSize)
	{
		ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);
		ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

		ImGui.PushStyleColor(ImGuiCol.WindowBg, ButtonColor());

		ImGui.Begin($"btn_{gameObject.Id}_bg", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse);

		// Handle click and hover events
		if (wasClicked)
		{
			Clicked();
			clickedTimer = 0.15f;
		}
		HandleHoverState();
		wasClicked = wasHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

		ImGui.End();
		ImGui.PopStyleColor(); // Pop the background color
	}
	private void TextDraw(Vector2 windowSize, Vector2 textSize)
	{
		ImGui.SetNextWindowPos(DrawPosition, ImGuiCond.Always);
		ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

		// Set the window properties
		ImGui.PushStyleColor(ImGuiCol.Text, Color);
		ImGui.SetNextWindowBgAlpha(0.0f);
		ImGui.Begin($"btn_{gameObject.Id}_text", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground);

		// Set the font scale and render the text
		ImGui.SetWindowFontScale(FontSize);

		// Center the text within the window
		float cursorX = (windowSize.X - textSize.X) * 0.5f;
		float cursorY = (windowSize.Y - textSize.Y) * 0.5f;
		ImGui.SetCursorPos(new Vector2(cursorX, cursorY));

		ImGui.TextWrapped(Text);

		ImGui.End();
		ImGui.PopStyleColor();
	}
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
