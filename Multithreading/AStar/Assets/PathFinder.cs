﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class Pathfinder : MonoBehaviour
{

    public struct Node
    {
        public int2 coord;
        public int2 parent;
        public int gScore;
        public int hScore;
    }

    Hashtable obstacles;
    Node start, end;
    int safeGuard = 10000;

    public Tilemap map;
    public Tile defaultTile;
    public Camera cam;


    // Start is called before the first frame update
    void Start()
    {
        obstacles = new Hashtable();
        start = new Node { coord = int2.zero, parent = int2.zero, gScore = int.MaxValue, hScore = int.MaxValue };
        end = new Node { coord = int2.zero, parent = int2.zero, gScore = int.MaxValue, hScore = int.MaxValue };

        PlaceCube();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            PlaceStart();
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            PlaceEnd();
        }

        if (Input.GetMouseButtonDown(0) &&
            !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl))
        {
            PlaceObstacle();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearTiles();

            float startTime = Time.realtimeSinceStartup;

            FindPath();

            float endTime = Time.realtimeSinceStartup;
            Debug.Log(endTime - startTime);
        }

    }

    void PlaceCube()
    {
        int2 coord2;

        for (int i = -2500; i < 2500; i++)
        {
            coord2.x = 2500; coord2.y = i;

            obstacles.Add(coord2, true);

            // Convert int2 to Vector3Int
            UnityEngine.Vector3Int myVector3Int = new UnityEngine.Vector3Int(coord2.x, coord2.y, 0);


            map.SetTile(myVector3Int, defaultTile);
            map.SetTileFlags(myVector3Int, TileFlags.None);
            map.SetColor(myVector3Int, Color.black);
        }

        for (int i = -2500; i < 2500; i++)
        {
            coord2.x = -2500; coord2.y = i;

            obstacles.Add(coord2, true);

            // Convert int2 to Vector3Int
            UnityEngine.Vector3Int myVector3Int = new UnityEngine.Vector3Int(coord2.x, coord2.y, 0);


            map.SetTile(myVector3Int, defaultTile);
            map.SetTileFlags(myVector3Int, TileFlags.None);
            map.SetColor(myVector3Int, Color.black);
        }

        for (int i = -2500; i <= 2500; i++)
        {
            coord2.x = i; coord2.y = 2500;

            obstacles.Add(coord2, true);

            // Convert int2 to Vector3Int
            UnityEngine.Vector3Int myVector3Int = new UnityEngine.Vector3Int(coord2.x, coord2.y, 0);


            map.SetTile(myVector3Int, defaultTile);
            map.SetTileFlags(myVector3Int, TileFlags.None);
            map.SetColor(myVector3Int, Color.black);
        }

        for (int i = -2499; i < 2500; i++)
        {
            coord2.x = i; coord2.y = -2500;

            obstacles.Add(coord2, true);

            // Convert int2 to Vector3Int
            UnityEngine.Vector3Int myVector3Int = new UnityEngine.Vector3Int(coord2.x, coord2.y, 0);


            map.SetTile(myVector3Int, defaultTile);
            map.SetTileFlags(myVector3Int, TileFlags.None);
            map.SetColor(myVector3Int, Color.black);
        }
    }

    void ClearTiles()
    {
        map.ClearAllTiles();

        Vector3Int _start = new Vector3Int(start.coord.x, start.coord.y, 0);
        map.SetTile(_start, defaultTile);
        map.SetTileFlags(_start, TileFlags.None);
        map.SetColor(_start, Color.green);

        Vector3Int _end = new Vector3Int(end.coord.x, end.coord.y, 0);
        map.SetTile(_end, defaultTile);
        map.SetTileFlags(_end, TileFlags.None);
        map.SetColor(_end, Color.red);

        foreach (int2 o in obstacles.Keys)
        {
            Vector3Int obstacle = new Vector3Int(o.x, o.y, 0);
            map.SetTile(obstacle, defaultTile);
            map.SetTileFlags(obstacle, TileFlags.None);
            map.SetColor(obstacle, Color.black);
        }
    }

    void PlaceStart()
    {
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouseCell = map.WorldToCell(mouseWorldPos);
        int2 coord = new int2 { x = mouseCell.x, y = mouseCell.y };

        if (!obstacles.ContainsKey(coord) && !coord.Equals(end.coord))
        {
            map.SetTile(new Vector3Int(start.coord.x, start.coord.y, 0), null);

            start.coord = coord;
            map.SetTile(mouseCell, defaultTile);
            map.SetTileFlags(mouseCell, TileFlags.None);
            map.SetColor(mouseCell, Color.green);
        }
    }

    void PlaceEnd()
    {
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouseCell = map.WorldToCell(mouseWorldPos);
        int2 coord = new int2 { x = mouseCell.x, y = mouseCell.y };

        if (!obstacles.ContainsKey(coord) && !coord.Equals(start.coord))
        {
            map.SetTile(new Vector3Int(end.coord.x, end.coord.y, 0), null);

            end.coord = coord;
            map.SetTile(mouseCell, defaultTile);
            map.SetTileFlags(mouseCell, TileFlags.None);
            map.SetColor(mouseCell, Color.red);
        }

    }

    void PlaceObstacle()
    {
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouseCell = map.WorldToCell(mouseWorldPos);
        int2 coord = new int2 { x = mouseCell.x, y = mouseCell.y };

        if (obstacles.ContainsKey(coord))
        {
            map.SetTile(new Vector3Int(coord.x, coord.y, 0), null);
            obstacles.Remove(coord);
        }
        else if (!coord.Equals(start.coord) && !coord.Equals(end.coord))
        {
            obstacles.Add(coord, true);
            map.SetTile(mouseCell, defaultTile);
            map.SetTileFlags(mouseCell, TileFlags.None);
            map.SetColor(mouseCell, Color.black);
        }
    }

    public void FindPath()
    {
        NativeHashMap<int2, bool> isObstacle =
            new NativeHashMap<int2, bool>(obstacles.Count, Allocator.TempJob);
        NativeHashMap<int2, Node> nodes =
            new NativeHashMap<int2, Node>(safeGuard, Allocator.TempJob);
        NativeHashMap<int2, Node> openSet =
            new NativeHashMap<int2, Node>(safeGuard, Allocator.TempJob);
        NativeArray<int2> offsets = new NativeArray<int2>(8, Allocator.TempJob);

        foreach (int2 o in obstacles.Keys)
        {
            isObstacle.Add(o, true);
        }

        AStar aStar = new AStar
        {
            isObstacle = isObstacle,
            offsets = offsets,
            nodes = nodes,
            openSet = openSet,
            start = start,
            end = end,
            safeGuard = safeGuard
        };

        JobHandle handle = aStar.Schedule();
        handle.Complete();

        NativeArray<Node> nodeArray = nodes.GetValueArray(Allocator.TempJob);

        for (int i = 0; i < nodeArray.Length; i++)
        {
            Vector3Int currentNode = new Vector3Int(nodeArray[i].coord.x,
                nodeArray[i].coord.y, 0);

            if (!start.coord.Equals(nodeArray[i].coord) &&
                !end.coord.Equals(nodeArray[i].coord) &&
                !obstacles.ContainsKey(nodeArray[i].coord))
            {
                map.SetTile(currentNode, defaultTile);
                map.SetTileFlags(currentNode, TileFlags.None);
                map.SetColor(currentNode, Color.white);
            }
        }

        if (nodes.ContainsKey(end.coord))
        {
            int2 currentCoord = end.coord;

            while (!currentCoord.Equals(start.coord))
            {
                currentCoord = nodes[currentCoord].parent;
                Vector3Int currentTile = new Vector3Int(currentCoord.x,
                    currentCoord.y, 0);

                map.SetTile(currentTile, defaultTile);
                map.SetTileFlags(currentTile, TileFlags.None);
                map.SetColor(currentTile, Color.green);
            }
        }

        nodes.Dispose();
        openSet.Dispose();
        isObstacle.Dispose();
        offsets.Dispose();
        nodeArray.Dispose();
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct AStar : IJob
    {
        public NativeHashMap<int2, bool> isObstacle;
        public NativeHashMap<int2, Node> nodes;
        public NativeHashMap<int2, Node> openSet;
        public NativeArray<int2> offsets;

        public Node start;
        public Node end;

        public int safeGuard;

        public void Execute()
        {
            Node current = start;
            current.gScore = 0;
            current.hScore = SquaredDistance(current.coord, end.coord);
            openSet.TryAdd(current.coord, current);

            offsets[0] = new int2(0, 1);
            offsets[1] = new int2(1, 1);
            offsets[2] = new int2(1, 0);
            offsets[3] = new int2(1, -1);
            offsets[4] = new int2(0, -1);
            offsets[5] = new int2(-1, -1);
            offsets[6] = new int2(-1, 0);
            offsets[7] = new int2(-1, 1);

            int counter = 0;

            do
            {
                current = openSet[ClosestNode()];
                nodes.TryAdd(current.coord, current);

                for (int i = 0; i < offsets.Length; i++)
                {
                    if (!nodes.ContainsKey(current.coord + offsets[i]) &&
                        !isObstacle.ContainsKey(current.coord + offsets[i]))
                    {
                        Node neighbour = new Node
                        {
                            coord = current.coord + offsets[i],
                            parent = current.coord,
                            gScore = current.gScore +
                                SquaredDistance(current.coord, current.coord + offsets[i]),
                            hScore = SquaredDistance(current.coord + offsets[i], end.coord)
                        };

                        if (openSet.ContainsKey(neighbour.coord) && neighbour.gScore <
                            openSet[neighbour.coord].gScore)
                        {
                            openSet[neighbour.coord] = neighbour;
                        }
                        else if (!openSet.ContainsKey(neighbour.coord))
                        {
                            openSet.TryAdd(neighbour.coord, neighbour);
                        }
                    }
                }

                openSet.Remove(current.coord);
                counter++;

                if (counter > safeGuard)
                    break;

            } while (openSet.Count() != 0 && !current.coord.Equals(end.coord));
        }

        public int SquaredDistance(int2 coordA, int2 coordB)
        {
            int a = coordB.x - coordA.x;
            int b = coordB.y - coordA.y;
            return a * a + b * b;
        }

        public int2 ClosestNode()
        {
            Node result = new Node();
            int fScore = int.MaxValue;

            NativeArray<Node> nodeArray = openSet.GetValueArray(Allocator.Temp);

            for (int i = 0; i < nodeArray.Length; i++)
            {
                if (nodeArray[i].gScore + nodeArray[i].hScore < fScore)
                {
                    result = nodeArray[i];
                    fScore = nodeArray[i].gScore + nodeArray[i].hScore;
                }
            }

            nodeArray.Dispose();
            return result.coord;
        }
    }
}