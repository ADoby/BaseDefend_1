using UnityEngine;

/// <summary>
/// Script that instantiates a bullet trail object with BulletTracer script attached on it.
/// </summary>
public class BulletImpactTrail : MonoBehaviour
{
    [SerializeField]
    private Transform muzzle = null;
    [SerializeField]
    private BulletTracer lineRenderer = null;
    [SerializeField]
    private float range = 1000f;

    /// <summary>
    /// If the hit was on some object based on the Range.
    /// </summary>
    /// <param name="hit"></param>
    public void OnHit(RaycastHit hit)
    {
        if (!lineRenderer)
            return;

        BulletTracer lineObject = vp_Utility.Instantiate(lineRenderer, muzzle.position, Quaternion.identity) as BulletTracer;
        if (lineObject)
            lineObject.OnHitLine(hit.point, muzzle.position);
    }

    /// <summary>
    /// If the hit was somewhere in space(infinity distance).
    /// </summary>
    /// <param name="ray"></param>
    public void OnMiss(Ray ray)
    {
        if (!lineRenderer)
            return;

        BulletTracer lineObject = vp_Utility.Instantiate(lineRenderer, muzzle.position, Quaternion.identity) as BulletTracer;
        if (lineObject)
            lineObject.OnMissLine(muzzle.position, ray.GetPoint(range));
    }
}