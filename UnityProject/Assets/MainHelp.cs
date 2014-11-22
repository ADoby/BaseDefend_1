using UnityEngine;
using System.Collections;

public class MainHelp : MonoBehaviour {

    public GUIStyle style;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(5, 20, 200f, 200f), style: style);
        GUILayout.Label("This is a test level :P\n" +
            "You can walk (W,A,S,D)\n, run (shift)\n, jump (space)\n, shoot (left mouse button)\n, switch weapons (Q,E)\n" +
            "It will auto jump when running towards jumpable obstacles\n" +
            "Weapons have forces which can bring down enemies\n" + 
            "Zombie guy can kill you :P");
        GUILayout.EndArea();
    }
}
