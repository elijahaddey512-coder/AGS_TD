
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;

    private bool isPaused = false;

    private float previousTimeScale;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisibility;

    private void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        isPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        previousTimeScale = Time.timeScale;
        previousCursorLock = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        Time.timeScale = previousTimeScale;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisibility;

        Debug.Log("Game Resumed");
    }

    public void QuitToCoverPage()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Cover Page");
    }
}


