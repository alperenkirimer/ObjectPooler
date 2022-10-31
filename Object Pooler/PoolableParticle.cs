using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableParticle : MonoBehaviour
{
    private Vector3 scale0;

    private void Awake()
    {
        scale0 = transform.localScale;
        var main = GetComponent<ParticleSystem>().main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    void OnParticleSystemStopped()
    {
        transform.localScale = scale0;
        ObjectPoolManager.Recycle(gameObject);
    }
}
