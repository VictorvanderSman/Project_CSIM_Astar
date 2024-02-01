using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

public class PathfindingManager : MonoBehaviour {

	public Queue <Pathresult> results = new Queue<Pathresult>();

	static PathfindingManager instance;
	Pathfinding pathfinding;

	public GameObject[] targets;

	Grid_locations grid_loc;

	bool already_found_path = false;

	public Vector2[] found_path = new Vector2[0];

	grid_A grid_;
	public bool use_lists = false;

	public bool using_piggy_backing = false;

	public int paths_calculated;
	void Awake() {
		
		paths_calculated = 0;

		instance = this;
		grid_loc = GetComponent<Grid_locations>();

		grid_ = GetComponent<grid_A>();

		pathfinding = GetComponent<Pathfinding>();
		
	}
	void Update()
	{
		if (results.Count > 0 )
		{
			int itemsInQueue = results.Count;
			lock(results)
			{
				for (int i = 0; i < itemsInQueue; i++)
				{
					Pathresult result = results.Dequeue();
					result.callback(result.path, result.success);
				}
			}
		}
	}

	public static void RequestPath(PathRequest request, Node_loc node) 
	{
		if (instance.use_lists)
		{
			instance.pathfinding.Pathfinder(request, instance.FinishedProcessingPath, instance.FinishedProcessingPath2, instance.found_path);
		}

		List<Node_loc> neighbours = instance.grid_loc.Getneighbours(node);
		instance.already_found_path = false;

		foreach (Node_loc neighbour in neighbours)
		

        {
			if (!instance.using_piggy_backing)
			{
				break;
			}
			// for each neighboring cell of the loc grid, check if there is a path going to your goal
			foreach(Vector2[] path in neighbour.units)
			{
				//Debug.Log("we found a path to compare");
				//Debug.Log("Paths end: " + path.Last().ToString());
				//Debug.Log("What we were looking for: " + request.pathEnd.ToString());
				//Debug.Log("Distance: " + Vector2.Distance(path.Last(), request.pathEnd));

				// If this is where I want to go, go to the first waypoint, unless the second waypoint is closer 
				if (1 > Vector2.Distance(path.Last(), request.pathEnd))
				{
					// Debug.Log("We found someone to piggyback off! ------------------------------------------");
					// Instead move to the start of the already found path 
					request.pathEnd = path[0];
					//Debug.Log("The point we're going to first: " + path[0].ToString());
					//Debug.Log("Paths end: " + path.Last().ToString());

					instance.pathfinding.FindPath(request, instance.FinishedProcessingPath, instance.FinishedProcessingPath2, path);
					instance.already_found_path = true;
					break;
				}
				if (instance.already_found_path)
				{
					break;
				}
			}
		}


		ThreadStart threadStart = delegate {
			instance.pathfinding.FindPath(request, instance.FinishedProcessingPath, instance.FinishedProcessingPath2, instance.found_path);

		};
		threadStart.Invoke();	
	}

	

	public void FinishedProcessingPath(Pathresult result) {
		
		lock (results){
		results.Enqueue(result);
		}
		
	}

	public void FinishedProcessingPath2(Pathresult result, Vector2[] found_path) {
		

		List<Vector2> path_to_path = result.path.Cast<Vector2>().ToList();
		List<Vector2> path_to_goal = found_path.Cast<Vector2>().ToList();

		path_to_path.InsertRange((result.path.Count() - 1), path_to_goal);
		
		
		result.path = path_to_path.ToArray();

		lock (results){
		results.Enqueue(result);
		}
	}

	
	
}
public struct PathRequest {
		public Vector2 pathStart;
		public Vector2 pathEnd;
		public Action<Vector2[], bool> callback;

		public PathRequest(Vector2 _start, Vector2 _end, Action<Vector2[], bool> _callback) {
			pathStart = _start;
			pathEnd = _end;
			callback = _callback;
		}

	}
public struct Pathresult
	{
		public Vector2[] path;
		public bool success;
		public Action<Vector2[], bool> callback;

		public Pathresult(Vector2[] _path, bool _succes, Action<Vector2[], bool> _callback) {
			path = _path;
			success = _succes;
			callback = _callback;
		}
		
	}