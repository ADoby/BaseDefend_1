using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Singleton Class for rewinding Rigidbodies
//It resets the transform and rigidbody to a position, rotation and its velocity
//back in time

public class Rewinder : MonoBehaviour {

    public bool addAllRigidbodyOnStart = true;

    public float saveStep = 0.2f;
    public int savedSteps = 20;

    private static Dictionary<Rigidbody, List<RewindInformation>> rewinders = new Dictionary<Rigidbody, List<RewindInformation>>();

    private int currentSavedSteps = 0;
    private bool enoughSteps = false;

    #region Instance

    private static Rewinder instance;

    void Awake()
    {
        instance = this;
    }

    public static Rewinder Instance
    {
        get 
        {
            if (!instance)
                instance = new Rewinder();
            return instance;
        }
    }

    #endregion

    // Use this for initialization
	void Start () {
        if (addAllRigidbodyOnStart)
        {
            foreach (var item in GameObject.FindObjectsOfType<Rigidbody>())
            {
                rewinders.Add(item, new List<RewindInformation>());
            }
        }


        StartCoroutine(UpdatePositions());
	}

    public static bool GoBackSecondsStatic(float time)
    {
        return Rewinder.Instance.GoBackSeconds(time);
    }

    public static bool GoBackStepsStatic(int steps)
    {
        return Rewinder.Instance.GoBackSteps(steps);
    }

    //Goes back in time: time / saveStep = steps
    public bool GoBackSeconds(float time)
    {
        int steps = Mathf.RoundToInt(time / saveStep);
        return GoBackSteps(steps);
    }

    public bool GoBackSteps(int steps)
    {
        if (steps > currentSavedSteps)
        {
            return false;
        }

        foreach (var item in rewinders)
        {
            item.Key.transform.position = item.Value[currentSavedSteps - steps].position;
            item.Key.transform.rotation = item.Value[currentSavedSteps - steps].rotation;
            item.Key.velocity = item.Value[currentSavedSteps - steps].velocity;
        }
        currentSavedSteps -= steps;
        enoughSteps = false;

        return true;
    }

    IEnumerator UpdatePositions()
    {
        if (!enoughSteps)
        {
            if (currentSavedSteps < savedSteps)
            {
                currentSavedSteps++;
            }
            else
            {
                enoughSteps = true;
            }
        }
        

        foreach (var item in rewinders)
        {
            if (enoughSteps)
            {
                item.Value.RemoveAt(0);
            }

            item.Value.Add(new RewindInformation(
                item.Key.transform.position, 
                item.Key.transform.rotation, 
                item.Key.velocity));
        }

        yield return new WaitForSeconds(saveStep);
        StartCoroutine(UpdatePositions());
    }
}

public class RewindInformation
{
    public RewindInformation(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
    }

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
}
