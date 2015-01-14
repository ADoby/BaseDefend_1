using UnityEngine;
using System.Collections;

public class MenuInputTest : MonoBehaviour {

    void PlayShooter()
    {
        Application.LoadLevel(2);
    }

    void Exit()
    {
        Application.Quit();
    }
}
