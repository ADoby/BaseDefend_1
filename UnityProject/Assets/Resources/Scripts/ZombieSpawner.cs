using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SpawnInfo
{
    public Game.EnemyType type;
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
        ResetSpawn();
    }

	void Start () 
    {
        ResetSpawn();
	}

    public void ResetSpawn()
    {
        SpawnTimer = Random.Range(MinSpawnTime, MaxSpawnTime);
    }

    public void RandomSpawn()
    {
        Game.TrySpawnEnemy(SpawnPoint.position, Quaternion.identity);

        ResetSpawn();
    }

	// Update is called once per frame
	void Update () 
    {
        SpawnTimer -= Game.EnemyDeltaTime;
        if (SpawnTimer <= 0)
            RandomSpawn();
	}
}
