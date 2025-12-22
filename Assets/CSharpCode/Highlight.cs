using UnityEngine;

public class HighlightByBool : MonoBehaviour
{
    //原材质
    public Material normalMaterial;
    //高亮材质
    public Material highlightMaterial;   

    private SpriteRenderer sr;


    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
    }
}
