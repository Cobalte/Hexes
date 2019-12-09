using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemAutoDestroy : MonoBehaviour {

    private ParticleSystem partSys;

    private void Awake() {
        partSys = GetComponent<ParticleSystem>();
    }

    private void Update() {
        if (!partSys.IsAlive()) {
            Destroy(gameObject);
        }
    }
}
