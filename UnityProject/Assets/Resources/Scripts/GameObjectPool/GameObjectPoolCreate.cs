using UnityEngine;
using System.Collections;

public class GameObjectPoolCreate : MonoBehaviour {

    public bool PoolCreated = false;

	public GameObject prefab;
	public string poolName;
	public int count;

	public bool useOwnerAsParent = false;

    public Timer TestForPool;

    void Update()
    {
        if(PoolCreated)
            return;
        if (TestForPool.UpdateAutoReset())
        {
            CreatePool();
        }
    }

	// Use this for initialization
	void Awake ()
    {
        CreatePool();
	}

    public void CreatePool()
    {
        if(useOwnerAsParent)
			GameObjectPool.Instance.CreatePool(poolName, prefab, gameObject, count);
		else
			GameObjectPool.Instance.CreatePool(poolName, prefab, null, count);

        if (GameObjectPool.Instance.PoolExists(poolName))
        {
            PoolCreated = true;
        }
    }
}
