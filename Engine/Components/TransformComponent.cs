using OpenTK;
using OpenTK.Mathematics;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;

namespace Game.Engine.Components
{
    /// <summary>
    /// Represents a component that manages the position, rotation, and scale of an object in 3D space.
    /// </summary>
    public class TransformComponent : Component
	{
		[SerializeField] private Vector3 localPosition = Vector3.Zero;
		[SerializeField] private Quaternion localRotation = Quaternion.Identity;
		[SerializeField] private Vector3 localScale = Vector3.One;
		[SerializeField] private TransformComponent? parent = null;

		/// <summary>
		/// Gets or sets the global position of the object.
		/// </summary>
		public Vector3 Position
		{
			get { return (parent != null) ? parent.TransformPoint(localPosition) : localPosition; }
			set { localPosition = (parent != null) ? parent.InverseTransformPoint(value) : value; }
		}

		/// <summary>
		/// Gets or sets the global rotation of the object.
		/// </summary>
		public Quaternion Rotation
		{
			get { return (parent != null) ? parent.TransformRotation(localRotation) : localRotation; }
			set { localRotation = (parent != null) ? parent.InverseTransformRotation(value) : value; }
		}

		/// <summary>
		/// Gets or sets the global scale of the object.
		/// </summary>
		public Vector3 Scale
		{
			get { return (parent != null) ? parent.TransformVector(localScale) : localScale; }
			set { localScale = (parent != null) ? parent.InverseTransformVector(value) : value; }
		}

		/// <summary>
		/// Gets the forward direction of the object in world space.
		/// </summary>
		public Vector3 Forward
		{
			get { return Vector3.Transform(Vector3.UnitZ, Rotation); }
		}

		/// <summary>
		/// Gets the up direction of the object in world space.
		/// </summary>
		public Vector3 Up
		{
			get { return Vector3.Transform(Vector3.UnitY, Rotation); }
		}

		/// <summary>
		/// Gets the right direction of the object in world space.
		/// </summary>
		public Vector3 Right
		{
			get { return Vector3.Cross(Forward, Up); }
		}

		/// <summary>
		/// Gets or sets the parent (or null if no parent) of the object.
		/// </summary>
		public TransformComponent? Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformComponent"/> class with default values.
		/// </summary>
		public TransformComponent()
		{
			localPosition = Vector3.Zero;
			localRotation = Quaternion.Identity;
			localScale = Vector3.One;
		}

		/// <summary>
		/// Gets the model matrix representing the combined transformations (translation, rotation, scale).
		/// </summary>
		public Matrix4 GetModelMatrix()
		{
			Matrix4 translationMatrix = Matrix4.CreateTranslation(Position);
			Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(Rotation);
			Matrix4 scaleMatrix = Matrix4.CreateScale(Scale);

			return scaleMatrix * rotationMatrix * translationMatrix;
		}

		/// <summary>
		/// Transforms a local point to global space.
		/// </summary>
		/// <param name="point">The local point to transform.</param>
		/// <returns>The point in global space.</returns>
		public Vector3 TransformPoint(Vector3 point)
		{
			return Vector3.Transform(point, Rotation) * Scale + Position;
		}

		/// <summary>
		/// Transforms a global point to local space.
		/// </summary>
		/// <param name="point">The global point to transform.</param>
		/// <returns>The point in local space.</returns>
		public Vector3 InverseTransformPoint(Vector3 point)
		{
			Vector3 invertedScale = new Vector3(1.0f / Scale.X, 1.0f / Scale.Y, 1.0f / Scale.Z);
			return Vector3.Transform(point - Position, Quaternion.Invert(Rotation)) / invertedScale;
		}

		/// <summary>
		/// Transforms a local rotation to global space.
		/// </summary>
		/// <param name="localRotation">The local rotation to transform.</param>
		/// <returns>The rotation in global space.</returns>
		public Quaternion TransformRotation(Quaternion localRotation)
		{
			return Rotation * localRotation;
		}

		/// <summary>
		/// Transforms a global rotation to local space.
		/// </summary>
		/// <param name="worldRotation">The global rotation to transform.</param>
		/// <returns>The rotation in local space.</returns>
		public Quaternion InverseTransformRotation(Quaternion worldRotation)
		{
			return Quaternion.Invert(Rotation) * worldRotation;
		}

		/// <summary>
		/// Transforms a local vector to global space.
		/// </summary>
		/// <param name="localVector">The local vector to transform.</param>
		/// <returns>The vector in global space.</returns>
		public Vector3 TransformVector(Vector3 localVector)
		{
			return Vector3.Transform(localVector, Rotation) * Scale;
		}

		/// <summary>
		/// Transforms a global vector to local space.
		/// </summary>
		/// <param name="worldVector">The global vector to transform.</param>
		/// <returns>The vector in local space.</returns>
		public Vector3 InverseTransformVector(Vector3 worldVector)
		{
			return Vector3.Transform(worldVector / Scale, Quaternion.Invert(Rotation));
		}
	}
}
