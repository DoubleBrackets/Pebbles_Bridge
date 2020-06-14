using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager mainMenuManager;

    private void Awake()
    {
        mainMenuManager = this;
    }
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
