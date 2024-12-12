using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;

    }
    #region Tags to be constant 
    
    public const string DART_BULLET = "DartBullet";


    #endregion
    
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> _poolDictionary;


    private void OnEnable()
    {
        _poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {

            var objectPool = new Queue<GameObject>();

            for (var i = 0; i < pool.size; i++)
            {

                var obj = Instantiate(pool.prefab, this.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);

            }
            _poolDictionary.Add(pool.tag, objectPool);
            pool.prefab.SetActive(false);

        }
        foreach (var pool in pools)
        {
            Debug.Log("Object Pools: " + pool.tag);
        }
    }

    public GameObject SpawnFromPool(string poolTag, Vector3 position, Quaternion rotation,Transform parent)
    {
        if (!_poolDictionary.ContainsKey(poolTag))
        {
            Debug.LogWarning("Pool with tag " + poolTag + " doesn't exist.");
            return null;
        }

        var objectToSpawn = _poolDictionary[poolTag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.transform.SetParent(parent);

        return objectToSpawn;

    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {

        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return;
        }

        objectToReturn.SetActive(false);
        _poolDictionary[tag].Enqueue(objectToReturn);

    }

}
