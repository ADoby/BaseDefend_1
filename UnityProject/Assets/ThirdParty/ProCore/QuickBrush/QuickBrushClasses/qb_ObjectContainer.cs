using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
//For the moment, this class is obsolete but remains here for legacy reasons. It may be used in the future to store scene objects. Undo operations will not be handled here, though as this class remains in the scene
//Once the Undo process is upgraded, A removal tool will be included to remove instances of this class and other legacy components from the scene
public class qb_ObjectContainer : MonoBehaviour
{
/*	[SerializeField]
	public GameObject[] sceneObjects = new GameObject[0];
	
	static qb_ObjectContainer instance;
	
	public static qb_ObjectContainer GetInstance()
	{
		if(instance == null)
		{
			//try to find in scene
			qb_ObjectContainer tryInstance = (qb_ObjectContainer)FindObjectOfType(typeof(qb_ObjectContainer));
			
			//if not present, spawn one
			if(tryInstance == null)
			{	
				//tryInstance = EditorUtility.CreateGameObjectWithHideFlags("QB_ObjectContainer", HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable,typeof(qb_ObjectContainer)).GetComponent<qb_ObjectContainer>();
				tryInstance =  new GameObject("QB_ObjectContainer").AddComponent(typeof(qb_ObjectContainer)) as qb_ObjectContainer;
				tryInstance.hideFlags =  HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
			}		
			instance = tryInstance;
		}
		
		return instance;
	}

	public void EraseObjects(List<int> indexList)
	{	

		List<GameObject> removalList = new List<GameObject>();

		foreach(int index in indexList)
		{
			removalList.Add(sceneObjects[index]);
		}

		foreach(GameObject obj in removalList) 
		{
			ArrayUtility.Remove(ref sceneObjects,obj);
			EraseObject(obj);
		}		

	}
	
	public void EraseObject(GameObject obj)
	{
		
		#if UNITY_4_3
			Undo.DestroyObjectImmediate(obj);
		#else
			Undo.RegisterSceneUndo("Erased Object");
			//Undo.RegisterUndo(obj, "Erased Object");
			GameObject.DestroyImmediate(obj);
		#endif
		
	}

	public void VerifyObjects()
	{
		qb_Object[] objs = Object.FindObjectsOfType(typeof(qb_Object)) as qb_Object[];
		sceneObjects = new GameObject[objs.Length];

		for(int i = 0; i < sceneObjects.Length; i++)
		{
			sceneObjects[i] = objs[i].gameObject;
		}
	}
	
	public GameObject[] GetObjects()
	{
		VerifyObjects();
		return sceneObjects;
	}
	
	*/
	/*
	public void VerifySceneObjects()
	{
		List<GameObject> removalList = new List<GameObject>();
		
		for(int i = 0; i < sceneObjects.Length; i++)
		{
			if(sceneObjects[i] == null)
				removalList.Add(sceneObjects[i]);
		}
		
		foreach(GameObject obj in removalList) 
		{
			ArrayUtility.Remove(ref sceneObjects,obj);
		}		
	}
	*/
}