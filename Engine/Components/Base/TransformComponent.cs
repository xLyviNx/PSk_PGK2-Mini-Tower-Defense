using OpenTK;
using OpenTK.Mathematics;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Components.Base
{
	/// <summary>
	/// Komponent transformacji, który definiuje położenie, skalę i rotację obiektu w przestrzeni.
	/// </summary>
	[Serializable]
    public class TransformComponent : Component
    {
		/// <summary>
		/// Lokalne położenie.
		/// </summary>
		public Vector3 LocalPosition { get; set; } = Vector3.Zero;
		/// <summary>
		/// Lokalna skala.
		/// </summary>
		public Vector3 LocalScale { get; set; } = Vector3.One;

        // Separate Euler angles
        private float _pitch { get; set; } = 0f; // X
		private float _yaw { get; set; } = 0f;   // Y
		private float _roll { get; set; } = 0f;  // Z
		/// <summary>
		/// Kąt pitch (X) w stopniach.
		/// </summary>
		public float Pitch { get => _pitch; set { _pitch = value % 360; } }

		/// <summary>
		/// Kąt yaw (Y) w stopniach.
		/// </summary>
		public float Yaw { get => _yaw; set { _yaw = value % 360; } }

		/// <summary>
		/// Kąt roll (Z) w stopniach.
		/// </summary>
		public float Roll { get => _roll; set { _roll = value % 360; } }

		private TransformComponent? parent = null;

		/// <summary>
		/// Klasa przechowująca referencje do potomnych obiektów (children).
		/// </summary>
		public ChildrenContainer Children { get; internal set; }
		/// <summary>
		/// Pozycja w przestrzeni globalnej.
		/// </summary>
		[JsonIgnore]
		public Vector3 Position
        {
            get { return parent != null ? parent.TransformPoint(LocalPosition) : LocalPosition; }
            set { LocalPosition = parent != null ? parent.InverseTransformPoint(value) : value; }
        }
		/// <summary>
		/// Skala w przestrzeni globalnej.
		/// </summary>
		[JsonIgnore]
		public Vector3 Scale
		{
			get
			{
				if (parent != null)
				{
					// Oblicz skalę w przestrzeni globalnej
					Vector3 parentWorldScale = parent.Scale;
					return Vector3.Multiply(LocalScale, parentWorldScale);
				}
				else
				{
					return LocalScale;
				}
			}
			set
			{
				if (parent != null)
				{
					// Oblicz skalę lokalną dziecka
					Vector3 parentWorldScale = parent.Scale;
					LocalScale = Vector3.Divide(value, parentWorldScale);
				}
				else
				{
					LocalScale = value;
				}
			}
		}
		/// <summary>
		/// Kierunek przodu.
		/// </summary>
		[JsonIgnore]
        public Vector3 Forward
        {
            get { return Vector3.Transform(Vector3.UnitZ, GetRotationMatrix().ExtractRotation()); }
        }
		/// <summary>
		/// Kierunek góry.
		/// </summary>
		[JsonIgnore]
        public Vector3 Up
        {
            get { return Vector3.Transform(Vector3.UnitY, GetRotationMatrix().ExtractRotation()); }
        }
		/// <summary>
		/// Kierunek prawo.
		/// </summary>
		[JsonIgnore]
        public Vector3 Right
        {
            get { return Vector3.Transform(Vector3.UnitX, GetRotationMatrix().ExtractRotation()); }
        }
		/// <summary>
		/// Rodzic (Parent).
		/// </summary>
		[JsonIgnore]
        public TransformComponent? Parent
        {
            get { return parent; }
            set
            {
                if (parent != null && parent.Children.AllObjects.Contains(parent))
                {
                    throw new Exception("Tried making child a parent object.");
                }

                if (parent != null)
                {
                    parent.Children.Remove(this);
                }
                parent = value;
                if (parent != null)
                {
                    parent.Children.Add(this);
                }
            }
        }
		/// <summary>
		/// Konstruktor transformacji.
		/// </summary>
		public TransformComponent()
        {
            //gameObject = attachedTo;
            LocalPosition = Vector3.Zero;
            Pitch = Yaw = Roll = 0f;
            LocalScale = Vector3.One;
            Children = new ChildrenContainer();
        }
		/// <summary>
		/// Pobiera macierz transformacji.
		/// </summary>
		public Matrix4 GetModelMatrix()
        {
            Matrix4 translationMatrix = Matrix4.CreateTranslation(Position);
            Matrix4 rotationMatrix = GetRotationMatrix();
            Matrix4 scaleMatrix = Matrix4.CreateScale(Scale);

            return scaleMatrix * rotationMatrix * translationMatrix;
        }
		/// <summary>
		/// Pobiera macierz lokalnej rotacji.
		/// </summary>
		public Matrix4 GetLocalRotationMatrix()
        {
            Matrix4 rollMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Roll));
            Matrix4 pitchMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Pitch));
            Matrix4 yawMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Yaw));

            return rollMatrix * pitchMatrix * yawMatrix;
        }
		/// <summary>
		/// Pobiera macierz globalnej rotacji.
		/// </summary>
		public Matrix4 GetWorldRotationMatrix()
		{
			Matrix4 rollMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(WorldRoll));
			Matrix4 pitchMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(WorldPitch));
			Matrix4 yawMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(WorldYaw));

			return rollMatrix * pitchMatrix * yawMatrix;
		}
		/// <summary>
		/// Pobiera macierz rotacji.
		/// </summary>
		public Matrix4 GetRotationMatrix()
        {
            if (Parent == null)
            {
                return GetLocalRotationMatrix();
            }
            else
            {
                return GetWorldRotationMatrix();
            }
        }
		/// <summary>
		/// Transformuje punkt.
		/// </summary>
		public Vector3 TransformPoint(Vector3 point)
        {
            return Vector3.Transform(point, GetRotationMatrix().ExtractRotation()) * Scale + Position;
        }
		/// <summary>
		/// Transformuje punkt odwrotnie.
		/// </summary>
		public Vector3 InverseTransformPoint(Vector3 point)
        {
            Vector3 invertedScale = new Vector3(1.0f / Scale.X, 1.0f / Scale.Y, 1.0f / Scale.Z);

            return Vector3.Transform(point - Position, Matrix4.Invert(GetRotationMatrix()).ExtractRotation()) * invertedScale;
        }
		/// <summary>
		/// Transformuje wektor.
		/// </summary>
		public Vector3 TransformVector(Vector3 localVector)
        {
            return Vector3.Transform(localVector, GetRotationMatrix().ExtractRotation()) * Scale;
        }
		/// <summary>
		/// Transformuje wektor odwrotnie.
		/// </summary>
		public Vector3 InverseTransformVector(Vector3 worldVector)
        {
            Vector3 invertedScale = new Vector3(1.0f / Scale.X, 1.0f / Scale.Y, 1.0f / Scale.Z);

            return Vector3.Transform(worldVector / Scale, Matrix4.Invert(GetRotationMatrix()).ExtractRotation());
        }
		/// <summary>
		/// Lokalna rotacja.
		/// </summary>
		public Vector3 LocalRotation
		{
			get
			{
				return new Vector3(Pitch, Yaw, Roll);
			}
			set
			{
				Pitch = value.X;
				Yaw = value.Y;
				Roll = value.Z;
			}
		}
		/// <summary>
		/// Globalna rotacja.
		/// </summary>
		[JsonIgnore]
		public Vector3 Rotation
		{
			get
			{
				if (parent == null)
					return LocalRotation;
				return new Vector3(WorldPitch, WorldYaw, WorldRoll);
			}
			set
			{
				if (parent == null)
					LocalRotation = value;
				else
				{
					Vector3 parentRotation = new Vector3(parent.WorldPitch, parent.WorldYaw, parent.WorldRoll);
					LocalRotation = value - parentRotation;
				}
			}
		}
		/// <summary>
		/// Globalny pitch.
		/// </summary>
		[JsonIgnore]
		public float WorldPitch
		{
			get
			{
				if (parent != null)
					return parent.WorldPitch + Pitch;
				else
					return Pitch;
			}
		}
		/// <summary>
		/// Globalny yaw.
		/// </summary>
		[JsonIgnore]
		public float WorldYaw
		{
			get
			{
				if (parent != null)
					return parent.WorldYaw + Yaw;
				else
					return Yaw;
			}
		}
		/// <summary>
		/// Globalny roll.
		/// </summary>
		[JsonIgnore] public float WorldRoll
		{
			get
			{
				if (parent != null)
					return parent.WorldRoll + Roll;
				else
					return Roll;
			}
		}
		/// <summary>
		/// Oblicza rotację w kierunku określonego punktu.
		/// </summary>
		public static Vector3 LookAtRotation(Vector3 eye, Vector3 target)
		{
			Vector3 forward = Vector3.Normalize(target - eye);

			float yaw = (float)Math.Atan2(forward.X, forward.Z);
			float pitch = (float)Math.Asin(forward.Y);

			float yawDegrees = MathHelper.RadiansToDegrees(yaw);
			float pitchDegrees = MathHelper.RadiansToDegrees(pitch);

			return new Vector3(-pitchDegrees, yawDegrees, 0f);
		}
		/// <summary>
		/// Interpoluje kąty.
		/// </summary>
		public static float LerpAngle(float a, float b, float t)
		{
			float delta = b - a;

			if (Math.Abs(delta) > 180)
			{
				if (delta > 0)
					a += 360;
				else
					b += 360;
			}

			float angle = a + (b - a) * t;

			angle = (angle + 360) % 360;

			return angle;
		}

		/// <summary>
		/// Obraca obiekt w kierunku określonego wektora.
		/// </summary>
		public void RotateTowards(Vector3 targetDirection, float speed)
		{
			Vector3 currentDirection = transform.LocalRotation;
			Vector3 newRotation = LookAtRotation(transform.Position, targetDirection);

			Vector3 lerpedRotation = new Vector3(
				LerpAngle(currentDirection.X, newRotation.X, speed * Time.deltaTime),
				LerpAngle(currentDirection.Y, newRotation.Y, speed * Time.deltaTime),
				LerpAngle(currentDirection.Z, newRotation.Z, speed * Time.deltaTime)
			);

			LocalRotation = lerpedRotation;
		}
	}

}
