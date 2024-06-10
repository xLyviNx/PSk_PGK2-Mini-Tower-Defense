using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Core.Physics;
using PGK2.Engine.Core;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.IO;
/// <summary>
/// Klasa PathFindingAgent reprezentuje agenta poruszającego się po ścieżkach.
/// </summary>
public class PathFindingAgent : Component
{
	/// <summary>
	/// Docelowa pozycja agenta.
	/// </summary>
	public Vector3 TargetPosition { get; private set; }

	/// <summary>
	/// Offset kroku agenta.
	/// </summary>
	public float StepOffset { get; private set; } = 0.8f;

	/// <summary>
	/// Prędkość poruszania się agenta.
	/// </summary>
	public float Speed = 3f;

	/// <summary>
	/// Początkowa docelowa odległość (odległości od ściany).
	/// </summary>
	public float InitialClearanceDistance = 0.3f;

	/// <summary>
	/// Minimalna docelowa odległość (odległości od ściany).
	/// </summary>
	public float MinClearanceDistance = 0.0f;

	/// <summary>
	/// Krok zmiany docelowej odległości (odległości od ściany).
	/// </summary>
	public float ClearanceStep = 0.01f;

	/// <summary>
	/// Aktualny waypoint na ścieżce.
	/// </summary>
	public int waypoint = 0;

	/// <summary>
	/// Ścieżka, po której porusza się agent.
	/// </summary>
	public List<Vector3> Path { get; private set; }
	/// <summary>
	/// Konstruktor klasy PathFindingAgent z parametrami pozycji i offsetu kroku.
	/// </summary>
	/// <param name="position">Początkowa pozycja agenta.</param>
	/// <param name="stepOffset">Offset kroku agenta.</param>
	public PathFindingAgent(Vector3 position, float stepOffset)
	{
		transform.Position = position;
		StepOffset = stepOffset;
		Path = new List<Vector3>();
	}
	/// <summary>
	/// Konstruktor domyślny klasy PathFindingAgent.
	/// </summary>
	public PathFindingAgent()
	{
		Path = new List<Vector3>();
	}
	/// <summary>
	/// Ustawia docelową pozycję agenta.
	/// </summary>
	/// <param name="targetPosition">Docelowa pozycja agenta.</param>
	public void SetTargetPosition(Vector3 targetPosition)
	{
		TargetPosition = targetPosition;
		var key = (transform.Position, targetPosition);
		if (Pathfinding.pathCache.TryGetValue(key, out var cachedPath))
		{
			Path = cachedPath;
			return;
		}
		CalculatePath();
	}
	/// <summary>
	/// Oblicza ścieżkę do docelowej pozycji.
	/// </summary>
	private void CalculatePath()
	{
		Path.Clear();
		Path = Pathfinding.FindPath(transform.Position, TargetPosition, StepOffset, 0.1f, InitialClearanceDistance, MinClearanceDistance, ClearanceStep);

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
			var key = (transform.Position, TargetPosition);
			Pathfinding.pathCache[key] = new List<Vector3>(Path); // Cache the found path
			//DrawPath();
		}
	}
	/// <summary>
	/// Rysuje ścieżkę agenta.
	/// </summary>
	public void DrawPath()
	{
		foreach (var path in Path)
		{
			DrawWaypoint(path);
		}
	}
	/// <summary>
	/// Rysuje pojedynczy waypoint.
	/// </summary>
	/// <param name="pos">Pozycja waypointa.</param>
	/// <returns>Obiekt GameObject reprezentujący waypoint.</returns>
	public static GameObject DrawWaypoint(Vector3 pos)
	{
		var cube = TemporaryCube.Create(pos);
		cube.GetComponent<ModelRenderer>().OutlineColor = Color4.Green;
		return cube;
	}
	/// <summary>
	/// Aktualizuje stan agenta.
	/// </summary>
	public override void Update()
	{
		base.Update();
		Move();
	}
	/// <summary>
	/// Przemieszcza agenta wzdłuż ścieżki.
	/// </summary>
	public void Move()
	{
		//Console.WriteLine($"Waypoint: {waypoint}, count: {Path.Count}");
		if (Path == null || Path.Count == 0 || waypoint>=Path.Count)
			return;

		Vector3 nextWaypoint = Path[waypoint];
		transform.Position = MoveTowards(transform.Position, nextWaypoint, Time.deltaTime * Speed);

		if ((transform.Position - nextWaypoint).LengthFast < 0.01f)
		{
			waypoint++;
		}
	}
	/// <summary>
	/// Przemieszcza aktualną pozycję w kierunku docelowej pozycji.
	/// </summary>
	/// <param name="current">Aktualna pozycja.</param>
	/// <param name="target">Docelowa pozycja.</param>
	/// <param name="maxDistanceDelta">Maksymalny dystans przesunięcia.</param>
	/// <returns>Nowa pozycja po przesunięciu.</returns>
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
/// <summary>
/// Statyczna klasa Pathfinding odpowiedzialna za znajdowanie ścieżek.
/// </summary>
public static class Pathfinding
{
	/// <summary>
	/// Pamięć podręczna ścieżek.
	/// </summary>
	internal static Dictionary<(Vector3, Vector3), List<Vector3>> pathCache = new Dictionary<(Vector3, Vector3), List<Vector3>>();
	/// <summary>
	/// Znajduje ścieżkę od pozycji startowej do docelowej.
	/// </summary>
	/// <param name="startPos">Pozycja startowa.</param>
	/// <param name="targetPos">Pozycja docelowa.</param>
	/// <param name="stepOffset">Offset kroku.</param>
	/// <param name="maxStepHeight">Maksymalna wysokość kroku.</param>
	/// <param name="initialClearanceDistance">Początkowa odległość od sciany.</param>
	/// <param name="minClearanceDistance">Minimalna odległość od sciany.</param>
	/// <param name="clearanceStep">Krok odległości od sciany.</param>
	/// <returns>Lista wektorów reprezentujących ścieżkę.</returns>
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

		return path;
	}
	/// <summary>
	/// Znajduje ścieżkę z uwzględnieniem odległości od sciany.
	/// </summary>
	/// <param name="startPos">Pozycja startowa.</param>
	/// <param name="targetPos">Pozycja docelowa.</param>
	/// <param name="stepOffset">Offset kroku.</param>
	/// <param name="maxStepHeight">Maksymalna wysokość kroku.</param>
	/// <param name="clearanceDistance">Odległość od sciany.</param>
	/// <returns>Lista wektorów reprezentujących ścieżkę.</returns>
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
	/// <summary>
	/// Oblicza heurystykę pomiędzy dwoma punktami.
	/// </summary>
	/// <param name="a">Pierwszy punkt.</param>
	/// <param name="b">Drugi punkt.</param>
	/// <returns>Odległość pomiędzy punktami.</returns>
	private static float Heuristic(Vector3 a, Vector3 b)
	{
		return Vector3.Distance(a, b);
	}
	/// <summary>
	/// Znajduje węzeł o najniższym koszcie F.
	/// </summary>
	/// <param name="openList">Lista otwartych węzłów.</param>
	/// <param name="gCost">Koszt G węzłów.</param>
	/// <param name="hCost">Koszt H węzłów.</param>
	/// <returns>Węzeł o najniższym koszcie F.</returns>
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
	/// <summary>
	/// Rekonstruuje ścieżkę na podstawie słownika cameFrom.
	/// </summary>
	/// <param name="cameFrom">Słownik z informacjami o pochodzeniu węzłów.</param>
	/// <param name="current">Aktualny węzeł.</param>
	/// <returns>Lista wektorów reprezentujących ścieżkę.</returns>
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
	/// <summary>
	/// Znajduje sąsiadujące węzły dla danego węzła.
	/// </summary>
	/// <param name="currentPos">Aktualna pozycja węzła.</param>
	/// <param name="stepOffset">Offset kroku.</param>
	/// <param name="maxStepHeight">Maksymalna wysokość kroku.</param>
	/// <param name="clearanceDistance">Odległość od sciany.</param>
	/// <returns>Lista wektorów reprezentujących sąsiadujące węzły.</returns>
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
	/// <summary>
	/// Sprawdza, czy na ścieżce znajdują się przeszkody.
	/// </summary>
	/// <param name="currentPos">Aktualna pozycja.</param>
	/// <param name="direction">Kierunek sprawdzenia.</param>
	/// <param name="stepOffset">Offset kroku.</param>
	/// <param name="clearanceDistance">Docelowa odległość od sciany.</param>
	/// <param name="hitInfo">Informacje o trafieniu promienia.</param>
	/// <returns>True, jeśli nie ma przeszkód; w przeciwnym razie false.</returns>
	private static bool CheckForObstacles(Vector3 currentPos, Vector3 direction, float stepOffset, float clearanceDistance, out RayCastHit hitInfo)
	{
		TagsContainer tags = new TagsContainer();
		tags.Add("map");

		// Perform a raycast with clearance distance added to the step offset
		bool hit = Physics.RayCast_Triangle(currentPos, direction, stepOffset + clearanceDistance, out hitInfo, tags);
		return !hit;
	}
}