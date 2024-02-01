using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{
    grid_A grid;
    PathfindingManager requestmanager;

    public List<double> time_spend = new List<double>();

    public float HeuristicMod = 1f;


    
    void Awake()
    {
        grid = GetComponent<grid_A>();
    } 

    

   public void FindPath(PathRequest request, Action<Pathresult> callback, Action<Pathresult, Vector2[]> callback2, Vector2[] Foundpath)
   {
        // timer 
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector2[] waypoints = new Vector2[0];
        bool pathSucces = false;


        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);

        if(startNode.walkable && targetNode.walkable){

            Heap<Node> openSet = new Heap<Node>(grid.maxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while(openSet.Count > 0) 
            {
                Node currentNode = openSet.RemoveFirst();
                
                closedSet.Add(currentNode);

                // check if the current node is the target
                if (currentNode == targetNode)
                {
                    sw.Stop();
                    time_spend.Add(sw.ElapsedMilliseconds);
                    pathSucces = true;
                    break;
                }


                // loop over all neighbours to evaluate
                foreach (Node neighbour in grid.Getneighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }
                    
                    int newMovementCostToNeighbour = currentNode.Gcost + Getdistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.Gcost || !openSet.Contains(neighbour))
                    {

                        neighbour.Gcost = newMovementCostToNeighbour;
                        neighbour.Hcost = Convert.ToInt32(Math.Pow((Getdistance(neighbour, targetNode)), HeuristicMod));
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else
                            openSet.UpdateItem(neighbour);

                    }
                }
            }
             
            if (pathSucces)
            {
                waypoints = retracePath(startNode, targetNode);
                pathSucces = waypoints.Length > 0;
                
            }
            if (Foundpath.Length > 0)
            {
                callback2(new Pathresult(waypoints, pathSucces, request.callback), Foundpath);
            }
            else
            {
                callback(new Pathresult(waypoints, pathSucces, request.callback));
            }
        }
    }
    
    Vector2[] retracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;

        }
        path.Add(current);
        
        Vector2[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);


        return waypoints;

    }

    Vector2[] SimplifyPath(List<Node> path)
    {            
            List<Vector2> waypoints = new List<Vector2>();

            if (path.Count < 2)
            {
                waypoints.Add(path[0].worldPosition);
                return waypoints.ToArray();
            }


            Vector2 directionOld = new Vector2(path[0].gridX - path[1].gridX,path[0].gridY - path[1].gridY);
            waypoints.Add(path[0].worldPosition);

            for(int i = 1; i < path.Count; i++)
            {
                Vector2  directionNew = new Vector2(path[i-1].gridX - path[i].gridX,path[i-1].gridY - path[i].gridY);
                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i].worldPosition);
                }
                directionOld = directionNew;

            }
        return waypoints.ToArray();    
    }

    int Getdistance(Node A, Node B)
    {
        int dstX = Mathf.Abs(A.gridX - B.gridX);
        int dstY = Mathf.Abs(A.gridY - B.gridY);
        
        // return the distance by adding the straight and angled distance
        if (dstX > dstY){
            return 14*dstY + 10 * (dstX - dstY);
        }
        return 14*dstX + 10 * (dstY - dstX);
    }  

    public void Pathfinder(PathRequest request, Action<Pathresult> callback, Action<Pathresult, Vector2[]> callback2, Vector2[] Foundpath) {

        // timer 
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector2[] waypoints = new Vector2[0];
        bool pathSucces = false;


        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);
		

		List<Node> openSet = new List<Node>();
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while (openSet.Count > 0) {
			Node node = openSet[0];
			for (int i = 1; i < openSet.Count; i ++) {
				if (openSet[i].Fcost < node.Fcost || openSet[i].Fcost == node.Fcost) {
					if (openSet[i].Hcost < node.Hcost)
						node = openSet[i];
				}
			}

			openSet.Remove(node);
			closedSet.Add(node);

			if (node == targetNode) {
                
                sw.Stop();
                time_spend.Add(sw.ElapsedMilliseconds);
				
                pathSucces = true;
                
				return;
			}

			foreach (Node neighbour in grid.Getneighbours(node)) {
				if (!neighbour.walkable || closedSet.Contains(neighbour)) {
					continue;
				}

				int newMovementCostToNeighbour = node.Gcost + Getdistance(node, neighbour);
				if (newMovementCostToNeighbour < neighbour.Gcost || !openSet.Contains(neighbour)) {

					neighbour.Gcost = newMovementCostToNeighbour;
                    neighbour.Hcost = Convert.ToInt32(Math.Pow((Getdistance(neighbour, targetNode)), HeuristicMod));
					neighbour.parent = node;

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);

                        
				}
			}
		

        if (pathSucces)
            {
                waypoints = PathRetracer(startNode, targetNode);
                pathSucces = waypoints.Length > 0;
                
            }
            if (Foundpath.Length > 0)
            {
                callback2(new Pathresult(waypoints, pathSucces, request.callback), Foundpath);
            }
            else
            {
                callback(new Pathresult(waypoints, pathSucces, request.callback));
            }

        }
	}

	Vector2[] PathRetracer(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

		Vector2[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;

	}


}



