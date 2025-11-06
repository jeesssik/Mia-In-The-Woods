using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneLoader.LoadScene("Level1");
    }

    public void OpenOptions()
    {
        SceneLoader.LoadScene("Options");
    }

    public void ExitGame()
    {
        SceneLoader.QuitGame();
    }
}
