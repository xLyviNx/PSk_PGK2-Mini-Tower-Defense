using OpenTK;
using OpenTK.Mathematics;
using PGK2.Engine.Core;
using PGK2.Engine.SceneSystem;
using System.Text.Json.Serialization;

namespace Game.Engine.Components
{
    /// <summary>
    /// Represents a component that manages the position, rotation, and scale of an object in 3D space.
    /// </summary>
    [Serializable]
    public class TransformComponent : Component
	{
		public Vector3 localPosition = Vector3.Zero;
		public QuaternionRotation localRotation = new QuaternionRotation();
		public Vector3 localScale = Vector3.One;
		private TransformComponent? parent = null;
		public ChildrenContainer children { get; private set; }
		public Guid parentId => (parent != null && parent.gameObject!=null ? parent.gameObject.Id : Guid.Empty);

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
		public QuaternionRotation Rotation
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
		[JsonIgnore]
		public Vector3 Forward
		{
			get { return Vector3.Transform(Vector3.UnitZ, Rotation.Quaternion); }
		}

		/// <summary>
		/// Gets the up direction of the object in world space.
		/// </summary>
		[JsonIgnore]
		public Vector3 Up
		{
			get { return Vector3.Transform(Vector3.UnitY, Rotation.Quaternion); }
		}

		/// <summary>
		/// Gets the right direction of the object in world space.
		/// </summary>
		[JsonIgnore]
		public Vector3 Right
		{
			get { return Vector3.Cross(Forward, Up); }
		}

		/// <summary>
		/// Gets or sets the parent (or null if no parent) of the object.
		/// </summary>
		[JsonIgnore]
		public TransformComponent? Parent
		{
			get { return parent; }
			set {

				if(parent != null && children.AllObjects.Contains(parent))
				{
					throw new Exception("Tried making child a parent object.");
				}

				if (parent!=null)
				{
					parent.children.Remove(this);
				}
				parent = value;
				if (parent!=null)
				{
					parent.children.Add(this);
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformComponent"/> class with default values.
		/// </summary>
		public TransformComponent(GameObject attachedTo)
		{
			gameObject = attachedTo;
			localPosition = Vector3.Zero;
			localRotation = new();
			localScale = Vector3.One;
			children = new();
		}

		/// <summary>
		/// Gets the model matrix representing the combined transformations (translation, rotation, scale).
		/// </summary>
		public Matrix4 GetModelMatrix()
		{
			Matrix4 translationMatrix = Matrix4.CreateTranslation(Position);
			Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(Rotation.Quaternion);
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
			return Vector3.Transform(point, Rotation.Quaternion) * Scale + Position;
		}

		/// <summary>
		/// Transforms a global point to local space.
		/// </summary>
		/// <param name="point">The global point to transform.</param>
		/// <returns>The point in local space.</returns>
		public Vector3 InverseTransformPoint(Vector3 point)
		{
			Vector3 invertedScale = new Vector3(1.0f / Scale.X, 1.0f / Scale.Y, 1.0f / Scale.Z);
			Quaternion invertedRotation = Rotation.Invert;

			// Poprawienie kolejności operacji i dodanie skalowania
			return Vector3.Transform(point - Position, invertedRotation) * invertedScale;
		}


		/// <summary>
		/// Transforms a local rotation to global space.
		/// </summary>
		/// <param name="localRotation">The local rotation to transform.</param>
		/// <returns>The rotation in global space.</returns>
		public QuaternionRotation TransformRotation(QuaternionRotation localRotation)
		{
			QuaternionRotation qr = new();
			qr.Quaternion = localRotation.Quaternion * Rotation.Quaternion;
			return qr;
		}

		/// <summary>
		/// Transforms a global rotation to local space.
		/// </summary>
		/// <param name="worldRotation">The global rotation to transform.</param>
		/// <returns>The rotation in local space.</returns>
		public QuaternionRotation InverseTransformRotation(QuaternionRotation worldRotation)
		{
			return new QuaternionRotation(Quaternion.Invert(Rotation.Quaternion) * worldRotation.Quaternion);
		}

		/// <summary>
		/// Transforms a local vector to global space.
		/// </summary>
		/// <param name="localVector">The local vector to transform.</param>
		/// <returns>The vector in global space.</returns>
		public Vector3 TransformVector(Vector3 localVector)
		{
			return Vector3.Transform(localVector, Rotation.Quaternion) * Scale;
		}
		/// <summary>
		/// Transforms a global vector to local space.
		/// </summary>
		/// <param name="worldVector">The global vector to transform.</param>
		/// <returns>The vector in local space.</returns>
		public Vector3 InverseTransformVector(Vector3 worldVector)
		{
			return Vector3.Transform(worldVector / Scale, Rotation.Invert);
		}
	}
	[Serializable]
	public class ChildrenContainer
	{
		[JsonIgnore] public List<TransformComponent> AllObjects;
		[JsonInclude]
		public List<Guid> All
		{
			get
			{
				List<Guid> guids = new();
				foreach (TransformComponent transform in AllObjects)
				{
					Console.WriteLine($"{transform.gameObject}");
					guids.Add(transform.gameObject.Id);
				}
				return guids;
			}
		}

		public bool Remove(TransformComponent child)
		{
			if (!Has(child)) return false;
			AllObjects.Remove(child);
			return true;
		}
		public bool Add(TransformComponent child)
		{
			if (Has(child)) return false;
			AllObjects.Add(child);
			return true;
		}
		public bool Has(TransformComponent child)
		{
			return AllObjects.Contains(child);
		}
		public ChildrenContainer()
		{
			AllObjects = new();
		}
	}
}
