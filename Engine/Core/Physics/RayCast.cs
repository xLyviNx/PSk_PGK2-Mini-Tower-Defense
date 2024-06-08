using Assimp;
using Assimp.Unmanaged;
using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using PGK2.Engine.SceneSystem;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PGK2.Engine.Core.Physics
{
	public static class Physics
	{
		public static bool RayCast_Triangle(CameraComponent camera, Vector2 MousePosition, float maxDistance, out RayCastHit hitInfo, TagsContainer tags)
		{
			if(camera==null)
			{
				Console.WriteLine("[RAYCAST] Camera is null");
				hitInfo = new();
				return false;
			}	
			Ray ray = Ray.GetRayFromScreenCoordinates(MousePosition, camera.ProjectionMatrix, camera.ViewMatrix, EngineWindow.instance.ClientSize.X, EngineWindow.instance.ClientSize.Y);
			Vector3 rayOrigin = ray.Origin;
			Vector3 rayDirection = ray.Direction;
			return RayCast_Triangle(rayOrigin, rayDirection, maxDistance, out hitInfo, tags);
		}

		public static bool RayCast_Triangle(Vector3 origin, Vector3 direction, float maxDistance, out RayCastHit hitInfo, TagsContainer tags)
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
						if (RayIntersections.RayIntersectsTriangle(ray, mesh, i / 3, modelTransform, out float distance, out Vector3 intersectionPoint))
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
									TriangleIndex = i / 3,
									Distance = distance,
									gameObject = modelRenderer.gameObject
								};
							}
						}
					}
				}
			}

			return hit;
		}

		private static List<ModelRenderer> GetAllRenderers(TagsContainer tags)
		{
			var models = new List<ModelRenderer>();

			foreach (ModelRenderer rend in SceneManager.ActiveScene.Renderers)
			{
				if (rend != null)
				{
					if (rend.Model == null) continue;
					if ( tags.Count == 0 || rend.gameObject.Tags.HasAny(tags))
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
		public static bool RayIntersectsTriangle(Ray ray, Mesh mesh, int triangleIndex, Matrix4 modelTransform, out float distance, out Vector3 intersectionPoint)
		{
			distance = 0.0f;
			intersectionPoint = Vector3.Zero;

			int i0 = (int)mesh.indices[triangleIndex * 3 + 0];
			int i1 = (int)mesh.indices[triangleIndex * 3 + 1];
			int i2 = (int)mesh.indices[triangleIndex * 3 + 2];

			Vector3 v0 = Vector3.TransformPosition(mesh.vertices[i0].Position, modelTransform);
			Vector3 v1 = Vector3.TransformPosition(mesh.vertices[i1].Position, modelTransform);
			Vector3 v2 = Vector3.TransformPosition(mesh.vertices[i2].Position, modelTransform);

			Vector3 edge1 = v1 - v0;
			Vector3 edge2 = v2 - v0;

			Vector3 h = Vector3.Cross(ray.Direction, edge2);
			float a = Vector3.Dot(edge1, h);
			if (Math.Abs(a) < 0.00001f)
			{
				return false; // Promień jest równoległy do trójkąta
			}

			float f = 1.0f / a;
			Vector3 s = ray.Origin - v0;
			float u = f * Vector3.Dot(s, h);
			if (u < 0.0f || u > 1.0f)
			{
				return false; // Punkt przecięcia jest poza trójkątem
			}

			Vector3 q = Vector3.Cross(s, edge1);
			float v = f * Vector3.Dot(ray.Direction, q);
			if (v < 0.0f || u + v > 1.0f)
			{
				return false; // Punkt przecięcia jest poza trójkątem
			}

			distance = f * Vector3.Dot(edge2, q);
			if (distance > 0.00001f)
			{
				intersectionPoint = ray.Origin + distance * ray.Direction;
				return true;
			}
			else
			{
				return false; // Punkt przecięcia jest za promieniem
			}
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
		public int TriangleIndex;
		public float Distance;
		internal GameObject gameObject;
	}

	public struct Triangle
	{
		public Vector3[] Vertices;
		public Vector3 PlaneNormal;
		public float PlaneDistance;
		public Vector3 PlaneUnitU;
		public Vector3 PlaneUnitV;
		public float EdgeU;
		public float EdgeV;

		public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
		{
			Vertices = new Vector3[] { v0, v1, v2 };

			Vector3 edge1 = v1 - v0;
			Vector3 edge2 = v2 - v0;
			PlaneNormal = Vector3.Cross(edge1, edge2).Normalized();
			PlaneDistance = Vector3.Dot(PlaneNormal, v0);

			PlaneUnitU = edge1.Normalized();
			PlaneUnitV = Vector3.Cross(PlaneNormal, edge1).Normalized();

			EdgeU = Vector3.Dot(v1 - v0, PlaneUnitU);
			EdgeV = Vector3.Dot(v2 - v0, PlaneUnitV);
		}
	}
}