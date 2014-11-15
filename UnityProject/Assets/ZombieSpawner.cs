using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SpawnInfo
{
    public string Pool = "";
    public float weight = 0f;
}

public class ZombieSpawner : MonoBehaviour 
{

    public float MinSpawnTime = 3.0f;
    public float MaxSpawnTime = 12.0f;

    private float SpawnTimer = 0f;
    public Transform SpawnPoint;


    void Awake()
    {
        if (SpawnPoint == null)
            enabled = false;

    }

	void Start () 
    {
        ResetSpawn();
	}

    public void ResetSpawn()
    {
        SpawnTimer = Random.Range(MinSpawnTime, MaxSpawnTime);
    }

    public SpawnInfo[] spawnInfos;

    public SpawnInfo GetNext()
    {
        var rnd = Random.value;
        for (int i = 0; i < spawnInfos.Length; i++)
        {
            if (rnd < spawnInfos[i].weight)
            {
                return spawnInfos[i];
            }
            rnd -= spawnInfos[i].weight;
        }
        return null;
    }

    public void RandomSpawn()
    {
        SpawnInfo info = GetNext();

        if (info != null)
            GameObjectPool.Instance.Spawn(info.Pool, SpawnPoint.position, Quaternion.identity);

        ResetSpawn();
    }

	// Update is called once per frame
	void Update () 
    {
        SpawnTimer -= Time.deltaTime;
        if (SpawnTimer <= 0)
            RandomSpawn();
	}
}
