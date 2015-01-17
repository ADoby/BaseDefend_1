using UnityEngine;
using System.Collections;

/// <summary>
/// Script that is auto attached on the bullet trail after it get's instantiated in the BulletImpactTrail script.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BulletTracer : MonoBehaviour
{
    public LineRenderer Line;
    private float _locationOnPath;
    private bool move;
    [SerializeField]
    private float _duration = 0.1f;
    private float _timeToArrive = 1f;
    private float _alpha = 1f;

    /// <summary>
    /// If the hit was on some object based on the Range.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="muzzlePosition"></param>
    public void OnHitLine(Vector3 destination, Vector3 muzzlePosition)
    {
        _alpha = 1f;
        move = true;
        _timeToArrive = Time.time + _duration;

        Line.SetPosition(0, muzzlePosition);
        Line.SetPosition(1, destination);
    }

    /// <summary>
    /// If the hit was somewhere in space(infinity distance).
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="muzzlePosition"></param>
    public void OnMissLine(Vector3 distance, Vector3 muzzlePosition)
    {
        _alpha = 1f;
        move = true;
        _timeToArrive = Time.time + _duration;

        Line.SetPosition(0, muzzlePosition);
        Line.SetPosition(1, distance);
    }

    void Update()
    {
        if (!move)
            return;
        _locationOnPath = (1.0f - (_timeToArrive - Time.time));

        SetColor();

        if (Time.time < _timeToArrive)
            return;
        move = false;
        vp_Utility.Destroy(gameObject);
    }

    private void SetColor()
    {
        _alpha = Mathf.Lerp(_alpha, 0.0f, _locationOnPath);
        Color color = Line.material.GetColor("_TintColor");
        color.a = _alpha;
        Line.materials[0].SetColor("_TintColor", color);
    }
}