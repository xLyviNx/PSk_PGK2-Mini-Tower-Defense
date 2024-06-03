using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Core.Physics;
using PGK2.Engine.Core;

public class PathFindingAgent : Component
{
	public Vector3 TargetPosition { get; private set; }
	public float StepOffset { get; private set; } = 0.8f;
	public float Speed = 1f;
	public float InitialClearanceDistance = 0.3f; // Initial distance to keep from walls
	public float MinClearanceDistance = 0.0f; // Minimum allowable clearance distance
	public float ClearanceStep = 0.01f; // Step by which to decrease clearance distance
	int waypoint = 0;
	public List<Vector3> Path { get; private set; }

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
		var key = (transform.Position, targetPosition);
		if (AStarPathfinding.pathCache.TryGetValue(key, out var cachedPath))
		{
			Path = cachedPath;
			return;
		}
		CalculatePath();
	}

	private void CalculatePath()
	{
		Path.Clear();
		Path = AStarPathfinding.FindPath(transform.Position, TargetPosition, StepOffset, 0.1f, InitialClearanceDistance, MinClearanceDistance, ClearanceStep);

		if (Path == null || Path.Count == 0)
		{
			Console.WriteLine("Unable to find a path.");
		}
		else
		{
			// Ensure the final target position is included
			if (Path[Path.Count - 1] != TargetPosition)
			{
				Path.Add(TargetPosition);
			}
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
		if (Path == null || Path.Count == 0 || waypoint>Path.Count)
			return;

		Vector3 nextWaypoint = Path[waypoint];
		transform.Position = MoveTowards(transform.Position, nextWaypoint, Time.deltaTime * Speed);

		if ((transform.Position - nextWaypoint).LengthFast < 0.1f)
		{
			waypoint++;
		}
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
	internal static Dictionary<(Vector3, Vector3), List<Vector3>> pathCache = new Dictionary<(Vector3, Vector3), List<Vector3>>();

	public static List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos, float stepOffset, float maxStepHeight, float initialClearanceDistance, float minClearanceDistance, float clearanceStep)
	{
		// Check if the path is already in the cache
		var key = (startPos, targetPos);
		if (pathCache.TryGetValue(key, out var cachedPath))
		{
			return new List<Vector3>(cachedPath); // Return a copy of the cached path
		}

		List<Vector3> path = null;

		// Attempt to find a path with progressively smaller clearance distances
		for (float clearanceDistance = initialClearanceDistance; clearanceDistance >= minClearanceDistance; clearanceDistance -= clearanceStep)
		{
			path = FindPathWithClearance(startPos, targetPos, stepOffset, maxStepHeight, clearanceDistance);
			if (path != null && path.Count > 0)
			{
				break;
			}
		}

		if (path == null || path.Count == 0)
		{
			Console.WriteLine("No path found with any clearance distance.");
		}
		else
		{
			pathCache[key] = new List<Vector3>(path); // Cache the found path
		}

		return path;
	}

	private static List<Vector3> FindPathWithClearance(Vector3 startPos, Vector3 targetPos, float stepOffset, float maxStepHeight, float clearanceDistance)
	{
		List<Vector3> openList = new List<Vector3> { startPos };
		HashSet<Vector3> closedList = new HashSet<Vector3>();
		Dictionary<Vector3, float> gCost = new Dictionary<Vector3, float> { [startPos] = 0 };
		Dictionary<Vector3, float> hCost = new Dictionary<Vector3, float> { [startPos] = Heuristic(startPos, targetPos) };
		Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();

		while (openList.Count > 0)
		{
			Vector3 current = GetLowestFCostNode(openList, gCost, hCost);

			if (Vector3.Distance(current, targetPos) < stepOffset)
			{
				Console.WriteLine($"Path found with clearance distance {clearanceDistance}.");
				return ReconstructPath(cameFrom, current);
			}

			openList.Remove(current);
			closedList.Add(current);

			foreach (Vector3 neighbor in GetNeighbors(current, stepOffset, maxStepHeight, clearanceDistance))
			{
				if (closedList.Contains(neighbor))
				{
					continue;
				}

				if (!gCost.ContainsKey(neighbor))
				{
					gCost[neighbor] = float.MaxValue;
				}

				float tentativeGCost = gCost[current] + Vector3.Distance(current, neighbor);

				if (tentativeGCost < gCost[neighbor])
				{
					cameFrom[neighbor] = current;
					gCost[neighbor] = tentativeGCost;
					hCost[neighbor] = Heuristic(neighbor, targetPos);

					if (!openList.Contains(neighbor))
					{
						openList.Add(neighbor);
					}
				}
			}
		}

		return null; // Return null if no path is found
	}

	private static float Heuristic(Vector3 a, Vector3 b)
	{
		return Vector3.Distance(a, b);
	}

	private static Vector3 GetLowestFCostNode(List<Vector3> openList, Dictionary<Vector3, float> gCost, Dictionary<Vector3, float> hCost)
	{
		Vector3 lowest = openList[0];
		float lowestFCost = gCost[lowest] + hCost[lowest];

		foreach (Vector3 node in openList)
		{
			float fCost = gCost[node] + hCost[node];
			if (fCost < lowestFCost)
			{
				lowest = node;
				lowestFCost = fCost;
			}
		}

		return lowest;
	}

	private static List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current)
	{
		List<Vector3> totalPath = new List<Vector3> { current };
		while (cameFrom.ContainsKey(current))
		{
			current = cameFrom[current];
			totalPath.Add(current);
		}
		totalPath.Reverse();
		return totalPath;
	}

	private static List<Vector3> GetNeighbors(Vector3 currentPos, float stepOffset, float maxStepHeight, float clearanceDistance)
	{
		List<Vector3> neighbors = new List<Vector3>();

		Vector3[] directions = new Vector3[]
		{
			new Vector3(stepOffset, 0, 0),
			new Vector3(-stepOffset, 0, 0),
			new Vector3(0, 0, stepOffset),
			new Vector3(0, 0, -stepOffset),
			new Vector3(0, stepOffset, 0), // Moving up
            new Vector3(0, -stepOffset, 0) // Moving down
        };

		foreach (Vector3 direction in directions)
		{
			Vector3 neighbor = currentPos + direction;
			RayCastHit hitInfo;

			if (CheckForObstacles(currentPos, direction, stepOffset, clearanceDistance, out hitInfo))
			{
				float heightDifference = Math.Abs(currentPos.Y - neighbor.Y);
				if (heightDifference <= maxStepHeight)
				{
					neighbors.Add(neighbor);
				}
			}
		}

		return neighbors;
	}

	private static bool CheckForObstacles(Vector3 currentPos, Vector3 direction, float stepOffset, float clearanceDistance, out RayCastHit hitInfo)
	{
		TagsContainer tags = new TagsContainer();
		tags.Add("map");

		// Perform a raycast with clearance distance added to the step offset
		bool hit = Physics.RayCast_Triangle(currentPos, direction, stepOffset + clearanceDistance, out hitInfo, tags);
		return !hit;
	}
}