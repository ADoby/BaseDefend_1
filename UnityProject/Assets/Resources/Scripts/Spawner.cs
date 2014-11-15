using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

    public SpawnWeight[] spawnPools;

    WeightedRandomizer<string> randomizer;

    public bool activated = true;

    public float spawnTime = 5.0f;

	// Use this for initialization
	void Start () {
        Dictionary<string, int> weights = new Dictionary<string, int>();
        foreach (var item in spawnPools)
        {
            weights.Add(item.poolName, item.weight);
        }
        randomizer = new WeightedRandomizer<string>(weights);

        StartCoroutine(Spawn());


        //InvokeRepeating("spawny", spawnTime, spawnTime);
        //spawny();
	}

    void spawny()
    {
        string poolName = randomizer.TakeOne();

        GameObject enemie = GameObjectPool.Instance.Spawn(poolName, transform.position, Quaternion.identity);
        enemie.SendMessage("Reset", SendMessageOptions.DontRequireReceiver);

        Debug.Log("Spawning");
    }

    IEnumerator Spawn()
    {
        string poolName = randomizer.TakeOne();

        GameObject enemie = GameObjectPool.Instance.Spawn(poolName, transform.position, Quaternion.identity);
        enemie.SendMessage("Reset", SendMessageOptions.DontRequireReceiver);

        Debug.Log("Spawning");

        yield return new WaitForSeconds(spawnTime);
        StartCoroutine(Spawn());
    }
}

[System.Serializable]
public class SpawnWeight
{
    public string poolName;
    public int weight;
}

public class WeightedRandomizer<T>
{
    private static Random _random = new Random();
    private List<KeyValuePair<T, int>> _weights;

    private int weightSum = 0;

    public WeightedRandomizer(Dictionary<T, int> weights)
    {
        _weights = Sort(weights);

        weightSum = 0;
        foreach (var spawn in _weights)
        {
            weightSum += spawn.Value;
        }
    }

    public T TakeOne()
    {
        // Randomizes a number from Zero to Sum
        int roll = Random.Range(0, weightSum);

        // Finds chosen item based on spawn rate
        T selected = _weights[_weights.Count - 1].Key;
        foreach (var spawn in _weights)
        {
            if (roll < spawn.Value)
            {
                selected = spawn.Key;
                break;
            }
            roll -= spawn.Value;
        }

        // Returns the selected item
        return selected;
    }

    private List<KeyValuePair<T, int>> Sort(Dictionary<T, int> weights)
    {
        var list = new List<KeyValuePair<T, int>>(weights);

        // Sorts the Spawn Rate List for randomization later
        list.Sort(
            delegate(KeyValuePair<T, int> firstPair,
                     KeyValuePair<T, int> nextPair)
            {
                return firstPair.Value.CompareTo(nextPair.Value);
            }
         );

        return list;
    }
}
