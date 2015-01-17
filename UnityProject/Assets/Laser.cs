using UnityEngine;
using System.Collections;

public class Laser : vp_FPWeaponShooter
{
    public Transform LineStart;
    public LineRenderer lineRenderer;

    public float Range = 20f;

    public float MinDamage = 1f, MaxDamage = 5f;

    public Timer AttackSpeed;
    public Timer LifeTimer;
    public Timer BlendOutTimer;

    public Color StartShooting, EndShooting;

    private Color StartHidden, EndHidden;

    public AudioClip start, loop, end;

    public AudioSource audioSource;

    public PlayerAmmiRegeneration regenScript;

    private bool shooting = false;

    protected override void Awake()
    {
        base.Awake();
        StartHidden = StartShooting;
        StartHidden.a = 0f;
        EndHidden = EndShooting;
        EndHidden.a = 0f;

        audioSource.clip = loop;
        shooting = true;
        DeactivateLaser();
    }

	// Update is called once per frame
	void Update ()
    {
        if (!shooting && !lineRenderer.enabled)
            return;

        //lineRenderer.SetPosition(0, LineStart.position);

        if (LifeTimer.Update())
        {
            DeactivateLaser();
        }
        else
        {
            UpdateLineRendererColors(LifeTimer.Procentage);
            if (AttackSpeed.Update())
            {
                AttackSpeed.Reset();
                DoShoot(1f - LifeTimer.Procentage);
            }
        }
	}

    private void DoShoot(float lerpValue)
    {
        if (ProjectilePrefab == null)
            return;

        // will only trigger on local player in multiplayer
        if (m_SendFireEventToNetworkFunc != null)
            m_SendFireEventToNetworkFunc.Invoke();

        //m_CurrentFirePosition = GetFirePosition();
        m_CurrentFirePosition = LineStart.position;
        m_CurrentFireRotation = GetFireRotation();
        m_CurrentFireSeed = GetFireSeed();

        GameObject p = null;
        p = (GameObject)vp_Utility.Instantiate(ProjectilePrefab, m_CurrentFirePosition, m_CurrentFireRotation);

        vp_HitscanBullet bullet = p.GetComponent<vp_HitscanBullet>();
        if (bullet)
        {
            bullet.SetSender(Transform);
            bullet.Damage = Mathf.Lerp(MinDamage, MaxDamage, lerpValue);
        }

        p.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);	// preset defined scale

        //SetSpread(m_CurrentFireSeed, p.transform);

        //laser
        lineRenderer.SetPosition(0, m_CurrentFirePosition);

        Ray ray = new Ray(p.transform.position, p.transform.forward);
		RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Range, vp_Layer.Mask.BulletBlockers))
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, m_CurrentFirePosition + p.transform.forward * Range);
        }

        lineRenderer.enabled = true;
    }
    private void UpdateLineRendererColors(float lerpValue)
    {
        lineRenderer.SetColors(Color.Lerp(StartShooting, StartHidden, lerpValue), Color.Lerp(EndShooting, EndHidden, lerpValue));
    }

    public void DeactivateLaser()
    {
        LifeTimer.Finish();
        UpdateLineRendererColors(LifeTimer.Procentage);
        if (shooting)
        {
            shooting = false;
            lineRenderer.enabled = false;

            audioSource.Stop();
            audioSource.PlayOneShot(end);
        }
        if (regenScript) regenScript.enabled = true;
    }

    public override void Activate()
    {
        base.Activate();
        if(regenScript) regenScript.enabled = true;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        //DeactivateLaser();
        if(regenScript) regenScript.enabled = false;
    }
    public void StartShoot()
    {
        LifeTimer.Reset();
        UpdateLineRendererColors(LifeTimer.Procentage);

        if (regenScript) regenScript.enabled = false;

        if (!shooting)
        {
            shooting = true;
            audioSource.PlayOneShot(start);
            audioSource.PlayDelayed(start.length);
        }
    }

    public override void DisableFiring(float seconds = 10000000)
    {
        base.DisableFiring(seconds);
        //DeactivateLaser();
    }

    protected override void SpawnProjectiles()
    {
        StartShoot();
    }
}
