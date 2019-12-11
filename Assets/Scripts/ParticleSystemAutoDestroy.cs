using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleSystemAutoDestroy : MonoBehaviour {

    private List<ParticleSystem> particleSystems;

    private void Awake() {
        particleSystems = transform.GetComponentsInChildren<ParticleSystem>().ToList();
    }

    private void Update() {
        if (particleSystems.All(sys => !sys.IsAlive())) {
            Destroy(gameObject);
        }
    }
}
