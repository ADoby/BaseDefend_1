using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class Pathfinder : MonoBehaviour 
{
    //Singleton
    private static Pathfinder instance;
    public static Pathfinder Instance { get { return instance; } private set {} }

    //Variables
    [SerializeField]
    [HideInInspector]
    private Node[] Map = null;
    [SerializeField]
    [HideInInspector]
    private int mapWidth = 0;
    [SerializeField]
    [HideInInspector]
    private int mapHeight = 0;

    public float Tilesize = 1;
    public float MaxFalldownHeight;
    public float ClimbLimit;
    public int HeuristicAggression;
    public float HighestPoint = 100F;
    public float LowestPoint = -50F;

    public bool LocalCoordinates = false;

    public Vector2 MapStartPosition;
    public Vector2 MapEndPosition;

    public List<string> DisallowedTags;
    public List<string> IgnoreTags;
    public bool MoveDiagonal = true;

    public bool DrawMapInEditor = false;
    public bool CheckFullTileSize = false;

    //FPS
    private float updateinterval = 1F;
    private int frames = 0;
    private float timeleft = 1F;
    private int FPS = 60;
    private int times = 0;
    private int averageFPS = 0;

    int maxSearchRounds = 0;

    //Queue path finding to not bottleneck it
    private List<QueuePath> queue = new List<QueuePath>();

    //Set singleton!
    void Awake()
    {
        instance = this;
    }

    public bool CreateMapOnStart = false;
	
	void Start () 
    {
        if (Tilesize <= 0)
        {
            Tilesize = 1;
        }

        if(CreateMapOnStart) Pathfinder.Instance.CreateMap();

        //if(Map != null) UnityEngine.Debug.Log("Map generated " + Map.Length);
	}

    [ContextMenu("TestMap")]
    public void TestMap()
    {
        if (Map != null)
            UnityEngine.Debug.Log("Map generated " + Map.Length);
        else
            UnityEngine.Debug.Log("Map fail");
    }

    float overalltimer = 0;
    int iterations = 0;
    //Go through one 
	void Update () 
    {
        timeleft -= Time.deltaTime;
        frames++;

        if (timeleft <= 0F)
        {
            FPS = frames;
            averageFPS += frames;
            times++;
            timeleft = updateinterval;
            frames = 0;
        }
        
        float timer = 0F;
        float maxtime = 1000/FPS;
        //Bottleneck prevention
        while(queue.Count > 0 && timer < maxtime)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            StartCoroutine(PathHandler(queue[0].startPos, queue[0].endPos, queue[0].storeRef));
            //queue[0].storeRef.Invoke(FindPath(queue[0].startPos, queue[0].endPos));
            queue.RemoveAt(0);
            sw.Stop();
            //print("Timer: " + sw.ElapsedMilliseconds);
            timer += sw.ElapsedMilliseconds;
            overalltimer += sw.ElapsedMilliseconds;
            iterations++;
        }

        DrawMapLines();
	}

    #region map
    //-------------------------------------------------INSTANIATE MAP-----------------------------------------------//

    [ContextMenu("CreateMap")]
    private void CreateMap()
    {
        //Find positions for start and end of map
        int startX  = (int)MapStartPosition.x;
        int startZ  = (int)MapStartPosition.y;
        int endX    = (int)MapEndPosition.x;
        int endZ    = (int)MapEndPosition.y;

        //Find tile width and height
        int width = (int)((endX - startX) / Tilesize);
        int height = (int)((endZ - startZ) / Tilesize);

        //Set map up
        Map = new Node[width * height];
        mapWidth = width;
        mapHeight = height;
        int size = width* height;
        SetListsSize(size);

        //Fill up Map
        for (int yIndex = 0; yIndex < height; yIndex++)
        {
            for (int xIndex = 0; xIndex < width; xIndex++)
            {
                float x = startX + (xIndex * Tilesize) + (Tilesize / 2); //Position from where we raycast - X
                float y = startZ + (yIndex * Tilesize) + (Tilesize / 2); //Position from where we raycast - Z
                int ID = (yIndex * width) + xIndex; //ID we give to our Node!
                
                float dist = Mathf.Abs(HighestPoint) + Mathf.Abs(LowestPoint);
                RaycastHit[] hit;

                if (CheckFullTileSize)
                {
                    hit = Physics.SphereCastAll(new Vector3(x, HighestPoint, y), Tilesize / 2, Vector3.down, dist);
                }
                else
                {
                    hit = Physics.SphereCastAll(new Vector3(x, HighestPoint, y), Tilesize / 16, Vector3.down, dist);
                }
                bool free = true;
                float maxY = -Mathf.Infinity;
                
                foreach(RaycastHit h in hit)
                {
                    if (DisallowedTags.Contains(h.transform.tag))
                    {
                        if (h.point.y > maxY)
                        {
                            //It is a disallowed walking tile, make it false
                            Map[xIndex + yIndex * mapWidth] = new Node(xIndex, yIndex, 0, ID, x, y, false); //Non walkable tile!
                            free = false;
                            maxY = h.point.y;
                        }
                    }
                    else if(IgnoreTags.Contains(h.transform.tag))
                    {
                        //Do nothing we ignore these tags
                    }
                    else
                    {
                        if (h.point.y > maxY)
                        {
                            //It is allowed to walk on this tile, make it walkable!
                            Map[xIndex + yIndex * mapWidth] = new Node(xIndex, yIndex, h.point.y, ID, x, y, true); //walkable tile!
                            free = false;
                            maxY = h.point.y;
                        }
                    }
                }
                //We hit nothing set tile to false
                if (free == true)
                {
                    Map[xIndex + yIndex * mapWidth] = new Node(xIndex, yIndex, 0 ,ID, x, y, false);//Non walkable tile! 
                }        
            }
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEngine.Debug.Log("Map generated " + Map.Length);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        
    }

    #endregion //End map
    
    //---------------------------------------SETUP PATH QUEUE---------------------------------------//

    public void InsertInQueue(Vector3 startPos, Vector3 endPos, Action<List<Vector3>> listMethod)
    {
        QueuePath q = new QueuePath(startPos, endPos, listMethod);
        queue.Add(q);
    }

    #region astar
    //---------------------------------------FIND PATH: A*------------------------------------------//


    [SerializeField]
    [HideInInspector]
    private Node[] openList;

    [SerializeField]
    [HideInInspector]
    private Node[] closedList;

    private Node startNode;
    private Node endNode;
    private Node currentNode;
    //Use it with KEY: F-value, VALUE: ID. ID's might be looked up in open and closed list then
    private List<NodeSearch> sortedOpenList = new List<NodeSearch>();

    private void SetListsSize(int size)
    {
        openList   = new Node[size];
        closedList = new Node[size];
    }

    IEnumerator PathHandler(Vector3 startPos, Vector3 endPos, Action<List<Vector3>> listMethod)
    {
        yield return StartCoroutine(SinglePath(startPos, endPos, listMethod));
    }

    IEnumerator SinglePath(Vector3 startPos, Vector3 endPos, Action<List<Vector3>> listMethod)
    {
        FindPath(startPos, endPos, listMethod);
        yield return null;
    }

    public void FindPath(Vector3 startPos, Vector3 endPos, Action<List<Vector3>> listMethod)
    {
        //The list we returns when path is found
        List<Vector3> returnPath = new List<Vector3>();
        bool endPosValid = true;
        //Find start and end nodes, if we cant return null and stop!
        SetStartAndEndNode(startPos, endPos);

        if (startNode != null)
        {
            if (endNode == null)
            {
                endPosValid = false;
                FindEndNode(endPos);
                if (endNode == null)
                {
                    //still no end node - we leave and sends an empty list
                    maxSearchRounds = 0;
                    listMethod.Invoke(new List<Vector3>());
                    return;
                }
            }

            //Clear lists if they are filled
            Array.Clear(openList, 0, openList.Length); 
            Array.Clear(closedList, 0, openList.Length);
            if (sortedOpenList.Count > 0) { sortedOpenList.Clear(); }
            
            //Insert start node
            openList[startNode.ID] = startNode;
            //sortedOpenList.Add(new NodeSearch(startNode.ID, startNode.F));
            BHInsertNode(new NodeSearch(startNode.ID, startNode.F));

            bool endLoop = false;

            while (!endLoop)
            {
                //If we have no nodes on the open list AND we are not at the end, then we got stucked! return empty list then.
                if (sortedOpenList.Count == 0)
                {
                    print("Empty Openlist, closedList");
                    listMethod.Invoke(new List<Vector3>());
                    return;
                }

                //Get lowest node and insert it into the closed list
                int id = BHGetLowest();
                //sortedOpenList.Sort(sort);
                //int id = sortedOpenList[0].ID;
                currentNode = openList[id];
                closedList[currentNode.ID] = currentNode;
                openList[id] = null;
                //sortedOpenList.RemoveAt(0);

                if (currentNode.ID == endNode.ID)
                {
                    endLoop = true;
                    continue;
                }
                //Now look at neighbours, check for unwalkable tiles, bounderies, open and closed listed nodes.

                if (MoveDiagonal)
                {
                    NeighbourCheck();
                }
                else
                {
                    NonDiagonalNeighborCheck();
                }
            }


            Vector3 point;
            while (currentNode.parent != null)
            {
                point = new Vector3(currentNode.xCoord, currentNode.yCoord, currentNode.zCoord);
                if (LocalCoordinates)
                    point = transform.TransformPoint(point);
                returnPath.Add(point);
                currentNode = currentNode.parent;
            }

            //returnPath.Add(startPos);
            returnPath.Reverse();

            if (endPosValid)
            {
                //returnPath.Add(endPos);
            }

            if (returnPath.Count > 2 && endPosValid)
            {
                //Now make sure we do not go backwards or go to long
                if (Vector3.Distance(returnPath[returnPath.Count - 1], returnPath[returnPath.Count - 3]) < Vector3.Distance(returnPath[returnPath.Count - 3], returnPath[returnPath.Count - 2]))
                {
                    returnPath.RemoveAt(returnPath.Count - 2);
                }
                if (Vector3.Distance(returnPath[1], startPos) < Vector3.Distance(returnPath[0], returnPath[1]))
                {
                    returnPath.RemoveAt(0);
                }
            }
            maxSearchRounds = 0;
            listMethod.Invoke(returnPath);

        }
        else
        {
            maxSearchRounds = 0;
            listMethod.Invoke(new List<Vector3>());
        }
    }

    // Find start and end Node
    private void SetStartAndEndNode(Vector3 start, Vector3 end)
    {
        startNode = FindClosestNode(start);
        endNode = FindClosestNode(end);
    }

    public bool IsTheClosestNodeWalkable(Vector3 pos)
    {
        int x = (MapStartPosition.x < 0F) ? Mathf.FloorToInt(((pos.x + Mathf.Abs(MapStartPosition.x)) / Tilesize)) : Mathf.FloorToInt((pos.x - MapStartPosition.x) / Tilesize);
        int z = (MapStartPosition.y < 0F) ? Mathf.FloorToInt(((pos.z + Mathf.Abs(MapStartPosition.y)) / Tilesize)) : Mathf.FloorToInt((pos.z - MapStartPosition.y) / Tilesize);

        if (x < 0 || z < 0 || x > mapWidth || z > mapHeight)
            return false;

        Node n = Map[x + z * mapWidth];
        return n.walkable;
    }

    private Node FindClosestNode(Vector3 pos)
    {
        if (LocalCoordinates)
            pos = transform.InverseTransformPoint(pos);

        int x = (MapStartPosition.x < 0F) ? Mathf.FloorToInt(((pos.x + Mathf.Abs(MapStartPosition.x)) / Tilesize)) :  Mathf.FloorToInt((pos.x - MapStartPosition.x) / Tilesize);
        int z = (MapStartPosition.y < 0F) ? Mathf.FloorToInt(((pos.z + Mathf.Abs(MapStartPosition.y)) / Tilesize)) : Mathf.FloorToInt((pos.z - MapStartPosition.y) / Tilesize);

        if (x < 0 || z < 0 || x > mapWidth || z > mapHeight)
            return null;
        if ((x + z * mapWidth) > Map.Length)
            return null;

        Node n = Map[x + z * mapWidth];

        if (n.walkable)
        {
            return new Node(x, z, n.yCoord, n.ID, n.xCoord, n.zCoord, n.walkable);
        }
        else
        {
            //If we get a non walkable tile, then look around its neightbours
            for (int yIndex = z - 1; yIndex < z + 2; yIndex++)
            {
                for (int xIndex = x - 1; xIndex < x + 2; xIndex++)
                {
                    //Check they are within bounderies
                    if (yIndex > -1 && yIndex < mapHeight && xIndex > -1 && xIndex < mapWidth)
                    {
                        if (Map[xIndex + yIndex * mapWidth].walkable)
                        {
                            return new Node(xIndex, yIndex, Map[xIndex + yIndex * mapWidth].yCoord, Map[xIndex + yIndex * mapWidth].ID, Map[xIndex + yIndex * mapWidth].xCoord, Map[xIndex + yIndex * mapWidth].zCoord, Map[xIndex + yIndex * mapWidth].walkable);
                        }
                    }
                }
            }
            return null;
        }
    }

    private void FindEndNode(Vector3 pos)
    {
        if (LocalCoordinates)
            pos = transform.TransformPoint(pos);

        int x = (MapStartPosition.x < 0F) ? Mathf.FloorToInt(((pos.x + Mathf.Abs(MapStartPosition.x)) / Tilesize)) : Mathf.FloorToInt((pos.x - MapStartPosition.x) / Tilesize);
        int z = (MapStartPosition.y < 0F) ? Mathf.FloorToInt(((pos.z + Mathf.Abs(MapStartPosition.y)) / Tilesize)) : Mathf.FloorToInt((pos.z - MapStartPosition.y) / Tilesize);
        Node closestNode;
        try
        {
            closestNode = Map[x + z * mapWidth];
        }
        catch (Exception)
        {
            //UnityEngine.Debug.Log(string.Format("Error finding: {0} {1} => {2}", x, z, (x + z * mapWidth)));
            return;
        }

        List<Node> walkableNodes = new List<Node>();

        int turns = 1;

        while(walkableNodes.Count < 1 && maxSearchRounds < (int)10/Tilesize)
        {
            walkableNodes = EndNodeNeighbourCheck(x, z, turns);
            turns++;
            maxSearchRounds++;
        }

        if (walkableNodes.Count > 0) //If we found some walkable tiles we will then return the nearest
        {
            int lowestDist = 99999999;
            Node n = null;

            foreach (Node node in walkableNodes)
            {
                int i = GetHeuristics(closestNode, node);
                if (i < lowestDist)
                {
                    lowestDist = i;
                    n = node;
                }
            }
            endNode = new Node(n.x, n.y, n.yCoord, n.ID, n.xCoord, n.zCoord, n.walkable);
        }
    }

    private List<Node> EndNodeNeighbourCheck(int x, int z, int r)
    {      
        List<Node> nodes = new List<Node>();
        
        for (int i = z - r; i < z + r + 1; i++)
        {
            for (int j = x - r; j < x + r + 1; j++)
            {
                //Check that we are within bounderis, and goes in ring around our end pos
                if (i > -1 && j > -1 && i < mapWidth && j < mapHeight && ((i < z - r + 1 || i > z + r - 1) || (j < x - r + 1 || j > x + r - 1)))
                {
                    if (j + i * mapWidth > Map.Length)
                        continue;

                    //if it is walkable put it on the right list
                    if (Map[j + i * mapWidth].walkable)
                    {
                        nodes.Add(Map[j + i * mapWidth]);
                    }
                }
            }
        }

        return nodes;
    }

    private void NeighbourCheck()
    {
        int x = currentNode.x;
        int y = currentNode.y;

        for (int i = y - 1; i < y + 2; i++)
        {
            for (int j = x - 1; j < x + 2; j++)
            {
                //Check it is within the bounderies
                if (i > -1 && i < mapHeight && j > -1 && j < mapWidth)
                {
                    //Dont check for the current node.
                    if (i != y || j != x)
                    {
                        //Check the node is walkable
                        if (Map[j + i * mapWidth].walkable)
                        {
                            //We do not recheck anything on the closed list
                            if (!OnClosedList(Map[j + i * mapWidth].ID))
                            {
                                //Check if we can move up or jump down!
                                if ((Map[j + i  * mapWidth].yCoord - currentNode.yCoord < ClimbLimit && Map[j + i  * mapWidth].yCoord - currentNode.yCoord >= 0) || (currentNode.yCoord - Map[j + i  * mapWidth].yCoord < MaxFalldownHeight && currentNode.yCoord >= Map[j + i  * mapWidth].yCoord))
                                {
                                    //If it is not on the open list then add it to
                                    if (!OnOpenList(Map[j + i  * mapWidth].ID))
                                    {
                                        Node addNode = new Node(Map[j + i  * mapWidth].x, Map[j + i  * mapWidth].y, Map[j + i  * mapWidth].yCoord, Map[j + i  * mapWidth].ID, Map[j + i  * mapWidth].xCoord, Map[j + i  * mapWidth].zCoord, Map[j + i  * mapWidth].walkable, currentNode);
                                        addNode.H = GetHeuristics(Map[j + i  * mapWidth].x, Map[j + i  * mapWidth].y);
                                        addNode.G = GetMovementCost(x, y, j, i) + currentNode.G;
                                        addNode.F = addNode.H + addNode.G;
                                        //Insert on open list
                                        openList[addNode.ID] = addNode;
                                        //Insert on sorted list
                                        BHInsertNode(new NodeSearch(addNode.ID, addNode.F)); 
                                        //sortedOpenList.Add(new NodeSearch(addNode.ID, addNode.F));
                                    }
                                    else
                                    {
                                       ///If it is on openlist then check if the new paths movement cost is lower
                                        Node n = GetNodeFromOpenList(Map[j + i  * mapWidth].ID);
                                        if (currentNode.G + GetMovementCost(x, y, j, i) < openList[Map[j + i  * mapWidth].ID].G)
                                        {
                                            n.parent = currentNode;
                                            n.G = currentNode.G + GetMovementCost(x, y, j, i);
                                            n.F = n.G + n.H;
                                            BHSortNode(n.ID, n.F);
                                            //ChangeFValue(n.ID, n.F);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void NonDiagonalNeighborCheck()
    {            
        int x = currentNode.x;
        int y = currentNode.y;

        for (int i = y - 1; i < y + 2; i++)
        {
            for (int j = x - 1; j < x + 2; j++)
            {
                //Check it is within the bounderies
                if (i > -1 && i < mapHeight && j > -1 && j < mapWidth)
                {
                    //Dont check for the current node.
                    if (i != y || j != x)
                    {
                        //Check that we are not moving diagonal
                        if(GetMovementCost(x, y, j, i) < 14)
                        {
                            //Check the node is walkable
                            if (Map[j + i  * mapWidth].walkable)
                            {
                                //We do not recheck anything on the closed list
                                if (!OnClosedList(Map[j + i  * mapWidth].ID))
                                {
                                    //Check if we can move up or jump down!
                                    if ((Map[j + i  * mapWidth].yCoord - currentNode.yCoord < ClimbLimit && Map[j + i  * mapWidth].yCoord - currentNode.yCoord >= 0) || (currentNode.yCoord - Map[j + i  * mapWidth].yCoord < MaxFalldownHeight && currentNode.yCoord >= Map[j + i  * mapWidth].yCoord))
                                    {
                                        //If it is not on the open list then add it to
                                        if (!OnOpenList(Map[j + i  * mapWidth].ID))
                                        {
                                            Node addNode = new Node(Map[j + i  * mapWidth].x, Map[j + i  * mapWidth].y, Map[j + i  * mapWidth].yCoord, Map[j + i  * mapWidth].ID, Map[j + i  * mapWidth].xCoord, Map[j + i  * mapWidth].zCoord, Map[j + i  * mapWidth].walkable, currentNode);
                                            addNode.H = GetHeuristics(Map[j + i  * mapWidth].x, Map[j + i  * mapWidth].y);
                                            addNode.G = GetMovementCost(x, y, j, i) + currentNode.G;
                                            addNode.F = addNode.H + addNode.G;
                                            //Insert on open list
                                            openList[addNode.ID] = addNode;
                                            //Insert on sorted list
                                            BHInsertNode(new NodeSearch(addNode.ID, addNode.F));
                                            //sortedOpenList.Add(new NodeSearch(addNode.ID, addNode.F));
                                        }
                                        else
                                        {
                                            ///If it is on openlist then check if the new paths movement cost is lower
                                            Node n = GetNodeFromOpenList(Map[j + i  * mapWidth].ID);
                                            if (currentNode.G + GetMovementCost(x, y, j, i) < openList[Map[j + i  * mapWidth].ID].G)
                                            {
                                                n.parent = currentNode;
                                                n.G = currentNode.G + GetMovementCost(x, y, j, i);
                                                n.F = n.G + n.H;
                                                BHSortNode(n.ID, n.F);
                                                //ChangeFValue(n.ID, n.F);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void ChangeFValue(int id, int F)
    {
        foreach (NodeSearch ns in sortedOpenList)
        {
            if (ns.ID == id)
            {
                ns.F = F;
            }
        }
    }

    //Check if a Node is already on the openList
    private bool OnOpenList(int id)
    {
        return (openList[id] != null) ? true : false;
    }

    //Check if a Node is already on the closedList
    private bool OnClosedList(int id)
    {
        return (closedList[id] != null) ? true : false;
    }

    private int GetHeuristics(int x, int y)
    {
        //Make sure heuristic aggression is not less then 0!
        int HA = (HeuristicAggression < 0) ? 0 : HeuristicAggression;
        return (int)(Mathf.Abs(x - endNode.x) * (10F + (10F * HA))) + (int)(Mathf.Abs(y - endNode.y) * (10F + (10F * HA)));
    }

    private int GetHeuristics(Node a, Node b)
    {
        //Make sure heuristic aggression is not less then 0!
        int HA = (HeuristicAggression < 0) ? 0 : HeuristicAggression;
        return (int)(Mathf.Abs(a.x - b.x) * (10F + (10F * HA))) + (int)(Mathf.Abs(a.y - b.y) * (10F + (10F * HA)));
    }

    private int GetMovementCost(int x, int y, int j, int i)
    {
        //Moving straight or diagonal?
        return (x != j && y != i) ? 14 : 10;
    }

    private Node GetNodeFromOpenList(int id)
    {
        return (openList[id] != null) ? openList[id] : null;
    }

    #region Binary_Heap (min)

    private void BHInsertNode(NodeSearch ns)
    {
        //We use index 0 as the root!
        if (sortedOpenList.Count == 0)
        {
            sortedOpenList.Add(ns);
            openList[ns.ID].sortedIndex = 0;
            return;
        }

        sortedOpenList.Add(ns);
        bool canMoveFurther = true;
        int index = sortedOpenList.Count - 1;
        openList[ns.ID].sortedIndex = index;

        while (canMoveFurther)
        {
            int parent = Mathf.FloorToInt((index - 1) / 2);

            if (index == 0) //We are the root
            {
                canMoveFurther = false;
                openList[sortedOpenList[index].ID].sortedIndex = 0;
            }
            else
            {
                if (sortedOpenList[index].F < sortedOpenList[parent].F)
                {
                    NodeSearch s = sortedOpenList[parent];
                    sortedOpenList[parent] = sortedOpenList[index];
                    sortedOpenList[index] = s;

                    //Save sortedlist index's for faster look up
                    openList[sortedOpenList[index].ID].sortedIndex = index;
                    openList[sortedOpenList[parent].ID].sortedIndex = parent;

                    //Reset index to parent ID
                    index = parent;
                }
                else
                {
                    canMoveFurther = false;
                }
            }
        }
    }

    private void BHSortNode(int id, int F)
    {
        bool canMoveFurther = true;
        int index = openList[id].sortedIndex;
        sortedOpenList[index].F = F;

        while (canMoveFurther)
        {
            int parent = Mathf.FloorToInt((index - 1) / 2);

            if (index == 0) //We are the root
            {
                canMoveFurther = false;
                openList[sortedOpenList[index].ID].sortedIndex = 0;
            }
            else
            {
                if (sortedOpenList[index].F < sortedOpenList[parent].F)
                {
                    NodeSearch s = sortedOpenList[parent];
                    sortedOpenList[parent] = sortedOpenList[index];
                    sortedOpenList[index] = s;

                    //Save sortedlist index's for faster look up
                    openList[sortedOpenList[index].ID].sortedIndex = index;
                    openList[sortedOpenList[parent].ID].sortedIndex = parent;

                    //Reset index to parent ID
                    index = parent;
                }
                else
                {
                    canMoveFurther = false;
                }
            }
        }
    }

    private int BHGetLowest()
    {
       
        if (sortedOpenList.Count == 1) //Remember 0 is our root
        {
            int ID = sortedOpenList[0].ID;
            sortedOpenList.RemoveAt(0);           
            return ID;
        }
        else if (sortedOpenList.Count > 1)
        {
            //save lowest not, take our leaf as root, and remove it! Then switch through children to find right place.
            int ID = sortedOpenList[0].ID;
            sortedOpenList[0] = sortedOpenList[sortedOpenList.Count - 1];
            sortedOpenList.RemoveAt(sortedOpenList.Count - 1);
            openList[sortedOpenList[0].ID].sortedIndex = 0;

            int index = 0;
            bool canMoveFurther = true;
            //Sort the list before returning the ID
            while (canMoveFurther)
            {
                int child1 = (index * 2) + 1;
                int child2 = (index * 2) + 2;
                int switchIndex = -1;

                if (child1 < sortedOpenList.Count)
                {
                    switchIndex = child1;
                }
                else
                {
                    break;
                }
                if (child2 < sortedOpenList.Count)
                {
                    if (sortedOpenList[child2].F < sortedOpenList[child1].F)
                    {
                        switchIndex = child2;
                    }
                }
                if (sortedOpenList[index].F > sortedOpenList[switchIndex].F)
                {
                    NodeSearch ns = sortedOpenList[index];
                    sortedOpenList[index] = sortedOpenList[switchIndex];
                    sortedOpenList[switchIndex] = ns;

                    //Save sortedlist index's for faster look up
                    openList[sortedOpenList[index].ID].sortedIndex = index;
                    openList[sortedOpenList[switchIndex].ID].sortedIndex = switchIndex;

                    //Switch around idnex
                    index = switchIndex;
                }
                else
                {
                    break;
                }    
            }
            return ID;
          
        }
        else
        {
            return -1;
        }    
    }

    #endregion

    #endregion //End astar region!

    //---------------------------------------DRAW MAP IN EDITOR-------------------------------------//

    void OnDrawGizmosSelected()
    {
         //if (DrawMapInEditor == true && Map != null)
         //   {
         //       for (int i = 0; i < mapHeight; i++)
         //       {
         //           for (int j = 0; j < mapWidth; j++)
         //           {
         //               Gizmos.color = (Map[j + i  * mapWidth].walkable) ? new Color(0, 0.8F, 0, 0.5F) : new Color(0.8F, 0, 0, 0.5F);
         //               Gizmos.DrawCube(new Vector3(Map[j + i  * mapWidth].xCoord, Map[j + i  * mapWidth].yCoord + 0.1F, Map[j + i  * mapWidth].zCoord), new Vector3(Tilesize, 0.5F, Tilesize));
         //           }
         //       }
         //   }
    }

    void DrawMapLines()
    {
        if (DrawMapInEditor == true && Map != null)
        {
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (!Map[j + i  * mapWidth].walkable)
                        continue;
                    
                    for (int y = i - 1; y < i + 2; y++)
                    {
                        for (int x = j - 1; x < j + 2; x++)
                        {
                            if (y < 0 || x < 0 || y >= mapHeight || x >= mapWidth)
                                continue;

                            if(!Map[x + y  * mapWidth].walkable)
                                continue;

                            if (Map[j + i  * mapWidth].yCoord > Map[x + y  * mapWidth].yCoord && Mathf.Abs(Map[j + i  * mapWidth].yCoord - Map[x + y  * mapWidth].yCoord) > MaxFalldownHeight)
                                continue;

                            if (Map[j + i  * mapWidth].yCoord < Map[x + y  * mapWidth].yCoord && Mathf.Abs(Map[x + y  * mapWidth].yCoord - Map[j + i  * mapWidth].yCoord) > ClimbLimit)
                                continue;
                          
                            Vector3 start = new Vector3(Map[j + i  * mapWidth].xCoord, Map[j + i  * mapWidth].yCoord + 0.1f, Map[j + i  * mapWidth].zCoord);
                            Vector3 end = new Vector3(Map[x + y  * mapWidth].xCoord, Map[x + y  * mapWidth].yCoord + 0.1f, Map[x + y  * mapWidth].zCoord);

                            if (LocalCoordinates)
                            {
                                start = transform.TransformPoint(start);
                                end = transform.TransformPoint(end);
                            }
                            
                            UnityEngine.Debug.DrawLine(start, end, Color.green);
                        }
                    }
                }
            }
        }
    }

    #region DynamicSupport

    public void DynamicMapEdit(List<Vector3> checkList, Action<List<Vector2>> listMethod)
    {
        listMethod.Invoke(DynamicFindClosestNodes(checkList));
    }

    public void DynamicRedoMapEdit(List<Vector2> ids)
    {
        foreach (Vector2 v in ids)
        {
            Map[(int)v.x + (int)v.y * mapWidth].walkable = true;
        }
    }

    public void DynamicRaycastUpdate(Bounds b)
    {
        Vector3 startPos = new Vector3(b.min.x, 0, b.min.z);
        UnityEngine.Debug.Log("Startpos dyn: " + startPos);
        Node startNode = FindClosestNode(startPos);

        if (startNode == null)
            return;

        int xIterations = Mathf.CeilToInt(Mathf.Abs((b.max.x - b.min.x) / Tilesize)) + 2;
        int zIterations = Mathf.CeilToInt(Mathf.Abs((b.max.z - b.min.z) / Tilesize)) + 2;

        for (int x = startNode.x - 2; x < startNode.x + xIterations; x++)
        {
            for (int y = startNode.y - 2; y < startNode.y + zIterations; y++)
            {
                if (x >= 0 &&  y >= 0 && x < mapWidth && y < mapHeight)
                {

                    float dist = Mathf.Abs(HighestPoint) + Mathf.Abs(LowestPoint);
                    RaycastHit[] hit;

                    if (CheckFullTileSize)
                    {
                        hit = Physics.SphereCastAll(new Vector3(Map[x + y  * mapWidth].xCoord, HighestPoint, Map[x + y  * mapWidth].zCoord), Tilesize / 2, Vector3.down, dist);
                    }
                    else
                    {
                        hit = Physics.SphereCastAll(new Vector3(Map[x + y  * mapWidth].xCoord, HighestPoint, Map[x + y  * mapWidth].zCoord), Tilesize / 16, Vector3.down, dist);
                    }
                    
                    float maxY = -Mathf.Infinity;

                    foreach (RaycastHit h in hit)
                    {
                        if (DisallowedTags.Contains(h.transform.tag))
                        {
                            if (h.point.y > maxY)
                            {
                                //It is a disallowed walking tile, make it false
                                Map[x + y  * mapWidth].walkable = false;
                                Map[x + y  * mapWidth].yCoord = h.point.y;
                                maxY = h.point.y;
                            }
                        }
                        else if (IgnoreTags.Contains(h.transform.tag))
                        {
                            //Do nothing we ignore these tags
                        }
                        else
                        {
                            if (h.point.y > maxY)
                            {
                                //It is allowed to walk on this tile, make it walkable!
                                Map[x + y  * mapWidth].walkable = true;
                                Map[x + y  * mapWidth].yCoord = h.point.y;
                                maxY = h.point.y;
                            }
                        }
                    }
                }
            }
        }
    }

    private List<Vector2> DynamicFindClosestNodes(List<Vector3> vList)
    {      
        List<Vector2> returnList = new List<Vector2>();
        foreach (Vector3 pos in vList)
        {
            int x = (MapStartPosition.x < 0F) ? Mathf.FloorToInt(((pos.x + Mathf.Abs(MapStartPosition.x)) / Tilesize)) : Mathf.FloorToInt((pos.x - MapStartPosition.x) / Tilesize);
            int z = (MapStartPosition.y < 0F) ? Mathf.FloorToInt(((pos.z + Mathf.Abs(MapStartPosition.y)) / Tilesize)) : Mathf.FloorToInt((pos.z - MapStartPosition.y) / Tilesize);

            if (x >= 0 && x < mapWidth && z >= 0 && z < mapHeight)
            {
                if (Map[x + z  * mapWidth].walkable)
                {
                    Map[x + z  * mapWidth].walkable = false;
                    returnList.Add(new Vector2(x, z));
                }
            }
        }

        return returnList;
    }

    #endregion
}
