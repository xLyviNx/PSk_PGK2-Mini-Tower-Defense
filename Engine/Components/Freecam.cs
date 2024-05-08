using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PGK2.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Engine.Components
{
	public class Freecam : PGK2.Engine.Core.Component
	{
		private Vector3 oldrot;
		public override void Update()
		{
			base.Update();
			KeyboardState input = EngineInstance.Instance.window.KeyboardState;
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

			if (input.IsKeyDown(Keys.Left))
			{
				transform.Rotation += new Vector3(0, 1, 0) * (float)Time.deltaTime * 50f;
			}
			if (input.IsKeyDown(Keys.Right))
			{
				transform.Rotation += new Vector3(0, -1, 0) * (float)Time.deltaTime * 50f;
			}
			if (input.IsKeyDown(Keys.Down))
			{
				transform.Rotation += new Vector3(1, 0, 0) * (float)Time.deltaTime * 50f;
			}
			if (input.IsKeyDown(Keys.Up))
			{
				transform.Rotation += new Vector3(-1, 0, 0) * (float)Time.deltaTime * 50f;
			}
			if (oldrot != transform.Position)
				Console.WriteLine($"POS: {gameObject.transform.Position}");
			oldrot = transform.Position;
		}
	}
}
