using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PGK2.Engine.Core;
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
		float MouseSens = 50f;
		bool MouseLockButtonUp = true;
		public override void Update()
		{
			base.Update();
			if (EngineWindow.instance.IsFocused)
			{
				KeyboardState input = EngineInstance.Instance.window.KeyboardState;
				if (input.IsKeyDown(Keys.D1))
				{
					if (MouseLockButtonUp)
						Mouse.IsLocked = !Mouse.IsLocked;
					MouseLockButtonUp = false;
				}
				else if (!MouseLockButtonUp)
					MouseLockButtonUp = true;
				if (input.IsKeyDown(Keys.W))
				{
					transform.Position += transform.Forward * 2f * (float)Time.deltaTime; //Forward 
				}

				if (input.IsKeyDown(Keys.S))
				{
					transform.Position -= transform.Forward * 2f * (float)Time.deltaTime; //Backwards
				}

				if (input.IsKeyDown(Keys.A))
				{
					transform.Position += transform.Right * 2f * (float)Time.deltaTime; //Left
				}

				if (input.IsKeyDown(Keys.D))
				{
					transform.Position -= transform.Right * 2f * (float)Time.deltaTime; //Right
				}

				if (input.IsKeyDown(Keys.Space))
				{
					transform.Position += transform.Up * 5f * (float)Time.deltaTime; //Up 
				}

				if (input.IsKeyDown(Keys.LeftShift))
				{
					transform.Position -= transform.Up * 5f * (float)Time.deltaTime; //Down
				}
				if (Mouse.IsLocked)
				{
					if (Mouse.Delta.X != 0f)
						transform.Rotation -= new Vector3(0f, Mouse.Delta.X, 0f) * Time.deltaTime *  MouseSens;
					if (Mouse.Delta.Y != 0f)
						transform.Rotation += new Vector3(Mouse.Delta.Y, 0f, 0f) * Time.deltaTime * MouseSens;

					float x = Math.Clamp(transform.Rotation.X, -90f, 90f);

					transform.Rotation = new Vector3(x, transform.Rotation.Y, transform.Rotation.Z);
				}
			}
			else
			{
				Mouse.IsLocked = false;
			}
		}
	}
}
