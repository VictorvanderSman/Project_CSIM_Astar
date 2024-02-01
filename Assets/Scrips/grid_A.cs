using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grid_A : MonoBehaviour
{
    Node[,] grid;
    public bool DisplayGridDisplay;
    public Transform player;
    public int gridWorldSize;
    public float nodeRadius;
    public LayerMask unwalkableMask;

    float nodeDiameter;
    int GridSizeX, GridSizeY;

    private Vector2 position2;

    void Awake() 
    {
        nodeDiameter = nodeRadius *2;
        GridSizeX = Mathf.RoundToInt(gridWorldSize/nodeDiameter);
        GridSizeY = Mathf.RoundToInt(gridWorldSize/nodeDiameter);
        CreateGrid();
        
    }
    public int maxSize
    {
        get 
        {
            return GridSizeX * GridSizeY;
        }
        
    }
    public List<Node> Getneighbours(Node node){

        // create a list of the 8 neighbours of a node
        List<Node> neighbours = new List<Node>();

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
        grid = new Node[GridSizeX, GridSizeY];
        position2 = new Vector2(transform.position.x, transform.position.y);

        Vector2 worldbottomleft = position2 - Vector2.right * gridWorldSize/2 - Vector2.up * gridWorldSize/2;
        
        
        
        for (int x =0; x < GridSizeX; x ++)
        {
            for (int y =0; y < GridSizeX; y ++)
            {
                Vector2 worldPoint = worldbottomleft + Vector2.right *(x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter +nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);

            }

        }
    }
    public Node NodeFromWorldPoint(Vector2 worldPosition)
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
