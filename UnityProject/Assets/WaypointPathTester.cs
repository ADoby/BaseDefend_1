using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointPathTester : MonoBehaviour 
{

    public List<Vector3> Path = new List<Vector3>();
    public PathfinderType PathType = PathfinderType.GridBased;

    public Transform start, end;

    public bool hasPath = false;

    public void FindPath(Vector3 startPosition, Vector3 endPosition)
    {
        if (PathType == PathfinderType.GridBased)
        {
            Pathfinder.Instance.InsertInQueue(startPosition, endPosition, SetList);
        }
        else if (PathType == PathfinderType.WaypointBased)
        {
            WaypointPathfinder.Instance.InsertInQueue(startPosition, endPosition, SetList);
        }
    }

    public Timer pathUpdateTimer;

	// Update is called once per frame
	void Update () 
    {
        if (pathUpdateTimer.Update())
        {
            pathUpdateTimer.Reset();
            Repath();
        }
        if (hasPath && Path.Count > 0)
        {
            Vector3 lastPos = start.position;
            foreach (var pos in Path)
            {
                Debug.DrawLine(lastPos, pos);
                lastPos = pos;
            }
        }
	}

    public void Repath()
    {
        FindPath(start.position, end.position);
    }


    protected virtual void SetList(List<Vector3> path)
    {
        if (path == null)
        {
            return;
        }
        Path.Clear();
        Path = path;
        if (Path.Count > 0)
        {
            hasPath = true;
            //Path[0] = new Vector3(Path[0].x, Path[0].y - 1, Path[0].z);
            //Path[Path.Count - 1] = new Vector3(Path[Path.Count - 1].x, Path[Path.Count - 1].y - 1, Path[Path.Count - 1].z);
        }
        else
        {
            hasPath = false;
        }
    }
}
