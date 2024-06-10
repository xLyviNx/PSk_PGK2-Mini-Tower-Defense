using OpenTK.Mathematics;
using OpenTK.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Klasa reprezentująca myszkę w aplikacji.
	/// </summary>
	public class Mouse
	{
		/// <summary>
		/// Pozycja myszy na ekranie.
		/// </summary>
		public static Vector2 MousePosition { get; set; }

		/// <summary>
		/// Określa, czy kursor jest zablokowany.
		/// </summary>
		public static bool _isLocked;

		/// <summary>
		/// Liczba klatek od ostatniego ruchu myszy.
		/// </summary>
		public static uint framesSinceLastMove = 0;

		/// <summary>
		/// Pobiera lub ustawia, czy kursor jest zablokowany.
		/// </summary>
		public static bool IsLocked
		{
			get
			{
				if (EngineWindow.instance != null)
					return _isLocked && EngineWindow.instance.IsFocused && EngineWindow.instance.IsVisible;

				return _isLocked;
			}
			set
			{
				bool wasDiff = false;
				if (value != IsLocked)
				{
					wasDiff = true;
				}
				_isLocked = value;
				/*if(wasDiff)
				{
					CenterMouse();
					IgnoreDelta = true;
					unsafe
					{
						GLFW.SetInputMode(EngineWindow.instance.WindowPtr, CursorStateAttribute.Cursor, IsLocked ? CursorModeValue.CursorDisabled : CursorModeValue.CursorNormal);
					}
				}*/
			}
		}

		/// <summary>
		/// Określa, czy należy zignorować zmianę pozycji kursora.
		/// </summary>
		public static bool IgnoreDelta = false;

		/// <summary>
		/// Zmiana pozycji kursora między kolejnymi klatkami.
		/// </summary>
		public static Vector2 Delta { get; set; }

		private static Vector2i _lastscreensize;
		private static Vector2i _lastscreencenter;

		/// <summary>
		/// Środek ekranu aplikacji.
		/// </summary>
		public static Vector2i ScreenCenter
		{
			get
			{
				if (EngineWindow.instance == null) return Vector2i.Zero;
				if (EngineWindow.instance.Size != _lastscreensize)
				{
					_lastscreensize = EngineWindow.instance.Size;
					_lastscreencenter = new Vector2i((int)(Math.Floor(EngineWindow.instance.Size.X / 2f)), (int)(Math.Floor(EngineWindow.instance.Size.Y / 2f)));
				}
				return _lastscreencenter;
			}
		}

		/// <summary>
		/// Przesuwa kursor myszy na środek ekranu.
		/// </summary>
		private static void CenterMouse()
		{
			MoveMouse(ScreenCenter);
		}

		/// <summary>
		/// Przesuwa kursor myszy do określonej pozycji.
		/// </summary>
		/// <param name="x">Współrzędna X.</param>
		/// <param name="y">Współrzędna Y.</param>
		public static void MoveMouse(int x, int y)
		{
			MoveMouse(new Vector2i(x, y));
		}

		/// <summary>
		/// Przesuwa kursor myszy do określonej pozycji.
		/// </summary>
		/// <param name="pos">Pozycja do której ma być przesunięty kursor.</param>
		public static void MoveMouse(Vector2i pos)
		{
			unsafe
			{
				GLFW.SetCursorPos(EngineWindow.instance.WindowPtr, pos.X, pos.Y);
			}
		}
	}
}
