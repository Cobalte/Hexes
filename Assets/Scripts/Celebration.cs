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
        if (allSystems.All(sys => !sys.IsAlive())) {
            Destroy(gameObject);
        }
    }
    
}
