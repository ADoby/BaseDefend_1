using UnityEngine;

[System.Serializable]
public class UIScrollPanel : UIRect
{
    public bool ShowBox = true;
    public string content = "";

    public Rect Padding;

    public GUIStyle BoxStyle;

    public Vector2 scrollPosition = Vector2.zero;

    public GUIStyle HorizontalScrollBarStyle, VerticalScrollBarStyle;

    public float RelHeight = 0f;

    public float Height = 0f;

    public override void DrawMeBeforeChildren()
    {
        if (BoxStyle == null)
            BoxStyle = new GUIStyle(GUI.skin.box);
        if (HorizontalScrollBarStyle == null)
            HorizontalScrollBarStyle = new GUIStyle(GUI.skin.horizontalScrollbar);
        if (VerticalScrollBarStyle == null)
            VerticalScrollBarStyle = new GUIStyle(GUI.skin.verticalScrollbar);

        if (ShowBox)
            GUI.Box(absoluteRect, content, BoxStyle);

        RelHeight = Mathf.Clamp(RelHeight, 1f, 100f);
        Height = absoluteRect.height * RelHeight;

        scrollPosition = GUI.BeginScrollView(
            new Rect(absoluteRect.x + Padding.x, absoluteRect.y + Padding.y, absoluteRect.width + Padding.width, absoluteRect.height + Padding.height), 
            scrollPosition,
            new Rect(absoluteRect.x, absoluteRect.y, absoluteRect.width, Height), 
            false, true);
    }

    public override void DrawMeAfterChildern()
    {
        GUI.EndScrollView();
    }
}