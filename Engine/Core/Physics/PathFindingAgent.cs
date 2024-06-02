using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Core;
using PGK2.Engine.Core.Physics;
using PGK2.Engine.SceneSystem;

public class PathFindingAgent : Component
{
	public Vector3 TargetPosition { get; private set; }
	public float StepOffset { get; private set; } = 0.4f;
	public float Speed = 1f;
	public List<Vector3> Path { get; private set; }
	// Other properties and fields as needed

	public PathFindingAgent(Vector3 position, float stepOffset)
	{
		transform.Position = position;
		StepOffset = stepOffset;
		Path = new List<Vector3>();
	}

	public PathFindingAgent()
	{
		Path = new List<Vector3>();
	}

	public void SetTargetPosition(Vector3 targetPosition)
	{
		TargetPosition = targetPosition;
		CalculatePath();
	}

	private void CalculatePath()
	{
		// Clear the existing path
		Path.Clear();

		// Perform A* pathfinding algorithm
		Path = AStarPathfinding.FindPath(transform.Position, TargetPosition, StepOffset);

		if (Path == null || Path.Count == 0)
		{
			Console.WriteLine("Unable to find a path.");
		}
		else
		{
			Console.WriteLine($"Pathfinding complete. Found {Path.Count} waypoints.");
			DrawPath();
		}
	}

	public void DrawPath()
	{
		foreach (var path in Path)
		{
			DrawWaypoint(path);
		}
	}

	public static GameObject DrawWaypoint(Vector3 pos)
	{
		var cube = TemporaryCube.Create(pos);
		cube.GetComponent<ModelRenderer>().OutlineColor = Color4.Green;
		return cube;
	}

	public override void Update()
	{
		base.Update();
		Move();
	}

	public void Move()
	{
		if (Path == null || Path.Count == 0)
			return;

		// Move towards the next waypoint in the path
		Vector3 nextWaypoint = Path[0];

		transform.Position = MoveTowards(transform.Position, nextWaypoint, Time.deltaTime * Speed);

		if ((transform.Position - nextWaypoint).Length < StepOffset)
			Path.RemoveAt(0);
	}

	public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
	{
		Vector3 direction = target - current;
		float distance = direction.Length;

		if (distance <= maxDistanceDelta || distance == 0f)
		{
			return target;
		}

		return current + direction / distance * maxDistanceDelta;
	}
}

public static class AStarPathfinding
{
	public static List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos, float stepOffset)
	{
		// Placeholder implementation with obstacle detection and avoidance using raycasting
		List<Vector3> path = new List<Vector3>();

		// Add start position to the path
		path.Add(startPos);

		// Calculate intermediate points between start and target
		Vector3 direction = (targetPos - startPos).Normalized();
		float distance = (targetPos - startPos).Length;
		Vector3 currentPos = startPos;

		while (distance > stepOffset)
		{
			currentPos += direction * stepOffset;

			// Perform raycast to check for obstacles
			TagsContainer tags = new TagsContainer();
			if (!Physics.RayCast_Triangle(currentPos, direction, stepOffset, out RayCastHit hitInfo, tags))
			{
				// No obstacle, add current position to the path
				path.Add(currentPos);
			}
			else
			{
				var block = PathFindingAgent.DrawWaypoint(hitInfo.Point);
				block.GetComponent<ModelRenderer>().OutlineColor = Color4.Yellow;
				// Obstacle detected, adjust position or abort pathfinding
				Console.WriteLine($"Obstacle detected at {hitInfo.Point}. Adjusting path...");
				Vector3 triangleNormal = Vector3.Cross(hitInfo.Triangle.v1 - hitInfo.Triangle.v0, hitInfo.Triangle.v2 - hitInfo.Triangle.v0).Normalized();

				// Calculate a new direction to avoid the obstacle
				Vector3 newDirection = CalculateAvoidanceDirection(direction, triangleNormal);

				// Update current position based on the new direction
				currentPos += newDirection * stepOffset;

				// Add the adjusted position to the path
				path.Add(currentPos);

				// Reset the distance to the target since we've adjusted the path
				distance = (targetPos - currentPos).Length;
			}

			distance -= stepOffset;
		}

		// Add target position to the path
		path.Add(targetPos);

		return path;
	}

	private static Vector3 CalculateAvoidanceDirection(Vector3 currentDirection, Vector3 obstacleNormal)
	{
		// Reflect the current direction vector against the obstacle normal
		Vector3 reflection = currentDirection - 2 * Vector3.Dot(currentDirection, obstacleNormal) * obstacleNormal;

		// Add a small random perturbation to avoid sticking to walls
		Random random = new Random();
		Vector3 avoidanceDirection = reflection + (Vector3.UnitX * ((float)random.NextDouble() * 0.2f - 0.1f)) + (Vector3.UnitY * ((float)random.NextDouble() * 0.2f - 0.1f));

		return avoidanceDirection.Normalized();
	}
}
