using UnityEngine;
using System.Collections;

public class BaseHealthView : MonoBehaviour 
{

	public UIRect bar;
	public UIText text;

	[Range(0.001f, 1.0f)]
	public float Speed = 0.1f;

	private float currentValue = 0f;
	private float wantedValue = 1.0f;

	public float minDiff = 0.005f;


	// Use this for initialization
	void Start () 
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
		if (!TransitionFinished)
		{
			float delta = Time.deltaTime / 0.016f;
			currentValue = Mathf.Lerp(currentValue, wantedValue, Mathf.Min(delta * Speed, 1f));

			if (Mathf.Abs(currentValue - wantedValue) < minDiff)
				currentValue = wantedValue;

			bar.RelativeSize.y = currentValue;
			text.Text = System.String.Format("{0:###%}", currentValue);
		}
	}

	void OnMessage_BaseHealthChanged(HealthHandler handler)
	{
		wantedValue = handler.Procentage;
	}
}
