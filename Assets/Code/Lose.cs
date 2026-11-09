using UnityEngine;
using UnityEngine.SceneManagement;

public class Lose : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        //If there are no child then change the scene
        if (transform.childCount < 1)
        {
            SceneManager.LoadScene("Victory 1");
        }
    }
}

