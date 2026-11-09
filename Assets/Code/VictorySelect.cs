using UnityEngine;
using UnityEngine.SceneManagement;

public class VictorySelect : MonoBehaviour
{
    public string mainMenu = "MainMenu";

    public void Retry()
    {
        SceneManager.LoadScene("tut lvl");
    }

    public void Menu()
    {
        SceneManager.LoadScene("LevelScreen");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}