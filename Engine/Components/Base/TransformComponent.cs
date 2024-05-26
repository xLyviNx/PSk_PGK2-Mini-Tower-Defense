using OpenTK;
using OpenTK.Mathematics;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Components.Base
{
    [Serializable]
    public class TransformComponent : Component
    {
        public Vector3 LocalPosition { get; set; } = Vector3.Zero;
        public Vector3 LocalScale { get; set; } = Vector3.One;

        // Separate Euler angles
        public float Pitch { get; set; } = 0f; // X
        public float Yaw { get; set; } = 0f;   // Y
        public float Roll { get; set; } = 0f;  // Z

        private TransformComponent? parent = null;
        public ChildrenContainer Children { get; private set; }

		[JsonIgnore]
		public Vector3 Position
        {
            get { return parent != null ? parent.TransformPoint(LocalPosition) : LocalPosition; }
            set { LocalPosition = parent != null ? parent.InverseTransformPoint(value) : value; }
        }
		[JsonIgnore]
		public Vector3 Scale
        {
            get { return parent != null ? parent.TransformVector(LocalScale) : LocalScale; }
            set { LocalScale = parent != null ? parent.InverseTransformVector(value) : value; }
        }

        [JsonIgnore]
        public Vector3 Forward
        {
            get { return Vector3.Transform(Vector3.UnitZ, GetRotationMatrix().ExtractRotation()); }
        }

        [JsonIgnore]
        public Vector3 Up
        {
            get { return Vector3.Transform(Vector3.UnitY, GetRotationMatrix().ExtractRotation()); }
        }

        [JsonIgnore]
        public Vector3 Right
        {
            get { return Vector3.Transform(Vector3.UnitX, GetRotationMatrix().ExtractRotation()); }
        }

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

        public TransformComponent()
        {
            //gameObject = attachedTo;
            LocalPosition = Vector3.Zero;
            Pitch = Yaw = Roll = 0f;
            LocalScale = Vector3.One;
            Children = new ChildrenContainer();
        }

        public Matrix4 GetModelMatrix()
        {
            Matrix4 translationMatrix = Matrix4.CreateTranslation(Position);
            Matrix4 rotationMatrix = GetRotationMatrix();
            Matrix4 scaleMatrix = Matrix4.CreateScale(Scale);

            return scaleMatrix * rotationMatrix * translationMatrix;
        }

        public Matrix4 GetLocalRotationMatrix()
        {
            Matrix4 rollMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Roll));
            Matrix4 pitchMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Pitch));
            Matrix4 yawMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Yaw));

            return rollMatrix * pitchMatrix * yawMatrix;
        }
		public Matrix4 GetWorldRotationMatrix()
		{
			Matrix4 rollMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(WorldRoll));
			Matrix4 pitchMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(WorldPitch));
			Matrix4 yawMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(WorldYaw));

			return rollMatrix * pitchMatrix * yawMatrix;
		}
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

        public Vector3 TransformPoint(Vector3 point)
        {
            return Vector3.Transform(point, GetRotationMatrix().ExtractRotation()) * Scale + Position;
        }

        public Vector3 InverseTransformPoint(Vector3 point)
        {
            Vector3 invertedScale = new Vector3(1.0f / Scale.X, 1.0f / Scale.Y, 1.0f / Scale.Z);

            return Vector3.Transform(point - Position, Matrix4.Invert(GetRotationMatrix()).ExtractRotation()) * invertedScale;
        }

        public Vector3 TransformVector(Vector3 localVector)
        {
            return Vector3.Transform(localVector, GetRotationMatrix().ExtractRotation()) * Scale;
        }

        public Vector3 InverseTransformVector(Vector3 worldVector)
        {
            Vector3 invertedScale = new Vector3(1.0f / Scale.X, 1.0f / Scale.Y, 1.0f / Scale.Z);

            return Vector3.Transform(worldVector / Scale, Matrix4.Invert(GetRotationMatrix()).ExtractRotation());
        }

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

		[JsonIgnore]
		public float WorldPitch
		{
			get
			{
				if (parent != null)
					return parent.WorldPitch + LocalRotation.X;
				else
					return LocalRotation.X;
			}
		}

		[JsonIgnore]
		public float WorldYaw
		{
			get
			{
				if (parent != null)
					return parent.WorldYaw + LocalRotation.Y;
				else
					return LocalRotation.Y;
			}
		}

		[JsonIgnore] public float WorldRoll
		{
			get
			{
				if (parent != null)
					return parent.WorldRoll + LocalRotation.Z;
				else
					return LocalRotation.Z;
			}
		}
		public static Vector3 LookAtRotation(Vector3 eye, Vector3 target)
		{
			Vector3 forward = Vector3.Normalize(target - eye);

			float yaw = (float)Math.Atan2(forward.X, forward.Z);
			float pitch = (float)Math.Asin(forward.Y);

			float yawDegrees = MathHelper.RadiansToDegrees(yaw);
			float pitchDegrees = MathHelper.RadiansToDegrees(pitch);

			return new Vector3(-pitchDegrees, yawDegrees, 0f);
		}

	}
}
