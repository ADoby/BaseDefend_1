using UnityEngine;
using System.Collections;

public class BackToMenu : MonoBehaviour
{

    void OnGUI()
    {
        if (GUILayout.Button("BackToMenu"))
            Application.LoadLevel(0);
    }
}
