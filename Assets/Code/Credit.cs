using UnityEngine;
using UnityEngine.SceneManagement;

public class Credit : MonoBehaviour
{
    // Back Button Goes Back to Main Menu
    public void MainMenu()
    {
        SceneManager.LoadScene("Cover Page");
    }

}