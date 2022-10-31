using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager manager;

    public List<ObjectPool> ObjectPoolList = new List<ObjectPool>();
    public InitalizationMode InitMode = InitalizationMode.AWAKE;
    public bool AllowLogs;
    
    public bool IsInitialized { get; private set; }

    private Dictionary<GameObject, ObjectPool> allInstances = new Dictionary<GameObject, ObjectPool>();
    private Dictionary<GameObject, ObjectPool> poolPrefabs = new Dictionary<GameObject, ObjectPool>();

    private List<ObjectPool> autoIncreasePools;

    // Inspector GUI variables

    public bool ShowSummaryOnInspectorGUI;

    public enum InitalizationMode
    {
        AWAKE,
        START,
        MANUAL
    }

    private void Awake()
    {
        if (manager != null && manager != this)
        {
            Debug.LogWarning("There is already an Object Pool Manager in scene");
            Destroy(this);
            return;
        }

        manager = this;

        if (InitMode == InitalizationMode.AWAKE) 
            InitializeObjectPoolManager();
    }

    private void Start()
    {
        if (InitMode == InitalizationMode.START) 
            InitializeObjectPoolManager();
    }

    public static void InitializeObjectPoolManager()
    {
        if (manager == null) return;
        if (manager.IsInitialized)
        {
            Log("Object Pool Manager is already initialized");
            return;
        }

        manager.IsInitialized = true;

        foreach (ObjectPool objectPool in manager.ObjectPoolList)
        {
            manager.InitializeObjectPool(objectPool);
        }

        manager.autoIncreasePools = manager.ObjectPoolList.Where(objectPool => objectPool.AllowAutoIncrease == true).ToList();

        if (manager.autoIncreasePools.Count > 0)
        {
            manager.StartCoroutine(manager.AutoIncreaseWatcher());
            Log("Auto Increase Watcher started");
        }

        Log("Pool is initialized");
    }
 
    private void InitializeObjectPool(ObjectPool objectPool)
    {
        var prefab = objectPool.Prefab;

        if (prefab == null)
        {
            Log("Skipped Initialization: Prefab is null");
            return;
        }

        if(CheckPrefabExistsInPool(objectPool.Prefab))
        {
            Log("Skipped Initialization: There is already an Object Pool for: " + objectPool.Prefab.name);
            return;
        }

        if (objectPool.IsParticle == true && prefab.GetComponent<ParticleSystem>() == null)
        {
            objectPool.IsParticle = false;
        }

        objectPool.PoolParent = new GameObject(prefab.name + " Pool");
        objectPool.PoolParent.transform.SetParent(transform);

        poolPrefabs.Add(objectPool.Prefab, objectPool);

        AddNewInstances(objectPool, objectPool.Count);

        Log("Initialized Object Pool for: " + objectPool.Prefab.name);
    }

    private void AddNewInstances(ObjectPool objectPool, int count)
    {
        GameObject prefab = objectPool.Prefab;

        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(prefab);

            if (objectPool.IsParticle)
                instance.AddComponent<PoolableParticle>();

            allInstances.Add(instance, objectPool);
            SendToReserve(instance);
        }
    }

    private IEnumerator AutoIncreaseWatcher()
    {
        foreach (ObjectPool objectPool in autoIncreasePools)
        {
            objectPool.UpdateAutoIncreaseProperties();
        }

        while (true)
        {
            foreach (ObjectPool objectPool in autoIncreasePools)
            {
                if (objectPool.Count >= objectPool.MaxInstances) continue;

                int instanceCountInReserve = GetCountInReserveByPrefab(objectPool.Prefab);

                if (instanceCountInReserve < objectPool.AutoIncreaseThresholdCount)
                {
                    objectPool.Count = Mathf.Min(objectPool.Count + objectPool.AutoIncreaseCount, objectPool.MaxInstances);
                    objectPool.UpdateAutoIncreaseProperties();

                    int countDifference = objectPool.Count - GetCountTotalByPrefab(objectPool.Prefab);

                    if (countDifference < 1) continue;

                    Log(
                       "Auto Increase for: " + objectPool.Prefab.name +
                       ", Left In Reserve: " + instanceCountInReserve +
                       ", Going to create " + countDifference + " new instances"
                       );

                    while (countDifference > 0 && GetCountTotalByPrefab(objectPool.Prefab) < objectPool.MaxInstances)
                    {
                        countDifference--;
                        AddNewInstances(objectPool, 1);
                        yield return null;
                    }
                }
            }

            yield return new WaitForSeconds(1);
        }
    }

    public static GameObject Give(GameObject prefab) => Spawn(prefab, null, Vector3.zero, Quaternion.identity);
    public static GameObject Give(GameObject prefab, Transform parent) => Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
    public static GameObject Give(GameObject prefab, Vector3 position, Quaternion rotation) => Spawn(prefab, null, position, rotation);
    public static GameObject Give(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent) => Spawn(prefab, parent, position, rotation);
    
    private static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        if (manager.CheckPrefabExistsInPool(prefab))
        {
            GameObject instance;

            if (manager.GetCountInReserveByPrefab(prefab) > 0)
            {
                List<GameObject> instances = manager.GetInstancesInReserveByPrefab(prefab);
                instance = instances[0];
                instances.RemoveAt(0);
                manager.GetInstancesInUseByPrefab(prefab).Add(instance);
            }
            else
            {
                Log("Warning: There is no " + prefab.name + " left in reserve, creating a new instance");
                manager.AddNewInstances(manager.GetObjectPoolByPrefab(prefab), 1);
                instance = Give(prefab);
            }

            instance.SetActive(true);
            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.rotation = rotation;

            return instance;
        }
        else
        {
            Log("Warning: Creating a " + prefab.name + ". Did you forget to define in Object Pool Manager?");
            return Instantiate(prefab, position, rotation, parent);
        }
    }

    public static void Recycle(GameObject gameObj)
    {
        if (manager.CheckInstanceExistsInPool(gameObj))
        {
            manager.GetInstancesInUseByInstance(gameObj).Remove(gameObj);
            manager.SendToReserve(gameObj);
        }
        else
        {
            Log("Warning: Cannot recyle + " + gameObj.name + ", destroying it instead. Did you forget to define in Object Pool Manager?");
            Destroy(gameObj);
        }
    }

    private void SendToReserve(GameObject gameObj)
    {      
        if(IsInstanceInReserve(gameObj) == false)
        {
            GetInstancesInReserveByInstance(gameObj).Add(gameObj);
        }

        gameObj.transform.SetParent(GetObjectPoolByInstance(gameObj).PoolParent.transform);
        gameObj.SetActive(false);
    }

    private List<GameObject> GetInstancesInReserveByInstance(GameObject gameObj) => allInstances[gameObj].InstancesInReserve;
    private List<GameObject> GetInstancesInReserveByPrefab(GameObject prefab) => poolPrefabs[prefab].InstancesInReserve;
    private List<GameObject> GetInstancesInUseByInstance(GameObject gameObj) => allInstances[gameObj].InstancesInUse;
    private List<GameObject> GetInstancesInUseByPrefab(GameObject prefab) => poolPrefabs[prefab].InstancesInUse;
    public int GetCountInReserveByInstance(GameObject gameObj) => GetInstancesInReserveByInstance(gameObj).Count;
    public int GetCountInReserveByPrefab(GameObject prefab) => GetInstancesInReserveByPrefab(prefab).Count;
    public int GetCountInUseByInstance(GameObject gameObj) => GetInstancesInUseByInstance(gameObj).Count;
    public int GetCountInUseByPrefab(GameObject prefab) => GetInstancesInUseByPrefab(prefab).Count;
    public int GetCountTotalByInstance(GameObject gameObj) => GetCountInReserveByInstance(gameObj) + GetCountInUseByInstance(gameObj);
    public int GetCountTotalByPrefab(GameObject prefab) => GetCountInReserveByPrefab(prefab) + GetCountInUseByPrefab(prefab);
    public ObjectPool GetObjectPoolByInstance(GameObject gameObj) => allInstances[gameObj];
    public ObjectPool GetObjectPoolByPrefab(GameObject prefab) => poolPrefabs[prefab];
    private bool CheckPrefabExistsInPool(GameObject prefab) => poolPrefabs.ContainsKey(prefab);
    private bool CheckInstanceExistsInPool(GameObject gameObj) => allInstances.ContainsKey(gameObj);
    public bool IsInstanceInReserve(GameObject gameObj) => gameObj.transform.parent == GetObjectPoolByInstance(gameObj).PoolParent;

    private static void Log(string message)
    {
        if (manager == null) return;
        if (!manager.AllowLogs) return;

        Debug.Log("[Object Pool] " + message);
    }
}