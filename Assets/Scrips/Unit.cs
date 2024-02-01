using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {


	public Transform target;
	public GameObject[] targets;

	public float speed = 20;
	
	Vector2[] path;
	int targetIndex;
	public int PreviousOption = 999;

	public Node_loc previous_loc;

	Grid_locations grid_loc;

	GameObject manager;
	
	public PathfindingManager thingy;

	void Awake() {

		// Add yourself to a list kept by locations
		
		manager = GameObject.Find("A*");
		grid_loc = manager.GetComponent<Grid_locations>();
		thingy = manager.GetComponent<PathfindingManager>();

		grid_loc.all_units.Add(this);
		
		previous_loc = grid_loc.NodeFromWorldPoint(transform.position);

		targets = GameObject.FindGameObjectsWithTag("Targets");
		target = FindTarget();
		PathfindingManager.RequestPath(new PathRequest(transform.position,target.position, OnPathFound), previous_loc);
		
	}



	public void update_location()
	{
		// Where am I
		Node_loc new_location = grid_loc.NodeFromWorldPoint(transform.position);
		// Debug.Log("Updated location to add " + path[targetIndex].ToString() + "to " + new_location.worldPosition.ToString());
		// Update my location in grid 
		new_location.units.Add(path);
	}

	
	

	public void OnPathFound(Vector2[] newPath, bool pathSuccessful) {
		if (pathSuccessful) 
		{	
			
			
			path = newPath;
			targetIndex = 0;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	public Transform FindTarget()
	{
		int RandomOption = Random.Range(0, targets.Length);
		
		while(RandomOption == PreviousOption)
			RandomOption = Random.Range(0, targets.Length);
		
		PreviousOption = RandomOption;
		
		return targets[RandomOption].transform;
	}

	IEnumerator FollowPath() {

		Vector3 currentWaypoint = path[0];

		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex ++;
				if (targetIndex >= path.Length) {

					// at the end of the path, find a new target
					target = FindTarget();
					thingy.paths_calculated++;
					PathfindingManager.RequestPath(new PathRequest(transform.position,target.position, OnPathFound), previous_loc);
					
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}

			transform.position = Vector2.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
			
			yield return null;

		}
	}

	public void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector2.one);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}

	
}
