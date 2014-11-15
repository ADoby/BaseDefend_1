using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class qb_PrefabObject
{
	public qb_PrefabObject(UnityEngine.Object prefab)
	{
		this.prefab = prefab;
	}
	
	public qb_PrefabObject(UnityEngine.Object prefab, float weight)
	{
		this.prefab = prefab;
		this.weight = weight;
	}
	
	public UnityEngine.Object	prefab;
	public Texture2D 			iconTexture;
	public float				weight = 0.1f;
}
