using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombineTutorialGuide : MonoBehaviour {

    public TextMeshProUGUI TextObj;
    
    public bool IsActive => gameObject.activeSelf;
    public bool IsWaiting => obj1Set && !obj2Set;

    private GameObject block1;
    private GameObject block2;
    private bool obj1Set;
    private bool obj2Set;
    private RectTransform textRect;
    
    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        textRect = TextObj.GetComponent<RectTransform>();
    }

    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        if (!obj1Set || !obj2Set) {
            return;
        }

        Vector3 pos1 = block1.transform.position;
        Vector3 pos2 = block2.transform.position;

        // just fuck me up
        float dist = Vector3.Distance(pos1, pos2);
        float r = Mathf.Max(dist * 0.8f, 0.2f);
        float a = dist * dist / (2 * dist);
        float h = Mathf.Sqrt(r * r - a * a);
        float finalX = pos1.x + a * (pos2.x - pos1.x) / dist;
        float finalY = pos1.y + a * (pos2.y - pos1.y) / dist;

        Vector3 opt1 = new Vector3(
            x: finalX + h * (pos2.y - pos1.y) / dist,
            y: finalY - h * (pos2.x - pos1.x) / dist,
            z: pos1.z);
        Vector3 opt2 = new Vector3(
            x: finalX - h * (pos2.y - pos1.y) / dist,
            y: finalY + h * (pos2.x - pos1.x) / dist,
            z: pos1.z);

        transform.position = Vector3.Distance(opt1, Vector3.zero) < Vector3.Distance(opt2, Vector3.zero) ? opt1 : opt2;
        TextObj.alignment = finalX > 0f ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void ResetAndHide() {
        obj1Set = false;
        obj2Set = false;
        gameObject.SetActive(false);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void TrackObject1(GameObject obj) {
        block1 = obj;
        obj1Set = true;
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void TrackObject2(GameObject obj) {
        block2 = obj;
        obj2Set = true;
        gameObject.SetActive(true);
    }
}
