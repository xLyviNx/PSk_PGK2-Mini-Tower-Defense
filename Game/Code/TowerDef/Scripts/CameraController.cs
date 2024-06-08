using Assimp;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.Core;
using PGK2.Engine.Core.Physics;
using PGK2.Engine.SceneSystem;
using PGK2.Game.Code.TowerDef.Scripts;
using System.Diagnostics;

namespace PGK2.TowerDef.Scripts
{
	public class CameraController : Component
	{
		public static CameraController instance;
		float MouseSens = 3f;
		private CameraComponent? myCamera;
		private float TargetCameraDistance = 12f;
		private float CameraDistance;
		private float CameraDistanceLerpSpeed = 5f;
		private Vector2 CameraVelocity;
		Vector3 NormalBackground = new(0.3f, 0.6f, 0.5f);
		Vector3 DamageColor = new(0.7f, 0.1f, 0.0f);
		GameManager? manager;
		public override void Awake()
		{
			base.Awake();
			myCamera = GetComponent<CameraComponent>();
			manager = MyScene.FindObjectOfType<GameManager>();
			instance = this;
		}
		public override void Update()
		{
			Debug.Assert(myCamera != null);
			Debug.Assert(transform.Parent != null);
			Debug.Assert(EngineWindow.instance != null);
			myCamera.FieldOfView = 30f;
			Vector3 bgcolor = manager.TakenDamageTimer > 0f ? DamageColor : NormalBackground;
			Vector3 curr = new(myCamera.BackgroundColor.R, myCamera.BackgroundColor.G, myCamera.BackgroundColor.B);
			curr = Vector3.Lerp(curr, bgcolor, Time.deltaTime * 15f);
			myCamera.BackgroundColor = new(curr.X, curr.Y, curr.Z, 1f);
			if (EngineWindow.instance.IsFocused)
			{
				var mouse = EngineWindow.instance.MouseState;
				MouseLockController.HoldingCamera = mouse.IsButtonDown(MouseButton.Button2);
				KeyboardState input = EngineWindow.instance.KeyboardState;
				if (MouseLockController.HoldingCamera && Mouse.IsLocked) 
				{
					CameraVelocity = Vector2.Zero;
					if (Mouse.Delta.X != 0f)
						CameraVelocity.X -= Mouse.Delta.X * Time.deltaTime * MouseSens * 10f;
					if (Mouse.Delta.Y != 0f)
						CameraVelocity.Y -= Mouse.Delta.Y * Time.deltaTime * MouseSens * 10f;
				}
				TargetCameraDistance -= mouse.ScrollDelta.Y * 0.5f;
				TargetCameraDistance = Math.Clamp(TargetCameraDistance, 3f, 25f);
				if (mouse.IsButtonPressed(MouseButton.Left))
				{
					OnMouseClick(mouse.Position);
				}
			}
			transform.Parent.Yaw += CameraVelocity.X;
			transform.Parent.Pitch += CameraVelocity.Y;
			transform.Parent.Pitch = Math.Clamp(transform.Parent.Pitch, -44.9f, 134.9f);
			CameraVelocity = Vector2.Lerp(CameraVelocity, Vector2.Zero, Time.deltaTime * 2f);
			if (MathF.Abs(CameraDistance - TargetCameraDistance) > 0.005f)
				CameraDistance = MathHelper.Lerp(CameraDistance, TargetCameraDistance, Time.deltaTime * CameraDistanceLerpSpeed);
			else
				CameraDistance = TargetCameraDistance;
			transform.LocalPosition = new(0, CameraDistance, CameraDistance);
			var look = TransformComponent.LookAtRotation(transform.Position, transform.Parent.Position);
			transform.Rotation = look;
		}
		void OnMouseClick(Vector2 mousePosition)
		{
			
		}
	}
}
