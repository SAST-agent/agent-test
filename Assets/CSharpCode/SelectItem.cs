using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectItem : MonoBehaviour
{
    public GameObject itemBar;
    private int buttonCount = 0;

    //void Start()
    //{
    //    if (itemBar == null)
    //    {
    //        Debug.LogError("[SelectItem] itemBar 没有绑定");
    //        return;
    //    }
    //    //切换按钮点击模式
    //    for (int i = 0; i < 2; i++)
    //    {
    //        GameObject evidenceSet = itemBar.transform.GetChild(i).gameObject;
    //        buttonCount = evidenceSet.transform.childCount;
    //        for (int j = 0; j < buttonCount; j++)
    //        {
    //            item_InBar button = evidenceSet.transform.GetChild(j).GetComponent<item_InBar>();
    //            if (button == null)
    //            {
    //                Debug.LogWarning(
    //                    $"[SelectItem]上没有 item_InBar 组件"
    //                );
    //                continue;
    //            }
    //            button.changeMode();
    //        }
    //    }

    //}
    void Start()
    {

        
    }

    void OnEnable()
    {
        for (int i = 1; i < itemBar.transform.childCount; i++)
        {
            Transform evidenceSet = itemBar.transform.GetChild(i);

            foreach (Transform child in evidenceSet)
            {
                var item = child.GetComponent<item_InBar>();
                if (item == null) continue;

                item.changeMode(1);
            }
        }
    }



    void Update()
    {
        
    }

}
