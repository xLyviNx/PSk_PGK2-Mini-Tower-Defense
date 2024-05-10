using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
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
	public class Freecam : PGK2.Engine.Core.Component
	{
		float MouseSens = 3f;
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
				float speedmod = 1f;
				if (input.IsKeyDown(Keys.LeftShift))
					speedmod = 3f;
				if (input.IsKeyDown(Keys.W))
				{
					transform.Position += transform.Forward * 2f * (float)Time.deltaTime * speedmod; //Forward 
				}

				if (input.IsKeyDown(Keys.S))
				{
					transform.Position -= transform.Forward * 2f * (float)Time.deltaTime * speedmod; //Backwards
				}

				if (input.IsKeyDown(Keys.A))
				{
					transform.Position += transform.Right * 2f * (float)Time.deltaTime * speedmod; //Left
				}

				if (input.IsKeyDown(Keys.D))
				{
					transform.Position -= transform.Right * 2f * (float)Time.deltaTime * speedmod; //Right
				}

				if (input.IsKeyDown(Keys.Q))
				{
					transform.Position += transform.Up * 5f * (float)Time.deltaTime * speedmod; //Up 
				}

				if (input.IsKeyDown(Keys.E))
				{
					transform.Position -= transform.Up * 5f * (float)Time.deltaTime; //Down
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
