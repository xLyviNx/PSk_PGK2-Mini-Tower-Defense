using OpenTK.Mathematics;
using PGK2.Engine.Core;

namespace PGK2.Engine.Components
{
	public class SpinTest : Component
	{
		public override void Update()
		{
			base.Update();
			transform.Rotation += Vector3.One * 50f *(float)Time.deltaTime;
		}
		public SpinTest() { }
	}
}
