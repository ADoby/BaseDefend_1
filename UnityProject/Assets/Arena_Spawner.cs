using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyTypeWeightedSpawnInfo
{
	public Game.EnemyType enemyType;
	public float Weight = 0f;
}

[System.Serializable]
public class EnemyTypeSortedSpawnInfo
{
	public List<EnemyTypeWeightedSpawnInfo> weightedRandomSpawnInfo = new List<EnemyTypeWeightedSpawnInfo>();
	public int Amount = 0;
	public Timer WaitAfterThisSpawn;
	private int spawned = 0;
	public int Spawned
	{
		get
		{
			return spawned;
		}
		private set
		{
			spawned = value;
		}
	}

	public bool IsLastSpawn
	{
		get
		{
			return Amount >= 0 && spawned == Amount;
		}
	}

	public void Next()
	{
		if (IsLastSpawn)
			return;
		Randomize();
		spawned++;
	}

	private float WeightFunc(EnemyTypeWeightedSpawnInfo value)
	{
		return value.Weight;
	}
	private void Randomize()
	{
		CurrentInfo = weightedRandomSpawnInfo.RandomEntry(WeightFunc);
	}

	private EnemyTypeInfo currentInfo = null;
	public EnemyTypeInfo CurrentInfo
	{
		get
		{
			return currentInfo;
		}
		private set
		{
			currentInfo = value;
		}
	}

	public void Reset()
	{
		Spawned = 0;
	}
}

public class Arena_Spawner : MonoBehaviour 
{
	public LevelPart levelPart;

	public List<EnemyTypeSortedSpawnInfo> sortedSpawnInfo = new List<EnemyTypeSortedSpawnInfo>();

	void OnEnable()
	{
		Listen();
	}
	void OnDisable()
	{
		UnListen();
	}

	public void Listen()
	{
		levelPart.OnPartStart -= LevelPartStart;
		levelPart.OnPartStop -= LevelPartStop;
		levelPart.OnPartStart += LevelPartStart;
		levelPart.OnPartStop += LevelPartStop;
	}
	public void UnListen()
	{
		levelPart.OnPartStart -= LevelPartStart;
		levelPart.OnPartStop -= LevelPartStop;
	}

	public void LevelPartStart()
	{
		foreach (var item in sortedSpawnInfo)
		{
			item.Reset();
		}

		Spawning = true;
	}

	public void LevelPartStop()
	{
		Spawning = false;
	}

	public bool Spawning = false;

	void Update () 
	{
		if (!Spawning)
			return;

		
	}
}
