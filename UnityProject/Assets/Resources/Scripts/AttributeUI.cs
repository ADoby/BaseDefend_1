using UnityEngine;
using System.Collections;

public class AttributeUI : MonoBehaviour {

    

    public UIText AttributeText;
    public UIText AttributeDescription;
    public AttribInfo.Attribute type;

    void Awake()
    {
        Events.Instance.Register(this);
    }

    public void Minus()
    {
        Events.Instance.AttributeMinus.Send(type);
        UpdateText();
    }
    public void Pluss()
    {
        Events.Instance.AttributePluss.Send(type);
        UpdateText();
    }

    public void UpdateText()
    {
        if(AttributeText!= null)
            AttributeText.Text = Game.Instance.GetAttributeText(type);
        if (AttributeDescription != null)
            AttributeDescription.Text = Game.Instance.GetAttributeDescription(type);
    }

    void OnMessage_UIStateChanged(GameUI.State newState)
    {
        if (newState == GameUI.State.SHOPBASE || newState == GameUI.State.SHOPENEMIES || newState == GameUI.State.SHOPPLAYER)
        {
            UpdateText();
        }
    }
}
