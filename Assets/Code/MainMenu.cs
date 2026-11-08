
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Play Button - Opens the Level Selection Scene
    public void PlayGame()
    {
        SceneManager.LoadScene("LevelScreen");
    }

    // Credits Button - Opens the Credits Scene
    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }

    // Quit Button - Closes the Game
    public void QuitGame()
    {
        Debug.Log("Game is quitting...");
        Application.Quit();
    }
}

