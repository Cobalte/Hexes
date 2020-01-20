using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour {

    public float HideDelay;
    public float HideAnimDuration;
    private Animator anim;
    
    private void Start() {
        anim = GetComponent<Animator>();
        StartCoroutine(WaitForDelay());
    }

    private IEnumerator WaitForDelay() {
        yield return new WaitForSeconds(HideDelay);
        anim.Play("LoadingScreenAnim");
        StartCoroutine(WaitForAnim());
    }
    
    private IEnumerator WaitForAnim() {
        yield return new WaitForSeconds(HideAnimDuration);
        gameObject.SetActive(false);
    }
}
