using UnityEngine;
using System.Collections;

public class ConditionUI : MonoBehaviour 
{
    public UIRect Panel;
    public UIText conditionText;

    public Vector2 RelPosition
    {
        get
        {
            return conditionText.RelativePosition;
        }
        set
        {
            conditionText.RelativePosition = value;
        }
    }

    public void SetPositionIndex(int index, bool instant = false)
    {
        WantedY = -index * Panel.RelativeSize.y;
        CurrentLerpValue = 0f;

        if (instant)
            Panel.RelativePosition.y = WantedY;
    }
    
    private float WantedY = 0f;

    public float CurrentLerpValue = 0f;

    public float PositionChangeSpeed = 0.03f;

    public float LerpValue
    {
        get
        {
            return Mathf.Min(CurrentLerpValue + PositionChangeSpeed * (Time.deltaTime / 0.016f), 1f);
        }
    }

	void Update () 
    {
        CurrentLerpValue = LerpValue;

        Panel.RelativePosition.y = Mathf.Lerp(Panel.RelativePosition.y, WantedY, Mathf.Pow(CurrentLerpValue, 2));
	}

    void Reset()
    {
        SetPositionIndex(-5, true);
    }
}
