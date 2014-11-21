using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

    void OnGUI()
    {
        if (GUILayout.Button("Shooter"))
            Application.LoadLevel(1);
        if (GUILayout.Button("Robot_Test"))
            Application.LoadLevel(2);
    }
}
