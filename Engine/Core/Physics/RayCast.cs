using Assimp;
using Assimp.Unmanaged;
using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.SceneSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Engine.Core.Physics
{
	public static class Physics
	{
		public static bool RayCast_Triangle(CameraComponent camera, Vector2 MousePosition, float maxDistance, out RayCastHit hitInfo, TagsContainer tags = null)
		{
			Ray ray = Ray.GetRayFromScreenCoordinates(MousePosition, camera.ProjectionMatrix, camera.ViewMatrix, EngineWindow.instance.ClientSize.X, EngineWindow.instance.ClientSize.Y);
			Vector3 rayOrigin = ray.Origin;
			Vector3 rayDirection = ray.Direction;
			return RayCast_Triangle(rayOrigin, rayDirection, maxDistance, out hitInfo);

		}
		public static bool RayCast_Triangle(Vector3 origin, Vector3 direction, float maxDistance, out RayCastHit hitInfo, TagsContainer tags = null)
		{
			hitInfo = new RayCastHit();
			float closestDistance = maxDistance;
			bool hit = false;

			Ray ray = new Ray(origin, direction);
			List<ModelRenderer> models = GetAllRenderers(tags);

			foreach (var modelRenderer in models)
			{
				Model model = modelRenderer.Model;
				Matrix4 modelTransform = modelRenderer.transform.GetModelMatrix();
				foreach (var mesh in model.meshes)
				{
					for (int i = 0; i < mesh.indices.Count; i += 3)
					{
						Vector3 v0 = Vector3.TransformPosition(mesh.vertices[(int)mesh.indices[i]].Position, modelTransform);
						Vector3 v1 = Vector3.TransformPosition(mesh.vertices[(int)mesh.indices[i + 1]].Position, modelTransform);
						Vector3 v2 = Vector3.TransformPosition(mesh.vertices[(int)mesh.indices[i + 2]].Position, modelTransform);

						if (RayIntersections.RayIntersectsTriangle(ray, v0, v1, v2, out float distance, out Vector3 intersectionPoint))
						{
							if (distance < closestDistance)
							{
								closestDistance = distance;
								hit = true;
								hitInfo = new RayCastHit
								{
									Point = intersectionPoint,
									Mesh = mesh,
									Model = model,
									Triangle = (v0, v1, v2),
									Distance = distance,
									gameObject = modelRenderer.gameObject
								};
							}
						}
					}
				}
			}

			// Return false if no hit or if the hit is at maxDistance
			if (!hit || closestDistance == maxDistance)
			{
				return false;
			}

			return true;
		}
		private static List<ModelRenderer> GetAllRenderers(TagsContainer tags = null)
		{
			var models = new List<ModelRenderer>();

			foreach(ModelRenderer rend in SceneManager.ActiveScene.Renderers)
			{
				if(rend != null)
				{
					if (rend.Model == null) continue;
					if(tags == null || tags.Count==0 || rend.RenderTags.HasAny(tags))
					{
						models.Add(rend);
					}
				}
			}
			return models;
		}
	}
	public static class RayIntersections
	{
		public static bool RayIntersectsTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out float distance, out Vector3 intersectionPoint)
		{
			distance = 0.0f;
			intersectionPoint = Vector3.Zero;

			Vector3 edge1 = v1 - v0;
			Vector3 edge2 = v2 - v0;
			Vector3 h = Vector3.Cross(ray.Direction, edge2);
			float a = Vector3.Dot(edge1, h);

			if (Math.Abs(a) < 0.00001f)
				return false;

			float f = 1.0f / a;
			Vector3 s = ray.Origin - v0;
			float u = f * Vector3.Dot(s, h);

			if (u < 0.0f || u > 1.0f)
				return false;

			Vector3 q = Vector3.Cross(s, edge1);
			float v = f * Vector3.Dot(ray.Direction, q);

			if (v < 0.0f || u + v > 1.0f)
				return false;

			float t = f * Vector3.Dot(edge2, q);
			if (t > 0.00001f)
			{
				distance = t;
				intersectionPoint = ray.GetPoint(distance);
				return true;
			}

			return false;
		}
	}
	public class Ray
	{
		public Vector3 Origin { get; set; }
		public Vector3 Direction { get; set; }

		public Ray(Vector3 origin, Vector3 direction)
		{
			Origin = origin;
			Direction = direction.Normalized();
		}

		public Vector3 GetPoint(float distance)
		{
			return Origin + Direction * distance;
		}
		public static Ray GetRayFromScreenCoordinates(Vector2 screenCoordinates, Matrix4 projectionMatrix, Matrix4 viewMatrix, float screenWidth, float screenHeight)
		{
			// Convert screen coordinates to NDC
			Vector2 ndc;
			ndc.X = (2.0f * screenCoordinates.X) / screenWidth - 1.0f;
			ndc.Y = 1.0f - (2.0f * screenCoordinates.Y) / screenHeight;

			// Ray in clip space
			Vector4 rayClip = new Vector4(ndc.X, ndc.Y, -1.0f, 1.0f);

			// Transform ray to eye space
			Matrix4 invertedProjectionMatrix = projectionMatrix.Inverted();
			Vector4 rayEye = Vector4.TransformRow(rayClip, invertedProjectionMatrix);
			rayEye.Z = -1.0f;
			rayEye.W = 0.0f;

			// Transform ray to world space
			Matrix4 invertedViewMatrix = viewMatrix.Inverted();
			Vector3 rayWorld = Vector4.TransformRow(rayEye, invertedViewMatrix).Xyz;
			rayWorld.Normalize();

			Vector3 rayOrigin = invertedViewMatrix.Row3.Xyz;

			return new Ray(rayOrigin, rayWorld);
		}
	}

	public struct RayCastHit
	{
		public Vector3 Point;
		public Mesh Mesh;
		public Model Model;
		public (Vector3 v0, Vector3 v1, Vector3 v2) Triangle;
		public float Distance;
		internal GameObject gameObject;
	}
}