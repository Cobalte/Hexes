using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Celebration : MonoBehaviour {

    public ParticleSystem SparkleParticles;
    
    private List<ParticleSystem> allSystems;

    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        allSystems = transform.GetComponentsInChildren<ParticleSystem>().ToList();
    }

    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        // plain loop instead of linq - this runs every frame
        foreach (ParticleSystem system in allSystems) {
            if (system.IsAlive()) {
                return;
            }
        }

        // all done - hand this effect back to the pool for reuse
        PrefabPool.Release(gameObject);
    }
    
}
