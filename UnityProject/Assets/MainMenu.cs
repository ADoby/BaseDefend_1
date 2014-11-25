using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

    void OnGUI()
    {
        if (GUILayout.Button("Shooter, SUPER !!"))
            Application.LoadLevel(1);
        if (GUILayout.Button("Robot_Test, Broken sory ;("))
            Application.LoadLevel(2);
    }
}
