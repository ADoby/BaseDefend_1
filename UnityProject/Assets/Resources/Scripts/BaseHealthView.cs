using UnityEngine;
using System.Collections;

public class BaseHealthView : MonoBehaviour 
{

	public UIRect bar;
	public UIText text;

    public BaseShield shield1, shield2;
    public UIRect shieldRect1, shieldRect2;

	[Range(0.001f, 1.0f)]
	public float Speed = 0.1f;

	private float currentValue = 0f;
	private float wantedValue = 1.0f;

	public float minDiff = 0.005f;


	// Use this for initialization
    void Awake()
	{
		Data.Instance.Register(this);
	}

	private bool TransitionFinished
	{
		get
		{
			return currentValue == wantedValue;
		}
	}

	// Update is called once per frame
	void Update ()
    {
        float delta = Time.deltaTime / 0.016f;
		if (!TransitionFinished)
		{
			currentValue = Mathf.Lerp(currentValue, wantedValue, Mathf.Min(delta * Speed, 1f));

			if (Mathf.Abs(currentValue - wantedValue) < minDiff)
				currentValue = wantedValue;

			bar.RelativeSize.x = currentValue;
			text.Text = System.String.Format("{0:###%}", currentValue);
		}

        shieldRect1.RelativeSize.x = Mathf.Lerp(shieldRect1.RelativeSize.x, shield1.Procentage, Mathf.Min(delta * Speed, 1f));
        shieldRect2.RelativeSize.x = Mathf.Lerp(shieldRect2.RelativeSize.x, shield2.Procentage, Mathf.Min(delta * Speed, 1f));
	}

	void OnMessage_BaseHealthChanged(HealthHandler handler)
	{
		wantedValue = handler.Procentage;
	}
}
