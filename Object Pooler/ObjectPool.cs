using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPool
{
    public GameObject Prefab;

    public GameObject PoolParent;

    public HashSet<GameObject> InstancesInUse = new HashSet<GameObject>();
    public List<GameObject> InstancesInReserve = new List<GameObject>();

    public int Count = 50;

    [Tooltip("If the object has a Particle System and it is expected to be recycled when the animation ends.")]
    public bool IsParticle;

    [Tooltip("More instances will be automatically populated if the count in reserve falls under a threshold.")]
    public bool AllowAutoIncrease = false;

    [Range(1, 99)]
    public int AutoIncreaseThresholdPercentage = 25;
    public int AutoIncreaseThresholdCount;

    [Range(0.1f, 1f)]
    public float AutoIncreaseCoefficient = 0.5f;
    public int AutoIncreaseCount;

    [Tooltip("Auto increase will not exceed this population count.")]
    public int MaxInstances = 200;

    // Inspector GUI variables

    [HideInInspector] public bool IsExpandedOnInspectorGUI = true;

    public void UpdateAutoIncreaseProperties()
    {
        AutoIncreaseThresholdCount = (int)(Count * (AutoIncreaseThresholdPercentage / 100f));
        AutoIncreaseCount = (int)(Count * AutoIncreaseCoefficient);
    }
}