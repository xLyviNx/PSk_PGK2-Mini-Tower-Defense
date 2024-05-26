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
		private float CameraDistance = 12f;
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
			myCamera.FieldOfView = 30f;
			if (EngineWindow.instance.IsFocused)
			{
				var mouse = EngineWindow.instance.MouseState;
				MouseLockController.HoldingCamera = mouse.IsButtonDown(MouseButton.Button2);
				KeyboardState input = EngineWindow.instance.KeyboardState;
				if (MouseLockController.HoldingCamera && Mouse.IsLocked) 
				{
					if (Mouse.Delta.X != 0f)
						transform.Parent.Yaw -= Mouse.Delta.X * Time.deltaTime * MouseSens * 10f;
					if (Mouse.Delta.Y != 0f)
						transform.Parent.Pitch += Mouse.Delta.Y * Time.deltaTime * MouseSens * 10f;

					transform.Parent.Pitch = Math.Clamp(transform.Parent.Pitch, -44.9f, 134.9f);
				}
				CameraDistance -= mouse.ScrollDelta.Y * Time.deltaTime * 15f;
				CameraDistance = Math.Clamp(CameraDistance, 3f, 25f);
			}
			transform.LocalPosition = new(0, CameraDistance, CameraDistance);
			var look = TransformComponent.LookAtRotation(transform.Position, transform.Parent.Position);
			transform.Rotation = look;
		}
	}
}
