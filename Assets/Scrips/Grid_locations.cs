using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid_locations : MonoBehaviour
{
    Node_loc[,] grid;
    public int gridWorldSize;
    public float nodeRadius;
    public LayerMask unwalkableMask;

	float timePassed = 0f;

    float nodeDiameter;

    public List<Unit> all_units = new List<Unit>();

    int GridSizeX, GridSizeY;

    private Vector2 position2;

    PathfindingManager manager;


    grid_A grid_script;

    void Awake() 
    {

        grid_script = GetComponent<grid_A>();
        manager = GetComponent<PathfindingManager>();

        nodeDiameter = nodeRadius * 2;

        GridSizeX = Mathf.RoundToInt(gridWorldSize/nodeDiameter);
        GridSizeY = Mathf.RoundToInt(gridWorldSize/nodeDiameter);
        CreateGrid();
        
    }

    void Update()
    {
        timePassed += Time.deltaTime;
        if(timePassed > 1f && manager.using_piggy_backing)
        {
			clean();
            ask_all_units();
            timePassed = 0f;
        }
    }
    public void ask_all_units()
    {
        // ask all units to update their path in the thing
        foreach(Unit unit in all_units)
        {
            unit.update_location();
        }
    }

    public int maxSize
    {
        get 
        {
            return GridSizeX * GridSizeY;
        }
        
    }

    public List<Node_loc> Getneighbours(Node_loc node){

        // create a list of the 8 neighbours of a node
        List<Node_loc> neighbours = new List<Node_loc>();

        // loop over all neighbours but exempt yourself
        for (int x = -1; x <= 1; x++){
            for(int y = -1; y <=1; y++){
                if (y == 0 && x == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                
                // check if the point is not on the edge
                if (checkX >= 0 && checkX < GridSizeX && checkY >= 0 && checkY < GridSizeY)
                    {
                        neighbours.Add(grid[checkX, checkY]);
                    }
            }
        }
        return neighbours;
    }
    void CreateGrid()
    {
        grid = new Node_loc[GridSizeX, GridSizeY];
        position2 = new Vector2(transform.position.x, transform.position.y);

        Vector2 worldbottomleft = position2 - Vector2.right * gridWorldSize/2 - Vector2.up * gridWorldSize/2;
        
        for (int x =0; x < GridSizeX; x ++)
        {
            for (int y =0; y < GridSizeX; y ++)
            {
                Vector2 worldPoint = worldbottomleft + Vector2.right *(x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter +nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node_loc(worldPoint, x, y, new List<Vector2[]>());

            }

        }
    }
    void clean()
    {
        for (int x =0; x < GridSizeX; x ++)
        {
            for (int y =0; y < GridSizeX; y ++)
            {
                grid[x, y] = new Node_loc(grid[x,y].worldPosition, x, y, new List<Vector2[]>());
            }
        }
    }
    public Node_loc NodeFromWorldPoint(Vector2 worldPosition)
    {
        // function to return the grid coordinates from real world positions 
        float percentX = (worldPosition.x + gridWorldSize/2) / gridWorldSize;
        float percentY = (worldPosition.y + gridWorldSize/2) / gridWorldSize;


        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((GridSizeX -  1 ) * percentX);
        int y = Mathf.RoundToInt((GridSizeX -  1 ) * percentY);

        return grid[x,y];



    }

    
}

public struct Node_loc
    {   
        public Vector2 worldPosition;

        public int gridX;

        public int gridY;

        public List<Vector2[]> units;



        public Node_loc(Vector2 _worldPos, int _gridX, int _gridY, List<Vector2[]> _units)
        {
            worldPosition = _worldPos;
            gridX = _gridX;
            gridY = _gridY;
            units = _units;

        }

    }
