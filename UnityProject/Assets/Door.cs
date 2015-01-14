using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour 
{
    public Collider collider;
    public MeshRenderer rendering;
    public void Open()
    {
        collider.enabled = false;
        rendering.enabled = false;
    }
    public void Close()
    {
        collider.enabled = true;
        rendering.enabled = true;
    }
}
