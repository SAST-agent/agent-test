using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    void Start()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene("SC Demo");
    }

    
}
