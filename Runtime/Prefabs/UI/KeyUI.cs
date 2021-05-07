using ClassicFPS.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyUI : MonoBehaviour
{
    [Header("UI")]
    public  RectTransform keyElementPrefab;
    public int horizontalPadding;
    public int horizontalLength;

    public void UpdateUI (List<string> keys)
    {   
        for (int t = 0; t < transform.childCount; t++)
        {
            GameObject.Destroy(transform.GetChild(t).gameObject);
        }

        for (int i = 0; i < keys.Count; i++)
        {
            RectTransform t = Instantiate< RectTransform>(keyElementPrefab);
            

            var newPos = this.GetComponent<RectTransform>().position;
            newPos.x += horizontalLength*i + horizontalPadding*i;

            t.position = newPos;
            
            t.GetComponent<UnityEngine.UI.Image>().sprite = GameManager.keySettings[keys[i]].keySprite;
            t.SetParent(this.transform);
        }
    }
}
