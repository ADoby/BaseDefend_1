using UnityEngine;

[System.Serializable]
public class Timer {

	public float Value = 0f;
	public float timer = 0f;

	public float CurrentTime
	{
		get
		{
			return timer;
		}
	}

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

	public bool Update ()
	{
        return Add(Time.deltaTime);
	}
    public bool UpdateAutoReset()
    {
        return AddAutoReset(Time.deltaTime);
    }

    public bool Add(float amount)
    {
        timer = Mathf.Min(timer + amount, Value);
        return Finished;
    }
    public bool AddAutoReset(float amount)
    {
        if(Add(amount))
        {
            Reset();
            return true;
        }
        return false;
    }
    
	public float Procentage
	{
		get
		{
			if (Value == 0)
				return 1f;
			return Mathf.Clamp01(timer / Value);
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
