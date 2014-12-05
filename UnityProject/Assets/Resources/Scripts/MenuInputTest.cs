using UnityEngine;
using System.Collections;

public class MenuInputTest : MonoBehaviour {

    void PlayShooter()
    {
        Application.LoadLevel(1);
    }

    void Exit()
    {
        Application.Quit();
    }
}
