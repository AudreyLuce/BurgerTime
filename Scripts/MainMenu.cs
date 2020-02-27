﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("WorkingFinal");
    }

    public void LoadLore()
    {
        SceneManager.LoadScene("Lore");
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void CallMain()
    {
        SceneManager.LoadScene("Menu");
    }


    public void QuitGame()
    {
        Application.Quit();
    }

}
