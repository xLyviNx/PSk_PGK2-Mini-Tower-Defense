using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Engine.Components
{
	/// <summary>
	/// Komponent Freecam umożliwia swobodne poruszanie kamerą po scenie.
	/// </summary>
	public class Freecam : PGK2.Engine.Core.Component
	{
		/// <summary>
		/// Czułość ruchu myszy na obroty kamery.
		/// </summary>
		/// 
		float MouseSens = 3f;
		/// <summary>
		/// Wywoływane na początku działania komponentu.
		/// Ustawia początkową pozycję kamery.
		/// </summary>
		public override void Start()
		{
			base.Start();
			transform.Position=new Vector3(0,0,-1.5f);
		}
		/// <summary>
		/// Wywoływane w każdej klatce.
		/// Obsługuje ruch kamery w oparciu o sterowanie klawiaturą i myszą.
		public override void Update()
		{
			base.Update();
			if (EngineWindow.instance.IsFocused)
			{
				KeyboardState input = EngineInstance.Instance.window.KeyboardState;
				if (input.IsKeyPressed(Keys.D1))
				{
					Mouse.IsLocked = !Mouse.IsLocked;
				}				
				TransformComponent target = /*movinglight ? MyScene.Lights[0].transform :*/ transform;
				float speedmod = 1f;
				if (input.IsKeyDown(Keys.LeftShift))
					speedmod = 3f;
				if (input.IsKeyDown(Keys.W))
				{
					target.Position += transform.Forward * 2f * (float)Time.deltaTime * speedmod; //Forward 
				}

				if (input.IsKeyDown(Keys.S))
				{
					target.Position -= transform.Forward * 2f * (float)Time.deltaTime * speedmod; //Backwards
				}

				if (input.IsKeyDown(Keys.A))
				{
					target.Position += transform.Right * 2f * (float)Time.deltaTime * speedmod; //Left
				}

				if (input.IsKeyDown(Keys.D))
				{
					target.Position -= transform.Right * 2f * (float)Time.deltaTime * speedmod; //Right
				}

				if (input.IsKeyDown(Keys.Q))
				{
					target.Position += transform.Up * 5f * (float)Time.deltaTime * speedmod; //Up 
				}

				if (input.IsKeyDown(Keys.E))
				{
					target.Position -= transform.Up * 5f * (float)Time.deltaTime; //Down
				}
				if (Mouse.IsLocked)
				{
					if (Mouse.Delta.X != 0f)
						transform.Rotation -= new Vector3(0f, Mouse.Delta.X, 0f) * Time.deltaTime *  MouseSens * 10f;
					if (Mouse.Delta.Y != 0f)
						transform.Rotation += new Vector3(Mouse.Delta.Y, 0f, 0f) * Time.deltaTime * MouseSens * 10f;

					float x = Math.Clamp(transform.Rotation.X, -90f, 90f);

					transform.Rotation = new Vector3(x, transform.Rotation.Y, transform.Rotation.Z);
				}
				if (input.IsKeyPressed(Keys.F12))
				{
					SceneManager.UnloadScene(SceneManager.ActiveScene);
				}
			}
			else
			{
				Mouse.IsLocked = false;
			}
		}
	}
}
