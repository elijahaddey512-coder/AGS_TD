
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelection : MonoBehaviour
{
    // Tutorial Button - Loads the Tutorial Scene
    public void PlayTutorial()
    {
        SceneManager.LoadScene("tut lvl");
    }

    // Level 1 Button - Displays a message
    public TMP_Text messageText;

    public void Level1Locked()
    {
        messageText.text = "Complete the Tutorial First";
    }
}
