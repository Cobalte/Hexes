using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEditor;

public class HexGridBuilder : MonoBehaviour
{
    public GameObject mCellObj;
    public float mCellSize;
    public int mGridSize;
    private float mObjWidth;

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        mObjWidth = gameObject.GetComponent<RectTransform>().rect.width / Mathf.Sqrt(3);


        mCellSize = mObjWidth / mGridSize;
        float height = Mathf.Sqrt(3) * mCellSize;
        float width = 2 * mCellSize;
        int mGridRadius = mGridSize / 2;


        for (int x = 0; x < mGridSize; x++)
        {
            for (int y = 0; y < mGridSize; y++)
            {
                if (Mathf.Abs(x - mGridRadius + y - mGridRadius) < mGridSize - mGridRadius)
                {
                    float yPos = x * width + (width * y / 2);
                    float xPos = y * height * .75f;

                    GameObject obj = Instantiate(mCellObj);

                    obj.transform.SetParent(gameObject.transform);

                    float mXOffset = height * mGridRadius * .75f;
                    float mYOffset = (width * mGridRadius) + (width * mGridRadius / 2);

                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos - mXOffset, yPos - mYOffset);
                }
            }
        }
    }
}
