using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using System.Diagnostics;

namespace PGK2.TowerDef.Scripts
{
	public class CameraController : Component
	{
		float MouseSens = 3f;
		private CameraComponent? myCamera;
		public override void Awake()
		{
			base.Awake();
			myCamera = GetComponent<CameraComponent>();
			
		}
		public override void Update()
		{
			Debug.Assert(myCamera != null);
			Debug.Assert(transform.Parent != null);
			Debug.Assert(EngineWindow.instance != null);
			transform.LocalPosition = Vector3.One * 5f;
			transform.LocalRotation = new Vector3(180f, 0, 0f);
			if (EngineWindow.instance.IsFocused)
			{
				var mouse = EngineWindow.instance.MouseState;
				MouseLockController.HoldingCamera = mouse.IsButtonDown(MouseButton.Button2);
				KeyboardState input = EngineWindow.instance.KeyboardState;
				if (MouseLockController.HoldingCamera && Mouse.IsLocked) 
				{
					if (Mouse.Delta.X != 0f)
						transform.Parent.LocalRotation -= new Vector3(0f, Mouse.Delta.X, 0f) * Time.deltaTime * MouseSens * 10f;
					if (Mouse.Delta.Y != 0f)
						transform.Parent.LocalRotation += new Vector3(Mouse.Delta.Y, 0f, 0f) * Time.deltaTime * MouseSens * 10f;

					//float x = Math.Clamp(transform.Rotation.X, -90f, 90f);

					//transform.Rotation = new Vector3(x, transform.Rotation.Y, transform.Rotation.Z);
				}
			}
		}
	}
}
