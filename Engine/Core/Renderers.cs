using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Engine.Core
{
	[Serializable]
	public class Renderer : Component
	{
		public Material[] Materials { get; set; }

		public Renderer()
		{
			Materials = new Material[1]; // Default material
		}

		public virtual void Render(Matrix4 viewMatrix, Matrix4 projectionMatrix)
		{
			// Implement rendering logic using Materials, viewMatrix, and projectionMatrix
		}
	}

	[Serializable]
	public class MeshRenderer : Renderer
	{
		// Additional properties or methods specific to MeshRenderer
		// (e.g., Mesh object)
	}

	[Serializable]
	public class SkinnedMeshRenderer : Renderer
	{
		// Additional properties or methods specific to SkinnedMeshRenderer
		// (e.g., Skeleton, BoneWeights, etc.)
	}
}
