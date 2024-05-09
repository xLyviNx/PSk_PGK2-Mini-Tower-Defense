using OpenTK.Mathematics;
using OpenTK.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace PGK2.Engine.Core
{
	public class Mouse
	{
		public static Vector2 MousePosition { get; set; }
		public static bool _isLocked;
		public static uint framesSinceLastMove = 0;
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
				bool wasDiff = false; ;
				if (value != IsLocked)
				{
					wasDiff = true;
				}
				_isLocked = value;
				if(wasDiff)
				{
					CenterMouse();
					IgnoreDelta = true;
					unsafe
					{
						GLFW.SetInputMode(EngineWindow.instance.WindowPtr, CursorStateAttribute.Cursor, IsLocked? CursorModeValue.CursorDisabled : CursorModeValue.CursorNormal);
					}
				}
				
			}
		}
		public static bool IgnoreDelta = false;
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
