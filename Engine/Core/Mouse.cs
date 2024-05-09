using OpenTK.Mathematics;
using OpenTK.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PGK2.Engine.Core
{
	public class Mouse
	{
		public static Vector2i MousePosition { get; set; }
		public static bool _isLocked;
		public static bool IsLocked
		{
			get 
			{
				if (EngineWindow.instance!=null)
					return _isLocked && EngineWindow.instance.IsFocused && EngineWindow.instance.IsVisible;

				return _isLocked;
			}
			set
			{
				if (value != IsLocked)
				{
					CenterMouse();
				}
				_isLocked = value;
			}
		}
		public static Vector2i LockDelta { get; set; }
		public static Vector2 Delta { get; set; }
		private static Vector2i _lastscreensize;
		private static Vector2i _lastscreencenter;
		public static Vector2i ScreenCenter
		{
			get
			{
				if (EngineWindow.instance == null) return Vector2i.Zero;
				if (EngineWindow.instance.Size!=_lastscreensize)
				{
					_lastscreensize= EngineWindow.instance.Size;
					_lastscreencenter = new Vector2i((int)(Math.Floor(EngineWindow.instance.Size.X / 2f)), (int)(Math.Floor(EngineWindow.instance.Size.Y / 2f)));
				}
				return _lastscreencenter;
			}
		}
		public static void CenterIfLocked()
		{
			if (!IsLocked) return;
			if (EngineWindow.instance == null) return;
			if (!EngineWindow.instance.IsFocused) return;
			if (!EngineWindow.instance.IsVisible) return;
			//if (!EngineWindow.instance.IsPointInWindowBounds(MousePosition)) return;
			if (MousePosition == ScreenCenter) return;
			CenterMouse();
		}

		private static void CenterMouse()
		{
			MoveMouse(ScreenCenter);
		}
		public static void MoveMouse(int x, int y) { MoveMouse(new Vector2i(x, y)); }
		public static void MoveMouse(Vector2i pos)
		{
			unsafe
			{
				GLFW.SetCursorPos(EngineWindow.instance.WindowPtr, pos.X, pos.Y);
			}
		}
	}
}
