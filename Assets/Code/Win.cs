using UnityEngine;
using UnityEngine.SceneManagement;
public class Win : MonoBehaviour
{
    

    void Update()
    {
        //If there are no child then change the scene
        if (transform.childCount < 1)
        {
            SceneManager.LoadScene("Victory");
        }
    }
}




