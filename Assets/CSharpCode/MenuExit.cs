using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuExit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void exit()
    {
        SceneManager.LoadScene("SC All Props");
    }
}
