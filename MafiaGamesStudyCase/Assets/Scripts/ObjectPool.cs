using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Queue<GameObject> pooledObjects;
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private int poolSize;

    private void Awake()
    {

        pooledObjects = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objectPrefab);
            obj.SetActive(false);
            pooledObjects.Enqueue(obj);
        }
        
    }

    public GameObject GetPooledObject(int objectType)
    {
        GameObject obj = null;
        if (pooledObjects.Count != 0)
        {
            obj =  pooledObjects.Dequeue();
            obj.SetActive(true);
        }
        //pooledObjects.Enqueue(obj);
        return obj;
    }
    public void AddPooledObject(GameObject obj)
    {
        obj.SetActive(false);
        pooledObjects.Enqueue(obj);
    }
}