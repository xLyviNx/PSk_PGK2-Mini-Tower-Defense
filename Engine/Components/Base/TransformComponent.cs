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

        // Local Euler angles
        public float LocalPitch { get; set; } = 0f; // X
        public float LocalYaw { get; set; } = 0f;   // Y
        public float LocalRoll { get; set; } = 0f;  // Z

        private TransformComponent? parent = null;
        public ChildrenContainer Children { get; private set; }

        public Vector3 Position
        {
            get { return parent != null ? parent.TransformPoint(LocalPosition) : LocalPosition; }
            set { LocalPosition = parent != null ? parent.InverseTransformPoint(value) : value; }
        }

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
            LocalPitch = LocalYaw = LocalRoll = 0f;
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
            Matrix4 rollMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(LocalRoll));
            Matrix4 pitchMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(LocalPitch));
            Matrix4 yawMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(LocalYaw));

            return rollMatrix * pitchMatrix * yawMatrix;
        }

        public Matrix4 GetRotationMatrix()
        {
            if (parent != null)
            {
                return parent.GetRotationMatrix() * GetLocalRotationMatrix();
            }
            else
            {
                return GetLocalRotationMatrix();
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
                return new Vector3(LocalPitch, LocalYaw, LocalRoll);
            }
            set
            {
                LocalPitch = value.X;
                LocalYaw = value.Y;
                LocalRoll = value.Z;
            }
        }

		public Vector3 Rotation
		{
			get
            {
				if (parent != null)
				{
					return parent.TransformVector(LocalRotation);
				}
				else
				{
                    return LocalRotation;
				}
			}
			set
			{
				if (parent != null)
				{
					LocalRotation = parent.InverseTransformVector(value);
				}
				else
				{
					LocalRotation = value;
				}
			}
		}
	}
}