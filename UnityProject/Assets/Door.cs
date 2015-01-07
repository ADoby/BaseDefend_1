using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour 
{
    public Collider collider;
    public void Open()
    {
        collider.enabled = false;
    }
    public void Close()
    {
        collider.enabled = true;
    }
}
