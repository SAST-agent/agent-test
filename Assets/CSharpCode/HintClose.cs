using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintClose : MonoBehaviour
{
    //πÿµÙÃ· æ
    public void close()
    {
        transform.parent.parent.GetComponent<HintManager>().DismissHint();
        transform.parent.gameObject.SetActive(false);
    }
}
