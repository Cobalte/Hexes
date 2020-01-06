using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class ScoreMultiplierPanel : MonoBehaviour
{
    public List<GameObject> mComboPrefabs;
    public GameObject mActiveComboObj;

    private int currentLevel;
    private Image displayImage;

    //--------------------------------------------------------------------------------------------------------
    private void Start()
    {
        currentLevel = 0;
        mActiveComboObj = null;
        CreateComboPrefab();
    }

    //--------------------------------------------------------------------------------------------------------
    public void TryToIncrementLevel()
    {
        if (currentLevel < mComboPrefabs.Count - 1)
        {
            currentLevel++;
            CreateComboPrefab();
        }
    }

    public void CreateComboPrefab()
    {
        if (mActiveComboObj)
        {
            Destroy(mActiveComboObj);
        }
        mActiveComboObj = null;
        mActiveComboObj = Instantiate(mComboPrefabs[currentLevel]);
        mActiveComboObj.transform.SetParent(this.transform);
        mActiveComboObj.transform.localPosition = Vector3.zero;
        mActiveComboObj.transform.localScale = Vector3.one;
        Debug.Log(mComboPrefabs[currentLevel] + " created.");
    }

    //--------------------------------------------------------------------------------------------------------
    public void ResetLevel()
    {
        currentLevel = 0;
        CreateComboPrefab();
    }

    //--------------------------------------------------------------------------------------------------------
    public int GetCurrentMultiplier()
    {
        // returns 2, 4, 8, etc until all sprite are used
        return (int) Math.Pow(2, currentLevel);
    }
}