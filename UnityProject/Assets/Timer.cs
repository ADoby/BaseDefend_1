using UnityEngine;

[System.Serializable]
public class Timer {

    public float Value = 0f;
    private float timer = 0f;

	// Use this for initialization
	void Start () 
    {
        timer = 0f;
	}
    public void Reset()
    {
        timer = 0f;
    }
    public void Finish()
    {
        timer = Value;
    }
	// Update is called once per frame
	public bool Update ()
    {
        timer = Mathf.Min(timer + Time.deltaTime, Value);
        return Finished;
	}
    public float Procentage
    {
        get
        {
            if (Value == 0)
                return 0;
            return timer / Value;
        }
    }
    public bool Finished
    {
        get
        {
            return Procentage == 1;
        }
    }
}
