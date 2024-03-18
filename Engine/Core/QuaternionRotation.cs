using OpenTK.Mathematics;

namespace PGK2.Engine.Core
{
	[Serializable]
	public class QuaternionRotation
	{
		public Quaternion Quaternion;
		public Quaternion Invert => Quaternion.Inverted();
		public QuaternionRotation()
		{
			Quaternion = Quaternion.Identity;
		}

		public QuaternionRotation(Quaternion quaternion)
		{
			Quaternion = quaternion;
		}

		public Vector3 EulerAngles
		{
			get
			{
				Vector3 anglesInRad = Quaternion.ToEulerAngles();
				return ToDegrees(anglesInRad);
			}
			set
			{
				Quaternion = Quaternion.FromEulerAngles(ToRadians(value));
			}
		}

		public static Vector3 ToRadians(Vector3 degrees)
		{
			return new Vector3(
				MathHelper.DegreesToRadians(degrees.X),
				MathHelper.DegreesToRadians(degrees.Y),
				MathHelper.DegreesToRadians(degrees.Z)
			);
		}

		public static Vector3 ToDegrees(Vector3 radians)
		{
			return new Vector3(
				MathHelper.RadiansToDegrees(radians.X),
				MathHelper.RadiansToDegrees(radians.Y),
				MathHelper.RadiansToDegrees(radians.Z)
			);
		}
		public QuaternionRotation RotateX(float angleInDegrees)
		{
			Quaternion xRotation = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(angleInDegrees));
			Quaternion = xRotation * Quaternion;
			return this;
		}

		public QuaternionRotation RotateY(float angleInDegrees)
		{
			Quaternion yRotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(angleInDegrees));
			Quaternion = yRotation * Quaternion;
			return this;
		}

		public QuaternionRotation RotateZ(float angleInDegrees)
		{
			Quaternion zRotation = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(angleInDegrees));
			Quaternion = zRotation * Quaternion;
			return this;
		}
		public static QuaternionRotation operator +(QuaternionRotation rotation, Vector3 eulerAngleDelta)
		{
			// Convert the Euler angle delta to radians
			Vector3 eulerAngleDeltaInRadians = ToRadians(eulerAngleDelta);

			// Create quaternions representing the incremental rotations around each axis
			Quaternion xRotation = Quaternion.FromAxisAngle(Vector3.UnitX, eulerAngleDeltaInRadians.X);
			Quaternion yRotation = Quaternion.FromAxisAngle(Vector3.UnitY, eulerAngleDeltaInRadians.Y);
			Quaternion zRotation = Quaternion.FromAxisAngle(Vector3.UnitZ, eulerAngleDeltaInRadians.Z);

			// Combine the incremental rotations to get the total rotation
			Quaternion deltaQuaternion = xRotation * yRotation * zRotation;

			// Multiply the current rotation by the delta quaternion
			rotation.Quaternion = deltaQuaternion * rotation.Quaternion;
			Console.WriteLine(rotation.EulerAngles);
			return rotation;
		}
		public override string ToString()
		{
			Vector3 eulers = EulerAngles;
			return $"X: {eulers.X}, Y: {eulers.Y}, Z: {eulers.Z}";
		}
	}
}